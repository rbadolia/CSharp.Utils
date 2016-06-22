using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Threading
{

    #region Delegates

    public delegate T AsyncDelegate<out T>();

    #endregion Delegates

    public static class ThreadingHelper
    {
    	#region Public Methods and Operators

        public static Task<T> WrapTask<T>(Task<object> task)
        {
            return task.ContinueWith(parentTask => (T)parentTask.Result, TaskContinuationOptions.ExecuteSynchronously);
        }

        public static void StoreDataInThreadLocalStorage(string slotName, object data)
        {
        	Guard.ArgumentNotNullOrEmptyOrWhiteSpace(slotName, "slotName");
        	var slot = Thread.GetNamedDataSlot(slotName);
        	Thread.SetData(slot, data);
        }

        public static object GetDataFromThreadLocalStorage(string slotName)
        {
        	Guard.ArgumentNotNullOrEmptyOrWhiteSpace(slotName, "slotName");
        	var slot = Thread.GetNamedDataSlot(slotName);
        	return Thread.GetData(slot);
        }

        public static void RemoveDataFromThreadLocalStorage(string slotName)
		{
			Guard.ArgumentNotNullOrEmptyOrWhiteSpace(slotName, "slotName");
        	Thread.FreeNamedDataSlot(slotName);
		}

        public static AsyncResult<T> BeginExecute<T>(AsyncDelegate<T> asyncDelegate, AsyncCallback callback)
        {
            var result = new AsyncResult<T>();
            Task.Factory.StartNew(delegate
                {
                    try
                    {
                        T returnedValue = asyncDelegate();
                        result.SetCompleted(returnedValue);
                    }
                    catch (Exception ex)
                    {
                        result.SetCompletedWithException(ex);
                    }

                    if (callback != null)
                    {
                        callback(result);
                    }
                });
            return result;
        }

        public static T EndExecute<T>(AsyncResult<T> result)
        {
            result.AsyncWaitHandle.WaitOne();
            if (result.Exception != null)
            {
                throw result.Exception;
            }

            return result.TypedAsyncState;
        }

        public static void ExchangeIfGreaterThan(ref long location, long value)
        {
            exchangeIfGreaterThanOrLessThan(ref location, value, true);
        }

        public static void ExchangeIfLessThan(ref long location, long value)
        {
            exchangeIfGreaterThanOrLessThan(ref location, value, false);
        }

        public static void ExecuteParallel(IEnumerable<Action> actions, bool waitForAllToFinish)
        {
            if (!waitForAllToFinish)
            {
                foreach (Action action in actions)
                {
                    Task.Factory.StartNew(state =>
                        {
                            var act = (Action)state;
                            act();
                        }, action);
                }
            }
            else
            {
                var countDownEvent = new CountdownEvent(0);
                foreach (Action action in actions)
                {
                    countDownEvent.AddCount();
                    Task.Factory.StartNew(state =>
                        {
                            var act = (Action)state;
                            try
                            {
                                act();
                            }
                            finally
                            {
                                countDownEvent.Signal();
                            }
                        }, action);
                }

                countDownEvent.Wait();
                countDownEvent.Dispose();
            }
        }

        public static EventWaitHandle GetOrCreateNamedWaitHandle(string waitHandleName, EventResetMode mode, out bool isCreated)
        {
            isCreated = false;
            EventWaitHandle ewh;
            bool doesNotExist = false;
            try
            {
                return EventWaitHandle.OpenExisting(waitHandleName);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                doesNotExist = true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine(ex);
            }

            if (doesNotExist)
            {
                try
                {
                    ewh = new EventWaitHandle(true, mode, waitHandleName, out isCreated);
                    return ewh;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                string user = Environment.UserDomainName + "\\" + Environment.UserName;
                var ewhSec = new EventWaitHandleSecurity();

                var rule = new EventWaitHandleAccessRule(user, EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify, AccessControlType.Deny);
                ewhSec.AddAccessRule(rule);

                rule = new EventWaitHandleAccessRule(user, EventWaitHandleRights.ReadPermissions | EventWaitHandleRights.ChangePermissions, AccessControlType.Allow);
                ewhSec.AddAccessRule(rule);
                ewh = new EventWaitHandle(true, mode, waitHandleName, out isCreated, ewhSec);
                if (!isCreated)
                {
                    return null;
                }

                return ewh;
            }

            try
            {
                ewh = EventWaitHandle.OpenExisting(waitHandleName, EventWaitHandleRights.ReadPermissions | EventWaitHandleRights.ChangePermissions);
                EventWaitHandleSecurity ewhSec = ewh.GetAccessControl();

                string user = Environment.UserDomainName + "\\" + Environment.UserName;
                var rule = new EventWaitHandleAccessRule(user, EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify, AccessControlType.Deny);
                ewhSec.RemoveAccessRule(rule);
                rule = new EventWaitHandleAccessRule(user, EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify, AccessControlType.Allow);
                ewhSec.AddAccessRule(rule);
                ewh.SetAccessControl(ewhSec);
                ewh = EventWaitHandle.OpenExisting(waitHandleName);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine(ex);
                return null;
            }

            return ewh;
        }

        #endregion Public Methods and Operators

        #region Methods

        private static void exchangeIfGreaterThanOrLessThan(ref long location, long value, bool greaterThan)
        {
            long current = Interlocked.Read(ref location);
            while ((greaterThan && current < value) || (!greaterThan && current > value))
            {
                long previous = Interlocked.CompareExchange(ref location, value, current);
                if (previous == current || ((greaterThan && previous >= value) || (!greaterThan && previous <= value)))
                {
                    break;
                }

                current = Interlocked.Read(ref location);
            }
        }

        #endregion Methods
    }
}
