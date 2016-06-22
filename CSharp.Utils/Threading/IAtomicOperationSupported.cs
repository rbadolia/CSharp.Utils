using System;

namespace CSharp.Utils.Threading
{
    public interface IAtomicOperationSupported
    {
        void PerformAtomicOperation(Action action);
    }
}
