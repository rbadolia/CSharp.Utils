using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Web;

namespace CSharp.Utils.Diagnostics
{
    public static class ProcessHelper
    {
        #region Static Fields

        private static readonly object _lock = new object();

        private static string _currentProcessInstanceName;

        #endregion Static Fields

        #region Constructors and Finalizers

        static ProcessHelper()
        {
            CurrentProcess = Process.GetCurrentProcess();
            CurrentProcessType = GetCurrentProcessType();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static string AssembliesDirectory
        {
            get
            {
                return CurrentProcessType == ProcessType.AspDotNetApplication ? (RootPathOfApplication + "\\bin") : RootPathOfApplication;
            }
        }

        public static Process CurrentProcess { get; private set; }

        public static string CurrentProcessInstanceName
        {
            get
            {
                if (_currentProcessInstanceName == null)
                {
                    lock (_lock)
                    {
                        if (_currentProcessInstanceName == null)
                        {
                            int pid = CurrentProcess.Id;
                            _currentProcessInstanceName = GetProcessInstanceName(pid);
                        }
                    }
                }

                return _currentProcessInstanceName;
            }
        }

        public static ProcessType CurrentProcessType { get; private set; }

        public static int LargeObjectThreshold
        {
            get
            {
                return (85000 - (IntPtr.Size * 5)) / IntPtr.Size;
            }
        }

        public static string RootPathOfApplication
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public static string GetProcessInstanceName(int pid)
        {
            try
            {
                var cat = new PerformanceCounterCategory("Process");
                string[] instances = cat.GetInstanceNames();
                foreach (string instance in instances)
                {
                    using (var cnt = new PerformanceCounter("Process", "Id Process", instance, true))
                    {
                        var val = (int)cnt.RawValue;
                        if (val == pid)
                        {
                            return instance;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return null;
        }

        public static bool IsRunningAsService(string serviceName)
        {
            IntPtr serviceManagerHandle = WinApi.OpenSCManager(null, null, (uint)WinApi.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
            if (serviceManagerHandle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            IntPtr serviceHandle = WinApi.OpenService(serviceManagerHandle, serviceName, (uint)WinApi.SERVICE_ACCESS.SERVICE_ALL_ACCESS);
            if (serviceHandle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            var serviceStatus = new WinApi.SERVICE_STATUS_PROCESS();
            var buffer = new byte[1000];
            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                int bytesNeeded;
                bool success = WinApi.QueryServiceStatusEx(serviceHandle, WinApi.SC_STATUS_PROCESS_INFO, buffer, 1000, out bytesNeeded);
                if (!success)
                {
                    throw new Win32Exception();
                }

                IntPtr buffIntPtr = bufferHandle.AddrOfPinnedObject();
                Marshal.PtrToStructure(buffIntPtr, serviceStatus);
            }
            finally
            {
                bufferHandle.Free();
            }

            WinApi.CloseServiceHandle(serviceHandle);
            WinApi.CloseServiceHandle(serviceManagerHandle);

            return CurrentProcess.Id == serviceStatus.processID;
        }

        public static bool IsService(int processId)
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service WHERE ProcessId =" + "\"" + processId + "\""))
            {
                ManagementObjectCollection collection = searcher.Get();
                return collection.Count > 0;
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        public static ProcessType GetCurrentProcessType()
        {
            if (HttpContext.Current != null)
            {
                return ProcessType.AspDotNetApplication;
            }

            if (IsService(CurrentProcess.Id))
            {
                return ProcessType.WindowsService;
            }

            if (CurrentProcess.MainWindowHandle != IntPtr.Zero)
            {
                return ProcessType.WindowsApplication;
            }

            return ProcessType.ConsoleApplication;
        }

        #endregion Methods
    }
}
