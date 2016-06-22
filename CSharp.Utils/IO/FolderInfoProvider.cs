using System;
using System.Collections.Generic;
using System.IO;
using CSharp.Utils.Collections.Concurrent;

namespace CSharp.Utils.IO
{
    public class FolderInfoProvider
    {
        #region Fields

        private string _folderPath;

        #endregion Fields

        #region Constructors and Finalizers

        public FolderInfoProvider()
        {
            this.SearchPatterns = new List<string>();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string FolderPath
        {
            get
            {
                return this._folderPath;
            }

            set
            {
                if (value != null)
                {
                    value = value.TrimEnd('\\', '/');
                }

                this._folderPath = value;
            }
        }

        public List<string> SearchPatterns { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public string GetFolderPath()
        {
            string folderPath = IOHelper.ResolvePath(this._folderPath);
            return folderPath;
        }

        public List<CompositeKeyValue<string, FileInfo>> GetTree()
        {
            string directory = this.GetFolderPath();
            var directoryInfo = new DirectoryInfo(directory);
            if (directoryInfo.Exists)
            {
                List<FileInfo> files;
                if (this.SearchPatterns != null && this.SearchPatterns.Count > 0)
                {
                    files = new List<FileInfo>();
                    foreach (string s in this.SearchPatterns)
                    {
                        FileInfo[] f = directoryInfo.GetFiles(s, SearchOption.AllDirectories);
                        files.AddRange(f);
                    }
                }
                else
                {
                    FileInfo[] f = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
                    files = new List<FileInfo>(f);
                }

                var dictionary = new CompositeDictionary<string, FileInfo>(false);
                foreach (FileInfo file in files)
                {
                    string[] keys = this.getKeys(file);
                    dictionary.AddOrUpdate(file, keys);
                }

                List<CompositeKeyValue<string, FileInfo>> items = dictionary.GetCompleteTree(CompositeKeyValueComparer.Instance, true);
                return items;
            }

            return null;
        }

        #endregion Public Methods and Operators

        #region Methods

        private string[] getKeys(FileInfo file)
        {
            string directory = this.GetFolderPath();
            string s = file.DirectoryName.Substring(directory.Length);

            int index = file.Name.IndexOf('.');
            if (index > -1)
            {
                s += '\\' + file.Name.Substring(0, index);
            }

            s += '\\' + file.Extension.Substring(1);
            s += '\\' + file.Name;
            return s.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion Methods
    }
}
