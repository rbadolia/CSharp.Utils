using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Diagnostics.Performance.Dtos;
using CSharp.Utils.Reflection;
using CSharp.Utils.Threading;

namespace CSharp.Utils.Diagnostics.Performance
{
    public sealed class PerfCounterManager : AbstractDisposable
    {
        #region Static Fields

        private static readonly PerfCounterManager InstanceObject = new PerfCounterManager();

        #endregion Static Fields

        #region Fields

        private readonly Dictionary<Type, PerfCounterCategory> categories = new Dictionary<Type, PerfCounterCategory>();

        private readonly Dictionary<string, Pair<CustomPerfCounters, List<PerformanceCounter>>> customCounters = new Dictionary<string, Pair<CustomPerfCounters, List<PerformanceCounter>>>();

        private readonly MethodInfo genericAddInstanceMethod = typeof(PerfCounterManager).GetMethod("AddInstance", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        private IntervalTask task;

        #endregion Fields

        #region Constructors and Finalizers

        private PerfCounterManager()
        {
            this.StrictInterval = false;
            this.Interval = 2000;
            this.Enabled = false;
        }

        ~PerfCounterManager()
        {
            this.disposeAllCounters();
        }

        #endregion Constructors and Finalizers

        #region Public Events

        public event EventHandler<CountersCapturedEventArgs> AfterCountersCaptured;

        #endregion Public Events

        #region Public Properties

        public static PerfCounterManager Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public bool Enabled { get; set; }

        public long Interval { get; set; }

        public bool StrictInterval { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void AddCustomPerfCounters(CustomPerfCounters obj)
        {
            if (!this.Enabled)
            {
                return;
            }

            lock (this.customCounters)
            {
                Pair<CustomPerfCounters, List<PerformanceCounter>> pair;
                if (this.customCounters.TryGetValue(obj.CategoryName, out pair))
                {
                    return;
                }

                var counters = new List<PerformanceCounter>(); 
                pair = new Pair<CustomPerfCounters, List<PerformanceCounter>>(obj, counters);

                string currentProcessInstanceName = ProcessHelper.CurrentProcessInstanceName;
                foreach (CustomCounterInfo info in obj.Counters)
                {
                    try
                    {
                        var counter = new PerformanceCounter { CounterName = info.CounterName, CategoryName = info.CategoryName, InstanceName = info.IsCurrentProcess ? currentProcessInstanceName : info.InstanceName };
                        counters.Add(counter);
                    }
                    catch (Exception ex)
                    {
                        counters.Add(null);
                        Debug.WriteLine(ex);
                    }
                }

                this.customCounters.Add(obj.CategoryName, pair);
            }
        }

        [CautionUsedByReflection]
        public void AddInstance<T>(T instance, string instanceName, CounterStorageType storageType) where T : class
        {
            if (!this.Enabled)
            {
                return;
            }

            if (storageType != CounterStorageType.None)
            {
                Type t = typeof(T);
                lock (this.categories)
                {
                    PerfCounterCategory category;
                    if (!this.categories.TryGetValue(t, out category))
                    {
                        this.registerType<T>(storageType);
                    }

                    if (this.categories.TryGetValue(t, out category))
                    {
                        if (!category.HasReferencedInstance(instance))
                        {
                            PerfCounterInstance perfCounterInstance = category.CreateInstance(instanceName, instance);
                            this.start();
                        }
                    }
                }
            }
        }

        [CautionUsedByReflection]
        public void AddInstanceUnTyped(object instance, string instanceName, CounterStorageType storageType)
        {
            if (!this.Enabled)
            {
                return;
            }

            Type t = instance.GetType();
            this.genericAddInstanceMethod.MakeGenericMethod(t).Invoke(this, new[] { instance, instanceName, storageType });
        }

        public PerfCounterCategory[] GetAllCategories()
        {
            lock (this.categories)
            {
                return this.categories.Values.ToArray();
            }
        }

        public PerfCounterCategory GetCategory(string categoryName)
        {
            foreach (var kvp in this.categories)
            {
                if (kvp.Value.CategoryName == categoryName)
                {
                    PerfCounterCategory category = kvp.Value;
                    return category;
                }
            }

            return null;
        }

        public void RemoveCustomPerfCounters(string categoryName)
        {
            lock (this.customCounters)
            {
                Pair<CustomPerfCounters, List<PerformanceCounter>> pair;
                if (this.customCounters.TryGetValue(categoryName, out pair))
                {
                    foreach (PerformanceCounter counter in pair.Second)
                    {
                        try
                        {
                            counter.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                }
            }
        }

        public void RemoveInstance<T>(T instance) where T : class
        {
            Type t = typeof(T);
            lock (this.categories)
            {
                PerfCounterCategory category;
                if (this.categories.TryGetValue(t, out category))
                {
                    category.RemoveInstance(instance);
                    if (category.CounterInstances.Count == 0)
                    {
                        category.Dispose();
                        this.categories.Remove(t);
                    }
                }
            }
        }

        public void RemoveInstanceUnTyped(object instance)
        {
            Type t = instance.GetType();
            lock (this.categories)
            {
                List<Type> types = this.categories.Keys.ToList();
                foreach (Type t1 in types)
                {
                    if (t1.IsAssignableFrom(t))
                    {
                        PerfCounterCategory category = this.categories[t1];
                        category.RemoveInstance(instance);
                        if (category.CounterInstances.Count == 0)
                        {
                            category.Dispose();
                            this.categories.Remove(t1);
                        }
                    }
                }
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (this.task != null)
            {
                this.task.Dispose();
            }
        }

        private void CaptureCounters(object tag)
        {
            if (!this.Enabled)
            {
                return;
            }

            lock (this.categories)
            {
                bool isUpdated = false;
                if (this.categories.Count >= 0)
                {
                    foreach (var kvp in this.categories)
                    {
                        if (kvp.Value.CounterInstances.Count > 0)
                        {
                            this.CaptureCounters(kvp.Value);
                            isUpdated = true;
                        }
                    }
                }

                if (isUpdated)
                {
                    this.notifySubscribers();
                }
            }

            lock (this.customCounters)
            {
                foreach (var kvp in this.customCounters)
                {
                }
            }
        }

        private void CaptureCounters(PerfCounterCategory category)
        {
            try
            {
                string[] instanceNames = category.CounterInstances.Keys.ToArray();
                foreach (string instanceName in instanceNames)
                {
                    PerfCounterInstance instance = category.GetCounterInstance(instanceName);
                    if (instance != null)
                    {
                        object o = category.GetReferencedObject(instanceName);
                        if (o != null)
                        {
                            this.update(instance.Counters, o, category.MetaData.Type);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void disposeAllCounters()
        {
            foreach (var kvp in this.categories)
            {
                kvp.Value.Dispose();
            }

            this.categories.Clear();
        }

        private void notifySubscribers()
        {
            var afterCountersCaptured = this.AfterCountersCaptured;
            if (afterCountersCaptured != null)
            {
                var info = new CounterUpdateInfo();
                foreach (var kvp in this.categories)
                {
                    CounterCategoryInfo categoryInfo = kvp.Value.BuildCounterCategoryInfo();
                    info.Categories.Add(categoryInfo);
                }

                lock (this.customCounters)
                {
                    foreach (var kvp in this.customCounters)
                    {
                        var categoryInfo = new CounterCategoryInfo { CategoryName = kvp.Value.First.CategoryName };
                        info.Categories.Add(categoryInfo);
                        var instanceInfo = new CounterInstanceInfo { InstanceName = "Unique Instance", CounterValues = new double[kvp.Value.First.Counters.Count] };
                        categoryInfo.Instances.Add(instanceInfo);
                        for (int i = 0; i < kvp.Value.First.Counters.Count; i++)
                        {
                            if (kvp.Value.First.Counters[i] != null)
                            {
                                try
                                {
                                    categoryInfo.CounterNames.Add(kvp.Value.First.Counters[i].CounterName);
                                    instanceInfo.CounterValues[i] = kvp.Value.First.Counters[i].IsRaw ? kvp.Value.Second[i].RawValue : kvp.Value.Second[i].NextValue();
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex);
                                }
                            }
                        }
                    }
                }

                try
                {
                    afterCountersCaptured(this, new CountersCapturedEventArgs(info));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        private void registerType<T>(CounterStorageType storageType) where T : class
        {
            Type t = typeof(T);
            if (!this.categories.ContainsKey(t))
            {
                if (DynamicPerfCounterUpdater<T>.MetaData == null)
                {
                    return;
                }

                var category = new PerfCounterCategory(DynamicPerfCounterUpdater<T>.MetaData, storageType == CounterStorageType.MemoryAndPerfmon);
                this.categories.Add(t, category);
            }
        }

        private void start()
        {
            if (this.task == null)
            {
                this.task = new IntervalTask(this.Interval, true, this.StrictInterval);
                this.task.AddAction(this.CaptureCounters, null);
                this.task.Start();
            }
        }

        private void update(IList<PerfCounter> counters, object o, Type objectType)
        {
            if (o != null)
            {
                Type t = typeof(DynamicPerfCounterUpdater<>).MakeGenericType(new[] { objectType });
                t.InvokeMember("Update", BindingFlags.InvokeMethod, null, null, new[] { counters, o });
            }
        }

        #endregion Methods
    }
}
