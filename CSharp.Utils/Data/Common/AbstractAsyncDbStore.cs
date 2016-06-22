using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using CSharp.Utils.Contracts;
using CSharp.Utils.Threading;

namespace CSharp.Utils.Data.Common
{
    public class AbstractAsyncDbStore : AbstractDbStore, IDisposable, IInitializable
    {
        #region Fields

        protected long Interval { get; set; }

        protected int MaximumCacheSize { get; set; }

        private readonly IDbStrategy dbStrategy = null;

        private readonly List<DbParameterCollection> collection1 = new List<DbParameterCollection>();

        private readonly List<DbParameterCollection> collection2 = new List<DbParameterCollection>();

        private IntervalTask task;

        private List<DbParameterCollection> writingCollection;

        #endregion Fields

        #region Constructors and Finalizers

        public AbstractAsyncDbStore()
        {
            this.writingCollection = this.collection1;
        }

        ~AbstractAsyncDbStore()
        {
            this.Dispose(false);
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public bool IsInitialized { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                this.IsInitialized = true;
                this.start();
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected virtual void Dispose(bool disposing)
        {
            this.Flush(null);
            if (this.task != null)
            {
                this.task.Dispose();
            }
        }

        protected override IDbCommand GetCommand()
        {
            IDbConnection connection = this.dbStrategy.CreateConnection();
            IDbCommand command = connection.CreateCommand();
            return command;
        }

        protected override void Insert(object message, IDbCommand command)
        {
            command.Parameters.Clear();
            this.DbActivityStrategy.InitializeCommandForDml(command, CudOperationType.Created, message);
            this.Add(command.Parameters);
        }

        protected virtual void start()
        {
            if (this.task == null)
            {
                this.task = new IntervalTask(this.Interval, false);
                this.task.AddAction(this.Flush, null);
            }

            this.task.Start();
        }

        protected override int UpdateOrInsert(object message, IDbCommand command)
        {
            command.Parameters.Clear();
            this.DbActivityStrategy.InitializeCommandForDml(command, CudOperationType.Updated, message);
            this.Add(command.Parameters);
            return 0;
        }

        private void Add(IEnumerable coll)
        {
            this.writingCollection.Add(coll as DbParameterCollection);
            if (this.MaximumCacheSize > 0 && this.writingCollection.Count > this.MaximumCacheSize)
            {
                Task.Factory.StartNew(() => this.Flush(null));
            }
        }

        private void Flush(object tag)
        {
            List<DbParameterCollection> coll = this.writingCollection;
            this.writingCollection = this.writingCollection == this.collection1 ? this.collection2 : this.collection1;
            IDbCommand comm = base.GetCommand();
            var reader = new BulkCopyDataReader(coll);
            this.dbStrategy.BulkCopier.DoBulkCopy(reader, comm.Connection, this.TableName);
            coll.Clear();
        }

        #endregion Methods
    }
}
