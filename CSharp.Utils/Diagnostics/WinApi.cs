using System;
using System.Runtime.InteropServices;

namespace CSharp.Utils.Diagnostics
{
    public static class WinApi
    {
        #region Constants

        public const int SC_STATUS_PROCESS_INFO = 0;

        #endregion Constants

        #region Enums

        [Flags]
        public enum SCM_ACCESS : uint
        {
            SC_MANAGER_CONNECT = 0x00001, 

            SC_MANAGER_CREATE_SERVICE = 0x00002, 

            SC_MANAGER_ENUMERATE_SERVICE = 0x00004, 

            SC_MANAGER_LOCK = 0x00008, 

            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010, 

            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020, 

            SC_MANAGER_ALL_ACCESS = ACCESS_MASK.STANDARD_RIGHTS_REQUIRED | SC_MANAGER_CONNECT | SC_MANAGER_CREATE_SERVICE | SC_MANAGER_ENUMERATE_SERVICE | SC_MANAGER_LOCK | SC_MANAGER_QUERY_LOCK_STATUS | SC_MANAGER_MODIFY_BOOT_CONFIG, 

            GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ | SC_MANAGER_ENUMERATE_SERVICE | SC_MANAGER_QUERY_LOCK_STATUS, 

            GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE | SC_MANAGER_CREATE_SERVICE | SC_MANAGER_MODIFY_BOOT_CONFIG, 

            GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE | SC_MANAGER_CONNECT | SC_MANAGER_LOCK, 

            GENERIC_ALL = SC_MANAGER_ALL_ACCESS, 
        }

        [Flags]
        public enum SERVICE_ACCEPT : uint
        {
            STOP = 0x00000001, 

            PAUSE_CONTINUE = 0x00000002, 

            SHUTDOWN = 0x00000004, 

            PARAMCHANGE = 0x00000008, 

            NETBINDCHANGE = 0x00000010, 

            HARDWAREPROFILECHANGE = 0x00000020, 

            POWEREVENT = 0x00000040, 

            SESSIONCHANGE = 0x00000080, 
        }

        [Flags]
        public enum SERVICE_ACCESS : uint
        {
            STANDARD_RIGHTS_REQUIRED = 0xF0000, 

            SERVICE_QUERY_CONFIG = 0x00001, 

            SERVICE_CHANGE_CONFIG = 0x00002, 

            SERVICE_QUERY_STATUS = 0x00004, 

            SERVICE_ENUMERATE_DEPENDENTS = 0x00008, 

            SERVICE_START = 0x00010, 

            SERVICE_STOP = 0x00020, 

            SERVICE_PAUSE_CONTINUE = 0x00040, 

            SERVICE_INTERROGATE = 0x00080, 

            SERVICE_USER_DEFINED_CONTROL = 0x00100, 

            SERVICE_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG | SERVICE_QUERY_STATUS | SERVICE_ENUMERATE_DEPENDENTS | SERVICE_START | SERVICE_STOP | SERVICE_PAUSE_CONTINUE | SERVICE_INTERROGATE | SERVICE_USER_DEFINED_CONTROL
        }

        [Flags]
        public enum SERVICE_CONTROL : uint
        {
            STOP = 0x00000001, 

            PAUSE = 0x00000002, 

            CONTINUE = 0x00000003, 

            INTERROGATE = 0x00000004, 

            SHUTDOWN = 0x00000005, 

            PARAMCHANGE = 0x00000006, 

            NETBINDADD = 0x00000007, 

            NETBINDREMOVE = 0x00000008, 

            NETBINDENABLE = 0x00000009, 

            NETBINDDISABLE = 0x0000000A, 

            DEVICEEVENT = 0x0000000B, 

            HARDWAREPROFILECHANGE = 0x0000000C, 

            POWEREVENT = 0x0000000D, 

            SESSIONCHANGE = 0x0000000E
        }

        public enum SERVICE_STATE : uint
        {
            SERVICE_STOPPED = 0x00000001, 

            SERVICE_START_PENDING = 0x00000002, 

            SERVICE_STOP_PENDING = 0x00000003, 

            SERVICE_RUNNING = 0x00000004, 

            SERVICE_CONTINUE_PENDING = 0x00000005, 

            SERVICE_PAUSE_PENDING = 0x00000006, 

            SERVICE_PAUSED = 0x00000007
        }

        [Flags]
        public enum SERVICE_TYPES
        {
            SERVICE_KERNEL_DRIVER = 0x00000001, 

            SERVICE_FILE_SYSTEM_DRIVER = 0x00000002, 

            SERVICE_WIN32_OWN_PROCESS = 0x00000010, 

            SERVICE_WIN32_SHARE_PROCESS = 0x00000020, 

            SERVICE_INTERACTIVE_PROCESS = 0x00000100
        }

