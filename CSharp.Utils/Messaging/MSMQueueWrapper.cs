using System;
using System.Collections.Generic;
using System.Messaging;
using CSharp.Utils.Collections.Generic;

namespace CSharp.Utils.Messaging
{
    public class MSMQueueWrapper<T> : AbstractInitializableAndDisposable, ISimpleQueue<T>
    {
        #region Fields

        private MessageQueue adaptedObject;

        private MessageQueue _tempQueue;

        #endregion Fields

        #region Constructors and Finalizers

        public MSMQueueWrapper()
        {
            this.CreateQueueIfDoesNotExist = true;
            this.Formatter = new BinaryMessageFormatter();
            this.Recoverable = true;
            this.FilterDuplicates = true;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public bool CreateQueueIfDoesNotExist { get; set; }

        public bool FilterDuplicates { get; set; }

        public IMessageFormatter Formatter { get; set; }

        public string QueueName { get; set; }

        public bool Recoverable { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public IEnumerable<T> Dequeue()
        {
            this.Initialize();
            MessageQueue queue = this.adaptedObject;
            if (this.FilterDuplicates)
            {
                MessageEnumerator messageEnumerator = this.adaptedObject.GetMessageEnumerator2();
                IEnumerable<KeyValuePair<Message, T>> enumerable = this.convertCommonIntoMessageAndObjectPairs(messageEnumerator);
                IEnumerable<KeyValuePair<Message, T>> filteredEnumerable = new DuplicatesFilteredEnumerable<KeyValuePair<Message, T>>(enumerable);
                foreach (var kvp in filteredEnumerable)
                {
                    Message message = kvp.Key;
                    message.Recoverable = true;
                    this._tempQueue.Send(message);
                }

                queue = this._tempQueue;
            }

            MessageEnumerator tempMessageEnumerator = queue.GetMessageEnumerator2();
            return this.convertCommonIntoObjects(tempMessageEnumerator);
        }

        public void Enqueue(IEnumerable<T> objects)
        {
            this.Initialize();
            foreach (T obj in objects)
            {
                Enqueue(obj);
            }
        }

        public void Enqueue(T obj)
        {
            this.Initialize();
            var message = new Message(obj, this.Formatter) { Recoverable = this.Recoverable };
            this.adaptedObject.Send(message);
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            GeneralHelper.DisposeIDisposable(this.adaptedObject);
            GeneralHelper.DisposeIDisposable(this._tempQueue);
        }

        protected override void InitializeProtected()
        {
            this.adaptedObject = getOrCreateMessageQueue(this.QueueName, this.CreateQueueIfDoesNotExist);
            if (this.FilterDuplicates)
            {
                this._tempQueue = getOrCreateMessageQueue(this.QueueName + "_temp", true);
            }
        }

        private static MessageQueue getOrCreateMessageQueue(string queueName, bool createIfDoesNotExist)
        {
            if (MessageQueue.Exists(queueName))
            {
                return new MessageQueue(queueName);
            }

            if (!createIfDoesNotExist)
            {
                throw new ApplicationException("There is no MSMQ available with the given name: " + queueName);
            }

            return MessageQueue.Create(queueName, false);
        }

        private T convertMessageIntoObject(Message message)
        {
            return (T)this.Formatter.Read(message);
        }

        private IEnumerable<KeyValuePair<Message, T>> convertCommonIntoMessageAndObjectPairs(MessageEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                Message message = enumerator.Current;
                yield return new KeyValuePair<Message, T>(message, (T)this.Formatter.Read(message));
                enumerator.RemoveCurrent();
            }

            enumerator.Dispose();
        }

        private IEnumerable<T> convertCommonIntoObjects(MessageEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return this.convertMessageIntoObject(enumerator.Current);
                enumerator.RemoveCurrent();
            }

            enumerator.Dispose();
        }

        #endregion Methods
    }
}
