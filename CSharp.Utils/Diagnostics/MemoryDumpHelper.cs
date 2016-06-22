using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using CSharp.Utils.IO;

namespace CSharp.Utils.Diagnostics
{
    public static class MemoryDumpHelper
    {
        #region Constructors and Finalizers

        static MemoryDumpHelper()
        {
            DefaultMemoryDumpType = MemoryDumpTypes.MiniDumpWithFullMemory;
            CreateDumpOnUnhandledException = false;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DefaultDumpDirectoryPath = IOHelper.ResolvePath(@"\Dumps");
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static bool CreateDumpOnUnhandledException { get; set; }

        public static string DefaultDumpDirectoryPath { get; set; }

        public static MemoryDumpTypes DefaultMemoryDumpType { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public static void CreateCurrentProcessMemoryDump()
        {
            string directoryPath = DefaultDumpDirectoryPath;
            if (!Path.IsPathRooted(directoryPath))
            {
                directoryPath = IOHelper.ResolvePath(directoryPath);
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            Process currentProcess = Process.GetCurrentProcess();
            string fileName = GetMemoryDumpFileName(directoryPath, currentProcess);
            CreateMemoryDump(fileName, currentProcess, DefaultMemoryDumpType);
        }

        public static void CreateMemoryDump(MemoryDumpTypes dumpType, string processIdOrName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(processIdOrName))
            {
                CreateMemoryDump(null, fileName, dumpType);
                return;
            }

            int processId;
            if (int.TryParse(processIdOrName, out processId))
            {
                CreateMemoryDump(processId, fileName, dumpType);
            }
            else
            {
                Process[] processes = Process.GetProcessesByName(processIdOrName);
                if (processes.Length > 0)
                {
                    foreach (Process p in processes)
                    {
                        CreateMemoryDump(fileName, p, dumpType);
                    }
                }
            }
        }

        public static void CreateMemoryDump(int? processId, string fileName, MemoryDumpTypes dumpType)
        {
            Process process = processId == null ? Process.GetCurrentProcess() : Process.GetProcessById(processId.Value);
            CreateMemoryDump(fileName, process, dumpType);
        }

        public static void CreateMemoryDump(string fileName, Process process, MemoryDumpTypes dumpType)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                using (process)
                {
                    NativeMethods.MiniDumpWriteDump(process.Handle, process.Id, fs.SafeFileHandle.DangerousGetHandle(), (int)dumpType, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                }
            }
        }

        public static string GetMemoryDumpFileName(string directoryPath, int? processId)
        {
            Process process = processId == null ? Process.GetCurrentProcess() : Process.GetProcessById(processId.Value);
            return GetMemoryDumpFileName(directoryPath, process);
        }

        public static string GetMemoryDumpFileName(string directoryPath, Process process)
        {
            string dateTime = GlobalSettings.Instance.CurrentDateTime.ToString(CultureInfo.InvariantCulture).Replace('/', '_').Replace(':', '_');
            return string.Format(@"{0}\{1}_{2}_{3}.dmp", directoryPath, process.Id, process.ProcessName, dateTime);
        }

        #endregion Public Methods and Operators

        #region Methods

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (CreateDumpOnUnhandledException)
            {
                CreateCurrentProcessMemoryDump();
            }
        }

        #endregion Methods

        private static class NativeMethods
        {
            #region Methods

            [DllImport("dbghelp.dll")]
            public static extern bool MiniDumpWriteDump(IntPtr hProcess, int ProcessId, IntPtr hFile, int DumpType, IntPtr ExceptionParam, IntPtr UserStreamParam, IntPtr CallackParam);

            #endregion Methods
        }
    }
}
