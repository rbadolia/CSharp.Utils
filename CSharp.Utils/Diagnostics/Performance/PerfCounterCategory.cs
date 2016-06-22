using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CSharp.Utils.Diagnostics.Performance.Dtos;

namespace CSharp.Utils.Diagnostics.Performance
{
    [Serializable]
    public sealed class PerfCounterCategory : AbstractDisposable
    {
        #region Fields

        [NonSerialized]
        private readonly CounterCreationDataCollection collection;

        private readonly List<KeyValuePair<string, bool>> _counterNames = new List<KeyValuePair<string, bool>>();

        [NonSerialized]
        private readonly TypeMetaData _metadata;

        private readonly List<string> _referenceTypeCounterNames = new List<string>();

        private readonly List<string> _windowsCounterNames = new List<string>();

        #endregion Fields

        #region Constructors and Finalizers

        public PerfCounterCategory(TypeMetaData metadata, bool hasWindowsCategory)
        {
            this.CategoryType = PerformanceCounterCategoryType.MultiInstance;
            this._metadata = metadata;
            if (hasWindowsCategory)
            {
                this.collection = new CounterCreationDataCollection();
            }

            this.initialize();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string CategoryHelp { get; private set; }

        public string CategoryName { get; private set; }

        public PerformanceCounterCategoryType CategoryType { get; set; }

        public Dictionary<string, PerfCounterInstance> CounterInstances { get; private set; }

        public TypeMetaData MetaData
        {
            get
            {
                return this._metadata;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public CounterCategoryInfo BuildCounterCategoryInfo()
        {
            var counterCategoryInfo = new CounterCategoryInfo { CategoryName = this.CategoryName, CounterNames = this._windowsCounterNames, ReferenceTypeCounterNames = this._referenceTypeCounterNames };
            foreach (var counterInstance in this.CounterInstances)
            {
                var counterInstanceInfo = new CounterInstanceInfo { InstanceName = counterInstance.Value.InstanceName, CounterValues = new double[this._windowsCounterNames.Count], ReferenceTypeCounterValues = new object[this._referenceTypeCounterNames.Count] };
                int j = 0;
                int k = 0;
                for (int i = 0; i < this._counterNames.Count; i++)
                {
                    if (this._counterNames[i].Value)
                    {
                        counterInstanceInfo.CounterValues[j] = counterInstance.Value.Counters[i].RawValue;
                        j++;
                    }
                    else
                    {
                        counterInstanceInfo.ReferenceTypeCounterValues[k] = counterInstance.Value.Counters[i].Tag;
                        k++;
                    }
                }

                counterCategoryInfo.Instances.Add(counterInstanceInfo);
            }

            return counterCategoryInfo;
        }

        public PerfCounterInstance CreateInstance(string instanceName, object referenceObject)
        {
            var instance = new PerfCounterInstance { InstanceName = instanceName };
            foreach (var kvp in this._counterNames)
            {
                var counter = new PerfCounter();
                instance.Counters.Add(counter);
                if (kvp.Value && this.collection != null)
                {
                    var c = new PerformanceCounter(this.CategoryName, kvp.Key, instanceName, ".") { ReadOnly = false };
                    counter.SetAdaptedObject(c);
                }
            }

            instance.ReferenceObject = referenceObject;
            this.CounterInstances.Add(instanceName, instance);
            return instance;
        }

        public DataTable ExportAsDataTable(bool includeTimeStamp, bool onlyMetaData)
        {
            var table = new DataTable("Data");
            if (includeTimeStamp)
            {
                table.Columns.Add("TimeStamp", typeof(DateTime));
            }

            table.Columns.Add("InstanceName", typeof(string));
            foreach (var kvp in this._counterNames)
            {
                table.Columns.Add(kvp.Key.PutSpaceAtUpperCasing(), kvp.Value ? typeof(double) : typeof(object));
            }

            if (onlyMetaData)
            {
                table.AcceptChanges();
                return table;
            }

            PerfCounterInstance[] instances = this.GetAllInstances();
            foreach (PerfCounterInstance instance in instances)
            {
                DataRow row = table.NewRow();
                int k = 0;
                if (includeTimeStamp)
                {
                    row[0] = GlobalSettings.Instance.CurrentDateTime;
                    k++;
                }

                row[k] = instance.InstanceName;
                k++;
                for (int i = 0; i < this._counterNames.Count; i++)
                {
                    row[i + k] = this._counterNames[i].Value ? instance.Counters[i].RawValue : instance.Counters[i].Tag;
                }

                table.Rows.Add(row);
                table.AcceptChanges();
            }

            return table;
        }

        public DataTable ExportAsTransposedDataTable(bool includeGroupNames)
        {
            const int inc = 1;
            var table = new DataTable("data");
            table.Columns.Add("Counter CategoryName", typeof(string));

            PerfCounterInstance[] inastances = this.GetAllInstances();
            foreach (PerfCounterInstance counterInstance in inastances)
            {
                table.Columns.Add(counterInstance.InstanceName, typeof(object));
            }

            int propertyIndex = 0;
            foreach (var counterNamePair in this._counterNames)
            {
                DataRow row = table.NewRow();
                row[0] = counterNamePair.Key;
                for (int k = 0; k < inastances.Length; k++)
                {
                    if (counterNamePair.Value)
                    {
                        row[k + inc] = inastances[k].Counters[propertyIndex].RawValue;
                    }
                    else
                    {
                        row[k + inc] = inastances[k].Counters[propertyIndex].Tag ?? DBNull.Value;
                    }
                }

                table.Rows.Add(row);
                propertyIndex++;
            }

            return table;
        }

        public PerfCounterInstance[] GetAllInstances()
        {
            return this.CounterInstances.Values.ToArray();
        }

        public PerfCounterInstance GetCounterInstance(string instanceName)
        {
            PerfCounterInstance instance;
            if (this.CounterInstances.TryGetValue(instanceName, out instance))
            {
                object o = instance.ReferenceObject;
                if (o == null)
                {
                    instance.Dispose();
                    this.CounterInstances.Remove(instance.InstanceName);
                    return null;
                }
            }

            return instance;
        }

        public object GetReferencedObject(string instanceName)
        {
            PerfCounterInstance instance = this.GetCounterInstance(instanceName);
            if (instanceName != null)
            {
                return instance.ReferenceObject;
            }

            return null;
        }

        public bool HasReferencedInstance(object referencedObject)
        {
            PerfCounterInstance instance = this.getByReferencedObject(referencedObject);
            return instance != null;
        }

        public void RemoveInstance(object referencedObject)
        {
            PerfCounterInstance instance = this.getByReferencedObject(referencedObject);
            if (instance != null)
            {
                instance.Dispose();
                this.CounterInstances.Remove(instance.InstanceName);
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            foreach (var kvp in this.CounterInstances)
            {
                kvp.Value.Dispose();
            }

            this.CounterInstances.Clear();
            if (this.collection != null)
            {
                PerformanceCounterCategory.Delete(this.CategoryName);
            }
        }

        private void AddCollectionData(PerformanceCounterType counterType, string counterName)
        {
            if (this.collection != null)
            {
                var data = new CounterCreationData { CounterType = counterType, CounterName = counterName };
                this.collection.Add(data);
            }
        }

        private void Create()
        {
            if (this.collection != null)
            {
                if (PerformanceCounterCategory.Exists(this.CategoryName))
                {
                    PerformanceCounterCategory.Delete(this.CategoryName);
                }

                PerformanceCounterCategory.Create(this.CategoryName, this.CategoryHelp, this.CategoryType, this.collection);
            }
        }

        private PerfCounterInstance getByReferencedObject(object referencedObject)
        {
            var thingsToRemove = new List<PerfCounterInstance>();
            PerfCounterInstance instance = null;
            foreach (var kvp in this.CounterInstances)
            {
                object obj = kvp.Value.ReferenceObject;
                if (obj == null)
                {
                    thingsToRemove.Add(kvp.Value);
                }
                else
                {
                    if (obj == referencedObject)
                    {
                        instance = kvp.Value;
                        break;
                    }
                }
            }

            foreach (PerfCounterInstance thingToRemove in thingsToRemove)
            {
                this.CounterInstances.Remove(thingToRemove.InstanceName);
                thingToRemove.Dispose();
            }

            return instance;
        }

        private void initialize()
        {
            this.CounterInstances = new Dictionary<string, PerfCounterInstance>();
            this.CategoryName = this._metadata.CategoryName;
            this.CategoryHelp = this._metadata.CategoryHelp;

            foreach (PropertyMetaData pMetadata in this._metadata.Properties)
            {
                if (pMetadata.IsValidForWindowsPerfCounter)
                {
                    this.AddCollectionData(PerformanceCounterType.NumberOfItems64, pMetadata.CounterName);
                    this._counterNames.Add(new KeyValuePair<string, bool>(pMetadata.CounterName, true));
                    this._windowsCounterNames.Add(pMetadata.CounterName);
                }
                else
                {
                    this._counterNames.Add(new KeyValuePair<string, bool>(pMetadata.CounterName, false));
                    this._referenceTypeCounterNames.Add(pMetadata.CounterName);
                }
            }

            this.Create();
        }

        #endregion Methods
    }
}
