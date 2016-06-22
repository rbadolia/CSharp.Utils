using System;
using System.Collections.Generic;

namespace CSharp.Utils.Threading
{
    public static class DispatchingHelper
    {
        public static void Dispatch(IEnumerable<Delegate> delegates, IDispatcherProvider dispatcherProvider, params object[] arguments)
        {
            Dispatch(delegates, dispatcherProvider, del => del.DynamicInvoke(arguments));
        }

        public static void Dispatch(IEnumerable<Delegate> delegates, IDispatcherProvider dispatcherProvider, Action<Delegate> action)
        {
            if (dispatcherProvider == null)
            {
                foreach (var del in delegates)
                {
                    del.DynamicInvoke(action);
                }

                return;
            }

            foreach (var del in delegates)
            {
                if (del == null)
                {
                    continue;
                }

                Delegate[] invocationList = del.GetInvocationList();
                if (invocationList.Length > 1)
                {
                    Dispatch(invocationList, dispatcherProvider, action);
                }
                else
                {
                    IDispatcher dispatcher = null;
                    if (del.Target != null)
                    {
                        dispatcher = dispatcherProvider.GetDispatcher(del.Target);
                    }

                    if (dispatcher == null)
                    {
                        action(del);
                    }
                    else
                    {
                        Delegate delegateToBeCalled = del;
                        dispatcher.Dispatch(() => action(delegateToBeCalled));
                    }
                }
            }
        }
    }
}
