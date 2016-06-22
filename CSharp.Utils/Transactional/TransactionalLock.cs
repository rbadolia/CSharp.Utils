using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Transactions;

namespace CSharp.Utils.Transactional
{
    public class TransactionalLock
    {
        private Transaction _mOwningTransaction;

        private LinkedList<KeyValuePair<Transaction, ManualResetEvent>> _mPendingTransactions = new LinkedList<KeyValuePair<Transaction, ManualResetEvent>>();

        public bool Locked
        {
            get
            {
                return this.OwningTransaction != null;
            }
        }

        private Transaction OwningTransaction
        {
            get
            {
                lock (this)
                {
                    return this._mOwningTransaction;
                }
            }

            set
            {
                lock (this)
                {
                    this._mOwningTransaction = value;
                }
            }
        }

        public void Lock()
        {
            this.Lock(Transaction.Current);
        }

        public void Unlock()
        {
            Debug.Assert(this.Locked);

            this.OwningTransaction = null;

            LinkedListNode<KeyValuePair<Transaction, ManualResetEvent>> node = null;

            lock (this)
            {
                if (this._mPendingTransactions.Count > 0)
                {
                    node = this._mPendingTransactions.First;
                    this._mPendingTransactions.RemoveFirst();
                }
            }

            if (node != null)
            {
                Transaction transaction = node.Value.Key;
                ManualResetEvent manualEvent = node.Value.Value;
                this.Lock(transaction);
                lock (manualEvent)
                {
                    if (manualEvent.SafeWaitHandle.IsClosed == false)
                    {
                        manualEvent.Set();
                    }
                }
            }
        }

        private void Lock(Transaction transaction)
        {
            if (this.OwningTransaction == null)
            {
                if (transaction == null)
                {
                    return;
                }
                else
                {
                    Debug.Assert(transaction.IsolationLevel == IsolationLevel.Serializable);
                    this.OwningTransaction = transaction;
                    return;
                }
            }
            else
            {
                if (this.OwningTransaction == transaction)
                {
                    return;
                }
                else
                {
                    ManualResetEvent manualEvent = new ManualResetEvent(false);

                    KeyValuePair<Transaction, ManualResetEvent> pair;
                    pair = new KeyValuePair<Transaction, ManualResetEvent>(transaction, manualEvent);
                    lock (this)
                    {
                        this._mPendingTransactions.AddLast(pair);
                    }

                    if (transaction != null)
                    {
                        Debug.Assert(transaction.TransactionInformation.Status == TransactionStatus.Active);
                        transaction.TransactionCompleted += delegate
                        {
                            lock (this)
                            {
                                this._mPendingTransactions.Remove(pair);
                            }

                            lock (manualEvent)
                            {
                                if (manualEvent.SafeWaitHandle.IsClosed == false)
                                {
                                    manualEvent.Set();
                                }
                            }
                        };
                    }

                    manualEvent.WaitOne();
                    lock (manualEvent)
                    {
                        manualEvent.Close();
                    }
                }
            }
        }
    }
}
