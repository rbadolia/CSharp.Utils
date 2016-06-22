using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CSharp.Utils.Collections.Concurrent;
using CSharp.Utils.Contracts;

namespace CSharp.Utils.IO
{
    public sealed class FolderWatcher : AbstractInitializableAndDisposable, IInitializable
    {
        #region Fields

        private readonly CompositeDictionary<string, string> _dictionary = new CompositeDictionary<string, string>(true);

        private FileSystemWatcher _watcher;

        #endregion Fields

        #region Public Properties

        public List<string> Extensions { get; set; }

        public string FolderPath { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public XmlDocument ExportAsXmlDocument()
        {
            return CompositeKeyValue<string, string>.ExportAsXmlDocument(this.GetHierarchy());
        }

        public List<CompositeKeyValue<string, string>> GetHierarchy()
        {
            return this._dictionary.GetCompleteTree();
        }

        public string GetIfExists(string fileName)
        {
            string relativePath = fileName.Substring(this.FolderPath.Length);
            string[] splits = this.split(relativePath);
            string value;
            if (this._dictionary.TryGetValue(out value, splits))
            {
                return value;
            }

            return null;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this._watcher.Dispose();
            this._dictionary.Dispose();
        }

        protected override void InitializeProtected()
        {
            var di = new DirectoryInfo(this.FolderPath);
            if (!di.Exists)
            {
                di.Create();
            }

            FileInfo[] files = di.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo fileInfo in files)
            {
                if (this.Extensions.Contains(fileInfo.Extension, StringComparer.OrdinalIgnoreCase))
                {
                    this.addOrRemove(fileInfo.FullName, true);
                }
            }

            this._watcher = this.watchDirectory(this.FolderPath);
        }

        private void addOrRemove(string fileName, bool isAdd)
        {
            string relativePath = fileName.Substring(this.FolderPath.Length);
            string[] splits = this.split(relativePath);
            if (isAdd)
            {
                this._dictionary.AddOrUpdate(relativePath, splits);
            }
            else
            {
                this._dictionary.RemoveIfExists(splits);
            }
        }

        private string[] split(string relativePath)
        {
            return relativePath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private FileSystemWatcher watchDirectory(string directory)
        {
            directory = directory.ToUpper();
            var watcher = new FileSystemWatcher(directory);
            watcher.Created += this.watcher_Created;
            watcher.Deleted += this.watcher_Deleted;
            watcher.Renamed += this.watcher_Renamed;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private void watcher_Created(object sender, FileSystemEventArgs e)
        {
            this._watcher.EnableRaisingEvents = false;
            this.addOrRemove(e.FullPath, false);
            this._watcher.EnableRaisingEvents = true;
        }

        private void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            this._watcher.EnableRaisingEvents = false;
            this.addOrRemove(e.FullPath, false);
            this._watcher.EnableRaisingEvents = true;
        }

        private void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            this._watcher.EnableRaisingEvents = false;
            this.addOrRemove(e.OldFullPath, false);
            this.addOrRemove(e.FullPath, true);
            this._watcher.EnableRaisingEvents = true;
        }

        #endregion Methods
    }
}
