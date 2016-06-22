using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CSharp.Utils.Reflection
{
    public static class ComponentUpdateWatcher
    {
        #region Static Fields

        private static readonly Dictionary<string, KeyValuePair<FileSystemWatcher, List<ComponentUpdateRegistrationInfo>>> _dictionary = new Dictionary<string, KeyValuePair<FileSystemWatcher, List<ComponentUpdateRegistrationInfo>>>();

        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        #endregion Static Fields

        #region Public Methods and Operators

        public static void RegisterForUpdate(ComponentUpdateRegistrationInfo info)
        {
            _lock.EnterWriteLock();
            try
            {
                string directoryName = Path.GetDirectoryName(info.FileName).ToUpper();
                KeyValuePair<FileSystemWatcher, List<ComponentUpdateRegistrationInfo>> kvp;
                if (_dictionary.TryGetValue(directoryName, out kvp))
                {
                    kvp.Value.Add(info);
                }
                else
                {
                    var watcher = new FileSystemWatcher(directoryName);
                    watcher.Changed += watcher_Changed;
                    var list = new List<ComponentUpdateRegistrationInfo> { info };
                    _dictionary.Add(directoryName, new KeyValuePair<FileSystemWatcher, List<ComponentUpdateRegistrationInfo>>(watcher, list));
                    watcher.EnableRaisingEvents = true;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public static void UnregisterForUpdate(ComponentUpdateRegistrationInfo info)
        {
            string directoryName = Path.GetDirectoryName(info.FileName).ToUpper();
            _lock.EnterWriteLock();
            try
            {
                KeyValuePair<FileSystemWatcher, List<ComponentUpdateRegistrationInfo>> kvp;
                if (_dictionary.TryGetValue(directoryName, out kvp))
                {
                    kvp.Value.Remove(info);
                    if (kvp.Value.Count == 0)
                    {
                        kvp.Key.Dispose();
                        _dictionary.Remove(directoryName);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        private static void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var watcher = sender as FileSystemWatcher;
            string directoryName = watcher.Path.ToUpper();
            _lock.EnterReadLock();
            try
            {
                KeyValuePair<FileSystemWatcher, List<ComponentUpdateRegistrationInfo>> kvp;
                if (_dictionary.TryGetValue(directoryName, out kvp))
                {
                    foreach (ComponentUpdateRegistrationInfo info in kvp.Value)
                    {
                        if (info.FileName.ToUpper() == e.FullPath.ToUpper())
                        {
                            watcher.EnableRaisingEvents = false;
                            object component = info.Component;
                            if (component != null)
                            {
                                ComponentBuilder.UpdateComponent(component, info.FileName, info.NodePath, info.Instantiator);
                                if (info.Callback != null)
                                {
                                    info.Callback(info);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
                watcher.EnableRaisingEvents = true;
            }
        }

        #endregion Methods
    }
}
