using System;
using System.Xml.Serialization;
using CSharp.Utils.Contracts;
using CSharp.Utils.Diagnostics.Performance;

namespace CSharp.Utils
{
    [Serializable]
    [XmlRoot("component")]
    public abstract class AbstractComponent : AbstractInitializableAndDisposable, IUnique
    {
        #region Constructors

        protected AbstractComponent()
        {
            CounterStorageType = CounterStorageType.None;
        }

        #endregion Constructors

        #region Properties

        public CounterStorageType CounterStorageType
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return Id;
        }

        protected override void Dispose(bool disposing)
        {
        }

        protected override void InitializeProtected()
        {
        }

        #endregion Methods
    }
}
