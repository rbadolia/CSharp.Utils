using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CSharp.Utils.Validation;

namespace CSharp.Utils.IO
{
    public sealed class FolderWatcherFileQueue : AbstractInitializableAndDisposable, IFileQueue
    {
        #region Fields

        private readonly ManualResetEvent newFileWaitHandle = new ManualResetEvent(false);

        private readonly object syncLock = new object();

        private string directoryName;

        private string filter;

        private bool ignoreFileDeletes;

        private bool includeExistingFiles = true;

        private bool includeSubdirectories;

        private int? internalBufferSize;

        private LinkedList<string> list;

        private FileSystemWatcher watcher;

        #endregion Fields

        #region Constructors and Finalizers

        public FolderWatcherFileQueue(string directoryName, bool includeExistingFiles = true, string filter = null, bool includeSubdirectories = false)
        {
            this.IncludeExistingFiles = includeExistingFiles;
            this.DirectoryName = directoryName;
            this.Filter = filter;
            this.IncludeSubdirectories = includeSubdirectories;
        }

        public FolderWatcherFileQueue()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string DirectoryName
        {
            get
            {
                return this.directoryName;
            }

            set
            {
                this.CheckAndThrowObjectAlreadyInitializedException();
                this.directoryName = value;
            }
        }

        public string Filter
        {
            get
            {
                return this.filter;
            }

            set
            {
                this.CheckAndThrowObjectAlreadyInitializedException();
                this.filter = value;
            }
        }

        public bool IgnoreFileDeletes
        {
            get
            {
                return this.ignoreFileDeletes;
            }

            set
            {
                this.CheckAndThrowObjectAlreadyInitializedException();
                this.ignoreFileDeletes = value;
            }
        }

        public bool IncludeExistingFiles
        {
            get
            {
                return this.includeExistingFiles;
            }

            set
            {
                this.CheckAndThrowObjectAlreadyInitializedException();
                this.includeExistingFiles = value;
            }
        }

        public bool IncludeSubdirectories
        {
            get
            {
                return this.includeSubdirectories;
            }

            set
            {
                this.CheckAndThrowObjectAlreadyInitializedException();
                this.includeSubdirectories = value;
            }
        }

        public int? InternalBufferSize
        {
            get
            {
                if (this.watcher != null)
                {
                    return this.watcher.InternalBufferSize;
                }

                return this.internalBufferSize;
            }

            set
            {
                if (this.watcher != null && value != null)
                {
                    this.watcher.InternalBufferSize = value.Value;
                }

                this.internalBufferSize = value;
            }
        }

        public WaitHandle NewFileWaitHandle
        {
            get
            {
                return this.newFileWaitHandle;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public string GetNextFile()
        {
            lock (this.syncLock)
            {
                LinkedListNode<string> firstNode = this.list.First;
                if (firstNode == null)
                {
                    return null;
                }

                this.list.RemoveFirst();
                if (this.list.Count == 0)
                {
                    this.newFileWaitHandle.Reset();
                }

                return firstNode.Value;
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.watcher.Dispose();
        }

        protected override void InitializeProtected()
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(this.DirectoryName, "DirectoryName");
            this.list = new LinkedList<string>();
            this.watcher = new FileSystemWatcher(this.DirectoryName, this.Filter) { IncludeSubdirectories = this.IncludeSubdirectories, EnableRaisingEvents = false };

            if (this.internalBufferSize == null)
            {
                this.internalBufferSize = this.watcher.InternalBufferSize;
            }
            else
            {
                this.watcher.InternalBufferSize = this.internalBufferSize.Value;
            }

            this.watcher.Renamed += this._watcher_Renamed;
            this.watcher.Created += this._watcher_Created;
            if (!this.ignoreFileDeletes)
            {
                this.watcher.Deleted += this._watcher_Deleted;
            }

            this.watcher.EnableRaisingEvents = true;
            if (this.IncludeExistingFiles)
            {
                string[] files = Directory.GetFiles(this.DirectoryName, this.Filter);
                Array.Sort(files, LastWrittenDateComparer.Instance);
                lock (this.syncLock)
                {
                    foreach (string file in files)
                    {
                        this.list.AddLast(file);
                    }
                }
            }

            if (this.list.Count > 0)
            {
                this.newFileWaitHandle.Set();
            }
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            lock (this.syncLock)
            {
                this.deleted(e.FullPath);
                this.created(e.FullPath);
            }
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            lock (this.syncLock)
            {
                this.deleted(e.FullPath);
            }
        }

        private void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            lock (this.syncLock)
            {
                this.renamed(e.OldFullPath, e.FullPath);
            }
        }

        private void created(string filePath)
        {
            var fi = new FileInfo(filePath);
            if (fi.Exists)
            {
                this.list.AddLast(filePath);
                if (this.list.Count > 0)
                {
                    this.newFileWaitHandle.Set();
                }
            }
        }

        private void deleted(string filePath)
        {
            var fi = new FileInfo(filePath);
            if (!fi.Exists)
            {
                this.list.Remove(filePath);
                if (this.list.Count == 0)
                {
                    this.newFileWaitHandle.Reset();
                }
            }
        }

        private void renamed(string oldFilePath, string newFilepath)
        {
            this.deleted(oldFilePath);
            this.created(newFilepath);
        }

        #endregion Methods
    }
}
