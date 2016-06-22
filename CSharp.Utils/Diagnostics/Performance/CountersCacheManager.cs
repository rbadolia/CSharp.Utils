using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharp.Utils.Collections.Concurrent;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Diagnostics.Performance.Dtos;

namespace CSharp.Utils.Diagnostics.Performance
{
    public sealed class CountersCacheManager : AbstractDisposable
    {
        #region Static Fields

        private static readonly CountersCacheManager InstanceObject = new CountersCacheManager();

        #endregion Static Fields

        #region Fields

        private readonly LockFactory lockFactory = new LockFactory();

        private readonly CompositeDictionary<string, ProcessSettingsInfo> settingsDictionary = new CompositeDictionary<string, ProcessSettingsInfo>(true);

        private readonly CompositeDictionary<string, SlidingWindow<CounterData>> windowDictionary = new CompositeDictionary<string, SlidingWindow<CounterData>>(false);

        #endregion Fields

        #region Constructors and Finalizers

        private CountersCacheManager()
        {
            this.SlidingWindowSize = 60;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static CountersCacheManager Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public int SlidingWindowSize { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public List<string> GetApplicationNames(string machineName)
        {
            return this.GetSubKeys(machineName);
        }

        public List<string> GetCategoryNames(string machineName, string applicationName, string environment)
        {
            return this.GetSubKeys(machineName, applicationName, environment);
        }

        public List<string> GetCounterNames(string machineName, string applicationName, string environment, string categoryName, string instanceName)
        {
            return this.GetSubKeys(machineName, applicationName, environment, categoryName, instanceName);
        }

        public List<CounterData> GetCounterValues(string machineName, string applicationName, string environment, string categoryName, string instanceName, string counterName)
        {
            try
            {
                SlidingWindow<CounterData> window;
                if (this.windowDictionary.TryGetValue(out window, machineName, applicationName, environment, categoryName, instanceName, counterName))
                {
                    return window.GetAllItems().ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return new List<CounterData>(0);
        }

        public List<string> GetEnvironmentNames(string machineName, string applicationName)
        {
            return this.GetSubKeys(machineName, applicationName);
        }

        public List<string> GetInstanceNames(string machineName, string applicationName, string environment, string categoryName)
        {
            return this.GetSubKeys(machineName, applicationName, environment, categoryName);
        }

        public CounterData GetLastCounterData(string machineName, string applicationName, string environment, string categoryName, string instanceName, string counterName)
        {
            try
            {
                SlidingWindow<CounterData> window;
                if (this.windowDictionary.TryGetValue(out window, machineName, applicationName, environment, categoryName, instanceName, counterName))
                {
                    CounterData data;
                    window.TryGetLast(out data);
                    return data;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return null;
        }

        public List<string> GetMachineNames()
        {
            return this.GetSubKeys(new string[] { });
        }

        public bool GetShouldPublish(string machineName, string applicationName, string environment)
        {
            ProcessSettingsInfo settings;
            if (this.settingsDictionary.TryGetValue(out settings, machineName, applicationName, environment))
            {
                return settings.ShouldPublish;
            }

            return true;
        }

        public List<string> GetSubKeys(params string[] keys)
        {
            try
            {
                List<string> list = this.windowDictionary.GetKeys(keys);
                list.Sort();
                return list;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return new List<string>(0);
        }

        public void PublishCounters(CounterUpdateInfo info)
        {
            try
            {
                bool isCreated = false;
                ProcessSettingsInfo settings = this.settingsDictionary.GetOrAddAndGet(delegate
                    {
                        var sett = new ProcessSettingsInfo { ProcessId = info.ProcessInfo.ProcessId };
                        isCreated = true;
                        return sett;
                    }, info.ProcessInfo.MachineName, info.ProcessInfo.ApplicationName, info.ProcessInfo.Environment);
                lock (settings)
                {
                    if (!isCreated)
                    {
                        if (info.ProcessInfo.ProcessId != settings.ProcessId && (info.ProcessInfo.ProcessId != settings.PreviousProcessId))
                        {
                            settings.PreviousProcessId = settings.ProcessId;
                            settings.ProcessId = info.ProcessInfo.ProcessId;
                        }
                    }

                    if (settings.PreviousProcessId == null || settings.PreviousProcessId != info.ProcessInfo.ProcessId)
                    {
                        foreach (CounterCategoryInfo category in info.Categories)
                        {
                            for (int i = 0; i < category.CounterNames.Count; i++)
                            {
                                foreach (CounterInstanceInfo instance in category.Instances)
                                {
                                    SlidingWindow<CounterData> window;
                                    if (!this.windowDictionary.TryGetValue(out window, info.ProcessInfo.MachineName, info.ProcessInfo.ApplicationName, info.ProcessInfo.Environment, category.CategoryName, instance.InstanceName, category.CounterNames[i]))
                                    {
                                        window = new SlidingWindow<CounterData>(this.SlidingWindowSize);
                                        this.windowDictionary.AddOrUpdate(window, string.Intern(info.ProcessInfo.MachineName), string.Intern(info.ProcessInfo.ApplicationName), string.Intern(info.ProcessInfo.Environment), string.Intern(category.CategoryName), string.Intern(instance.InstanceName), string.Intern(category.CounterNames[i]));
                                    }

                                    window.Add(new CounterData(GlobalSettings.Instance.CurrentDateTime, instance.CounterValues[i]));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void SetShouldPublish(string machineName, string applicationName, string environment, bool value)
        {
            ProcessSettingsInfo settings;
            if (this.settingsDictionary.TryGetValue(out settings, machineName, applicationName, environment))
            {
                settings.ShouldPublish = value;
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.lockFactory.Dispose();
            this.windowDictionary.Dispose();
            this.settingsDictionary.Dispose();
        }

        #endregion Methods
    }
}
