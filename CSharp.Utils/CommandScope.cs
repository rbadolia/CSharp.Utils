using System;
using System.Collections.Generic;
using System.Transactions;
using CSharp.Utils.Diagnostics;
using CSharp.Utils.Transactional;
using CSharp.Utils.Validation;

namespace CSharp.Utils
{
    public sealed class CommandScope : AbstractIdentity, IDisposable
    {
        public Guid? ParentCommandScopeId { get; private set; }

        public string UseCaseName { get; private set; }

        public string EntityOrServiceName { get; private set; }

        public Guid ActorId { get; private set; }

        public string Comment { get; private set; }

        public DateTime ScopeStartedOn { get; private set; }

        public DateTime? ScopeEndedOn { get; private set; }

        public static bool CapturingPerformanceMetricsEnabled { get; set; }

        private TransactionScope _transactionScope;

        [ThreadStatic]
        private static Stack<CommandScope> _stack;

        public bool IsCompleted { get; private set; }

        public static event EventHandler BeforeComplete;

        public static event EventHandler AfterComplete;

        public static event EventHandler AfterDispose;

        public static event EventHandler AfterStartingCommandScope;

        private bool _isDisposed = false;

        private readonly UseCasePerformanceMetrics _metrics;

        private readonly long _ticksAtStart;

        static CommandScope()
        {
            CapturingPerformanceMetricsEnabled = true;
        }

        public CommandScope(string entityOrServiceName, string useCaseName, Guid actorId, string comment = null, bool shouldCapturePerformanceMetrics = true)
        {
            this._ticksAtStart = SharedStopWatch.ElapsedTicks;

            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(entityOrServiceName, "entityOrServiceName");
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(useCaseName, "useCaseName");

            this.EntityOrServiceName = entityOrServiceName;
            this.UseCaseName = useCaseName;
            this.ActorId = actorId;
            if (shouldCapturePerformanceMetrics && CapturingPerformanceMetricsEnabled)
            {
                this._metrics = UseCasePerformanceMetrics.GetUseCasePerformanceMetrics(this.EntityOrServiceName, this.UseCaseName);
                this._metrics.RequestReceived();
            }

            this._transactionScope = TransactionHelper.CreateTransactionScope();

            if (_stack == null)
            {
                _stack = new Stack<CommandScope>();
            }
            else
            {
                var parentCommandScope = _stack.Peek();
                if (parentCommandScope != null)
                {
                    this.ParentCommandScopeId = parentCommandScope.Id;
                }
            }

            _stack.Push(this);
            this.ScopeStartedOn = GlobalSettings.Instance.CurrentDateTime;
            try
            {
                var handler = AfterStartingCommandScope;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
            catch
            {
                this.Pop();
                throw;
            }
        }

        public static CommandScope Current
        {
            get
            {
                if (_stack == null)
                {
                    return null;
                }

                return _stack.Peek();
            }
        }

        public void Complete()
        {
            if (this._isDisposed || this.IsCompleted)
            {
                return;
            }

            try
            {
                var handler = BeforeComplete;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
            catch
            {
                this.UpdateMetrics(true);
                throw;
            }

            this._transactionScope.Complete();
            this._transactionScope.Dispose();
            this.IsCompleted = true;
            this.ScopeEndedOn = GlobalSettings.Instance.CurrentDateTime;
            try
            {
                var handler = AfterComplete;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
            finally
            {
                this.UpdateMetrics(false);
            }
        }

        public void Dispose()
        {
            if (this._isDisposed)
            {
                return;
            }

            this._transactionScope.Dispose();
            this.ScopeEndedOn = GlobalSettings.Instance.CurrentDateTime;

            this._isDisposed = true;

            try
            {
                var handler = AfterDispose;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }

            }
            finally
            {
                this.Pop();
                if (!this.IsCompleted)
                {
                    this.UpdateMetrics(true);
                }
            }
        }

        private void Pop()
        {
            _stack.Pop();
            if (_stack.Count == 0)
            {
                _stack = null;
            }
        }

        private void UpdateMetrics(bool isFailed)
        {
            if (this._metrics != null)
            {
                this._metrics.RequestProcessed(SharedStopWatch.ElapsedTicks - this._ticksAtStart, isFailed);
            }
        }

        public static void CheckAndThrowExceptionIfItIsNotInACommandScope()
        {
            if (Current == null)
            {
                throw new InvalidOperationException("A CommandScope is required.");
            }
        }
    }
}
