using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Transactions;

namespace CSharp.Utils.Transactional
{
    public class Transactional<T> : IEnlistmentNotification
    {
        private Transaction _mCurrentTransaction;

        private TransactionalLock _mLock;

        private T _mTemporaryValue;

        private T _mValue;

        static Transactional()
        {
            ResourceManager.ConstrainType(typeof(T));
        }

        public Transactional(T value)
        {
            this._mLock = new TransactionalLock();
            this._mValue = value;
        }

        public Transactional(Transactional<T> transactional)
        : this(transactional.Value)
        {
        }

        public Transactional()
        : this(default(T))
        {
        }

        public T Value
        {
            get
            {
                return this.GetValue();
            }

            set
            {
                this.SetValue(value);
            }
        }

        public static implicit operator T(Transactional<T> transactional)
        {
            return transactional.Value;
        }

        public static bool operator !=(T t1, Transactional<T> t2)
        {
            return !(t1 == t2);
        }

        public static bool operator !=(Transactional<T> t1, T t2)
        {
            return !(t1 == t2);
        }

        public static bool operator !=(Transactional<T> t1, Transactional<T> t2)
        {
            return !(t1 == t2);
        }

        public static bool operator ==(Transactional<T> t1, Transactional<T> t2)
        {
            bool t1Null = ReferenceEquals(t1, null) || t1.Value == null;
            bool t2Null = ReferenceEquals(t2, null) || t2.Value == null;
            if (t1Null && t2Null)
            {
                return true;
            }

            if (t1Null || t2Null)
            {
                return false;
            }

            return EqualityComparer<T>.Default.Equals(t1.Value, t2.Value);
        }

        public static bool operator ==(Transactional<T> t1, T t2)
        {
            bool t1Null = ReferenceEquals(t1, null) || t1.Value == null;
            bool t2Null = t2 == null;
            if (t1Null && t2Null)
            {
                return true;
            }

            if (t1Null || t2Null)
            {
                return false;
            }

            return EqualityComparer<T>.Default.Equals(t1.Value, t2);
        }

        public static bool operator ==(T t1, Transactional<T> t2)
        {
            bool t1Null = t1 == null;
            bool t2Null = ReferenceEquals(t2, null) || t2.Value == null;
            if (t1Null && t2Null)
            {
                return true;
            }

            if (t1Null || t2Null)
            {
                return false;
            }

            return EqualityComparer<T>.Default.Equals(t1, t2.Value);
        }

        public override bool Equals(object obj)
        {
            return this.Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            IDisposable disposable = this._mValue as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            this._mValue = this._mTemporaryValue;
            this._mCurrentTransaction = null;
            this._mTemporaryValue = default(T);
            this._mLock.Unlock();
            enlistment.Done();
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            this._mLock.Unlock();
            enlistment.Done();
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            this._mCurrentTransaction = null;

            IDisposable disposable = this._mTemporaryValue as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            this._mTemporaryValue = default(T);
            this._mLock.Unlock();
            enlistment.Done();
        }

        private void Enlist(T t)
        {
            Debug.Assert(this._mCurrentTransaction == null);
            this._mCurrentTransaction = Transaction.Current;
            Debug.Assert(this._mCurrentTransaction.TransactionInformation.Status == TransactionStatus.Active);
            this._mCurrentTransaction.EnlistVolatile(this, EnlistmentOptions.None);
            this._mTemporaryValue = ResourceManager.Clone(t);
        }

        private T GetValue()
        {
            this._mLock.Lock();
            if (this._mCurrentTransaction == null)
            {
                if (Transaction.Current == null)
                {
                    return this._mValue;
                }
                else
                {
                    this.Enlist(this._mValue);
                }
            }

            Debug.Assert(
                this._mCurrentTransaction == Transaction.Current, 
                "Invalid state in the volatile resource state machine");

            return this._mTemporaryValue;
        }

        private void SetValue(T t)
        {
            this._mLock.Lock();
            if (this._mCurrentTransaction == null)
            {
                if (Transaction.Current == null)
                {
                    this._mValue = t;
                    return;
                }
                else
                {
                    this.Enlist(t);
                    return;
                }
            }
            else
            {
                Debug.Assert(this._mCurrentTransaction == Transaction.Current, "Invalid state in the volatile resource state machine");
                this._mTemporaryValue = t;
            }
        }
    }
}
