using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CSharp.Utils.Reflection
{
    public class ComponentUpdateRegistrationInfo : AbstractDisposable
    {
        #region Fields

        private GCHandle _component;

        #endregion Fields
        #region Constructors and Finalizers

        public ComponentUpdateRegistrationInfo(object component, string fileName, string nodePath, OnComponentUpdateCallback callback, IObjectInstantiator instantiator)
        {
            this.Component = component;
            this.FileName = Path.GetFullPath(fileName);
            this.NodePath = nodePath;
            this.Callback = callback;
            this.Instantiator = instantiator;
        }

        #endregion Constructors and Finalizers

        #region Delegates

        public delegate void OnComponentUpdateCallback(ComponentUpdateRegistrationInfo info);

        #endregion Delegates

        #region Public Properties

        public OnComponentUpdateCallback Callback { get; private set; }

        public object Component
        {
            get
            {
                return this._component.Target;
            }

            private set
            {
                if (this._component.IsAllocated)
                {
                    object o = this._component.Target;
                    if (o != null)
                    {
                        this._component.Free();
                    }
                }

                if (value != null)
                {
                    this._component = GCHandle.Alloc(value, GCHandleType.WeakTrackResurrection);
                }
            }
        }

        public string FileName { get; private set; }

        public string NodePath { get; private set; }

        public IObjectInstantiator Instantiator { get; private set; }
        #endregion Public Properties

        #region Public Methods and Operators

        public override bool Equals(object obj)
        {
            var info = obj as ComponentUpdateRegistrationInfo;
            if (info != null)
            {
                return info.Component == this.Component && info.NodePath == this.NodePath && string.Compare(info.FileName, this.FileName, StringComparison.OrdinalIgnoreCase) == 0 && info.Callback == this.Callback;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            return DynamicToStringHelper<ComponentUpdateRegistrationInfo>.ExportAsString(this);
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (this._component.IsAllocated)
            {
                object o = this._component.Target;
                if (o != null)
                {
                    this._component.Free();
                }
            }
        }

        #endregion Methods
    }
}
