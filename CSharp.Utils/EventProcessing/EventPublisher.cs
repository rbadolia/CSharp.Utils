using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using CSharp.Utils.Collections.Concurrent;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.EventProcessing.Contracts;
using CSharp.Utils.Text.RegularExpressions;
using CSharp.Utils.Validation;

namespace CSharp.Utils.EventProcessing
{
    public sealed class EventPublisher
    {
        private static readonly EventPublisher _instance = new EventPublisher();

        private IList<Triplet<string, EventHandler<EventArg>, Regex>> _notityInSameTransactionCallbacks;

        private IList<Triplet<string, EventHandler<EventArg>, Regex>> _notityInNotSameTransactionCallbacks;

        private Dictionary<CommandScope, IList<KeyValuePair<IEvent, bool>>> _transactionalDomainEvents;

        private ReaderWriterLockSlim _slimForPostTransactionPublish = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private EventPublisher()
        {
            this._notityInSameTransactionCallbacks = new List<Triplet<string, EventHandler<EventArg>, Regex>>();
            this._notityInSameTransactionCallbacks = new SafeListDecorator<Triplet<string, EventHandler<EventArg>, Regex>>(this._notityInSameTransactionCallbacks);

            this._notityInNotSameTransactionCallbacks = new List<Triplet<string, EventHandler<EventArg>, Regex>>();
            this._notityInNotSameTransactionCallbacks = new SafeListDecorator<Triplet<string, EventHandler<EventArg>, Regex>>(this._notityInNotSameTransactionCallbacks);

            this._transactionalDomainEvents = new Dictionary<CommandScope, IList<KeyValuePair<IEvent, bool>>>();
            CommandScope.AfterComplete += this.CommandScope_AfterComplete;
            CommandScope.AfterDispose += this.CommandScope_AfterDispose;
        }

        public static EventPublisher Instance
        {
            get { return _instance; }
        }

        public void PublishEvent(IEvent eventObject, bool isRemoteEvent = false)
        {
            Guard.ArgumentNotNull(eventObject, "eventObject");

            var commandScope = CommandScope.Current;
            if (commandScope == null)
            {
                return;
            }

            try
            {
                this._slimForPostTransactionPublish.EnterWriteLock();
                IList<KeyValuePair<IEvent, bool>> eventKvps;
                if (!this._transactionalDomainEvents.TryGetValue(commandScope, out eventKvps))
                {
                    eventKvps = new List<KeyValuePair<IEvent, bool>>();
                    eventKvps = new SafeListDecorator<KeyValuePair<IEvent, bool>>(eventKvps);
                    this._transactionalDomainEvents.Add(commandScope, eventKvps);
                }

                eventKvps.Add(new KeyValuePair<IEvent, bool>(eventObject, isRemoteEvent));
            }
            finally
            {
                this._slimForPostTransactionPublish.ExitWriteLock();
            }

            this.PublishEvents(eventObject, true, isRemoteEvent);
        }

        public IDisposable Subscribe(string subjectWildcard, EventHandler<EventArg> notificationCallback, 
            bool notityInSameTransaction = false)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(subjectWildcard, "subjectWildcard");
            Guard.ArgumentNotNull(notificationCallback, "notificationCallback");
            var regex = RegexHelper.BuildRegexFromWildcard(subjectWildcard, false);
            var triplet = new Triplet<string, EventHandler<EventArg>, Regex>(subjectWildcard, notificationCallback, 
                regex);
            var list = notityInSameTransaction
                ? this._notityInSameTransactionCallbacks
                : this._notityInNotSameTransactionCallbacks;

            list.Add(triplet);
            return new Unsubscribe(triplet, list);
        }

        public IDisposable Subscribe(EventHandler<EventArg> notificationCallback, 
            bool notityInSameTransaction = false, params string[] subjectWildcards)
        {
            Guard.ArgumentNotNull(subjectWildcards, "subjectWildcards");
            foreach (var subjectWildcard in subjectWildcards)
            {
                Guard.ArgumentNotNullOrEmptyOrWhiteSpace(subjectWildcard, "subjectWildcard");
            }

            var unsubscribeMiltiple = new UnsubscribeMiltiple();
            foreach (var subjectWildcard in subjectWildcards)
            {
                var disposable = this.Subscribe(subjectWildcard, notificationCallback, notityInSameTransaction);
                unsubscribeMiltiple.Disposables.Add(disposable);
            }

            return unsubscribeMiltiple;
        }

        private void PublishEvents(IEvent eventObject, bool isNotifyingInSameTransaction, bool isRemoteEvent)
        {
            var list = isNotifyingInSameTransaction
                ? this._notityInSameTransactionCallbacks
                : this._notityInNotSameTransactionCallbacks;
            var arg = new EventArg(eventObject, isNotifyingInSameTransaction, isRemoteEvent);
            foreach (var triplet in list)
            {
                if (triplet.Third.IsMatch(eventObject.Subject))
                {
                    triplet.Second(this, arg);
                }
            }
        }

        private sealed class Unsubscribe : IDisposable
        {
            private readonly Triplet<string, EventHandler<EventArg>, Regex> _triplet;

            private readonly IList<Triplet<string, EventHandler<EventArg>, Regex>> _list;

            private bool _isDisposed = false;

            internal Unsubscribe(Triplet<string, EventHandler<EventArg>, Regex> triplet, 
                IList<Triplet<string, EventHandler<EventArg>, Regex>> list)
            {
                this._triplet = triplet;
                this._list = list;
            }

            public void Dispose()
            {
                if (!this._isDisposed)
                {
                    this._list.Remove(this._triplet);
                    this._isDisposed = true;
                }
            }
        }

        private sealed class UnsubscribeMiltiple : IDisposable
        {
            private bool _isDisposed = false;

            public UnsubscribeMiltiple()
            {
                this.Disposables=new List<IDisposable>();
            }

            public List<IDisposable> Disposables { get; private set; }

            public void Dispose()
            {
                if (!this._isDisposed)
                {
                    foreach (var disposable in this.Disposables)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        private void CommandScope_AfterDispose(object sender, EventArgs e)
        {
            var commandScope = sender as CommandScope;
            try
            {
                this._slimForPostTransactionPublish.EnterWriteLock();
                this._transactionalDomainEvents.Remove(commandScope);
            }
            finally
            {
                this._slimForPostTransactionPublish.ExitWriteLock();
            }
        }

        private void CommandScope_AfterComplete(object sender, EventArgs e)
        {
            var commandScope = sender as CommandScope;
            IList<KeyValuePair<IEvent, bool>> eventKvps;

            try
            {
                this._slimForPostTransactionPublish.EnterUpgradeableReadLock();
                if (this._transactionalDomainEvents.TryGetValue(commandScope, out eventKvps))
                {
                    try
                    {
                        this._slimForPostTransactionPublish.EnterWriteLock();
                        this._transactionalDomainEvents.Remove(commandScope);
                    }
                    finally
                    {
                        this._slimForPostTransactionPublish.ExitWriteLock();
                    }
                }
            }
            finally
            {
                this._slimForPostTransactionPublish.ExitUpgradeableReadLock();
            }

            if (eventKvps != null)
            {
                foreach (var kvp in eventKvps)
                {
                    this.PublishEvents(kvp.Key, false, kvp.Value);
                }
            }
        }
    }
}