        [Flags]
        private enum ACCESS_MASK : uint
        {
            DELETE = 0x00010000, 

            READ_CONTROL = 0x00020000, 

            WRITE_DAC = 0x00040000, 

            WRITE_OWNER = 0x00080000, 

            SYNCHRONIZE = 0x00100000, 

            STANDARD_RIGHTS_REQUIRED = 0x000f0000, 

            STANDARD_RIGHTS_READ = 0x00020000, 

            STANDARD_RIGHTS_WRITE = 0x00020000, 

            STANDARD_RIGHTS_EXECUTE = 0x00020000, 

            STANDARD_RIGHTS_ALL = 0x001f0000, 

            SPECIFIC_RIGHTS_ALL = 0x0000ffff, 

            ACCESS_SYSTEM_SECURITY = 0x01000000, 

            MAXIMUM_ALLOWED = 0x02000000, 

            GENERIC_READ = 0x80000000, 

            GENERIC_WRITE = 0x40000000, 

            GENERIC_EXECUTE = 0x20000000, 

            GENERIC_ALL = 0x10000000, 

            DESKTOP_READOBJECTS = 0x00000001, 

            DESKTOP_CREATEWINDOW = 0x00000002, 

            DESKTOP_CREATEMENU = 0x00000004, 

            DESKTOP_HOOKCONTROL = 0x00000008, 

            DESKTOP_JOURNALRECORD = 0x00000010, 

            DESKTOP_JOURNALPLAYBACK = 0x00000020, 

            DESKTOP_ENUMERATE = 0x00000040, 

            DESKTOP_WRITEOBJECTS = 0x00000080, 

            DESKTOP_SWITCHDESKTOP = 0x00000100, 

            WINSTA_ENUMDESKTOPS = 0x00000001, 

            WINSTA_READATTRIBUTES = 0x00000002, 

            WINSTA_ACCESSCLIPBOARD = 0x00000004, 

            WINSTA_CREATEDESKTOP = 0x00000008, 

            WINSTA_WRITEATTRIBUTES = 0x00000010, 

            WINSTA_ACCESSGLOBALATOMS = 0x00000020, 

            WINSTA_EXITWINDOWS = 0x00000040, 

            WINSTA_ENUMERATE = 0x00000100, 

            WINSTA_READSCREEN = 0x00000200, 

            WINSTA_ALL_ACCESS = 0x0000037f
        }

        #endregion Enums

        #region Public Methods and Operators

        public static bool CloseServiceHandle(IntPtr hscObject)
        {
            return NativeMethods.CloseServiceHandle(hscObject);
        }

        public static bool ControlService(IntPtr hService, SERVICE_CONTROL dwControl, ref SERVICE_STATUS lpServiceStatus)
        {
            return NativeMethods.ControlService(hService, dwControl, ref lpServiceStatus);
        }

        public static IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess)
        {
            return NativeMethods.OpenSCManager(machineName, databaseName, dwAccess);
        }

        public static IntPtr OpenService(IntPtr hscManager, string lpServiceName, uint dwDesiredAccess)
        {
            return NativeMethods.OpenService(hscManager, lpServiceName, dwDesiredAccess);
        }

        public static bool QueryServiceStatusEx(IntPtr serviceHandle, int infoLevel, byte[] buffer, int bufferSize, out int bytesNeeded)
        {
            return NativeMethods.QueryServiceStatusEx(serviceHandle, infoLevel, buffer, bufferSize, out bytesNeeded);
        }

        #endregion Public Methods and Operators

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SERVICE_STATUS
        {
            #region Fields

            public static readonly int SizeOf = Marshal.SizeOf(typeof(SERVICE_STATUS));

            public SERVICE_TYPES dwServiceType;

            public SERVICE_STATE dwCurrentState;

            public uint dwControlsAccepted;

            public uint dwWin32ExitCode;

            public uint dwServiceSpecificExitCode;

            public uint dwCheckPoint;

            public uint dwWaitHint;

            #endregion Fields
        }

        private static class NativeMethods
        {
            #region Methods

            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseServiceHandle(IntPtr hscObject);

            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ControlService(IntPtr hService, SERVICE_CONTROL dwControl, ref SERVICE_STATUS lpServiceStatus);

            [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr OpenService(IntPtr hscManager, string lpServiceName, uint dwDesiredAccess);

            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool QueryServiceStatusEx(IntPtr serviceHandle, int infoLevel, byte[] buffer, int bufferSize, out int bytesNeeded);

            #endregion Methods
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class SERVICE_STATUS_PROCESS
        {
            public int serviceType;

            public int currentState;

            public int controlsAccepted;

            public int win32ExitCode;

            public int serviceSpecificExitCode;

            public int checkPoint;

            public int waitHint;

            public int processID;

            public int serviceFlags;
        }
    }
}
