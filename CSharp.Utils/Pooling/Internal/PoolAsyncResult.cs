using System;
using CSharp.Utils.Threading;

namespace CSharp.Utils.Pooling.Internal
{
    internal class PoolAsyncResult : AsyncResult<object>
    {
        #region Constructors and Finalizers

        internal PoolAsyncResult(int poolId, Type poolType)
        {
            this.PoolId = poolId;
            this.PoolType = poolType;
        }

        #endregion Constructors and Finalizers

        #region Properties

        internal int PoolId { get; private set; }

        internal Type PoolType { get; private set; }

        #endregion Properties
    }
}
