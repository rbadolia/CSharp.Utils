using System;
using CSharp.Utils.Diagnostics;

namespace CSharp.Utils
{
    public sealed class GlobalSettings
    {
        #region Static Fields

        private static readonly GlobalSettings InstanceObject = new GlobalSettings();

        #endregion Static Fields

        #region Fields

        private bool _isService;

        #endregion Fields

        #region Constructors and Finalizers

        private GlobalSettings()
        {
            this.UseUtcTime = false;
            this.Environment = "DEV";
            this.MachineName = System.Environment.MachineName;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static GlobalSettings Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public string ApplicationName { get; set; }

        public DateTime CurrentDateTime
        {
            get
            {
                return this.UseUtcTime ? DateTime.UtcNow : DateTime.Now;
            }
        }

        public string Environment { get; set; }

        public bool IsService
        {
            get
            {
                return this._isService;
            }

            set
            {
                this._isService = value;
                if (!value && this.ApplicationName == null)
                {
                    this.ApplicationName = ProcessHelper.CurrentProcessInstanceName;
                }
            }
        }

        public string MachineName { get; private set; }

        public bool UseUtcTime { get; set; }

        #endregion Public Properties
    }
}
