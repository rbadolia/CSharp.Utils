using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace CSharp.Utils.Security
{
    public sealed class Impersonator : AbstractDisposable
    {
        #region Fields

        private WindowsImpersonationContext impersonationContext;

        #endregion Fields

        #region Constructors and Finalizers

        public Impersonator(string userName, string domainName, string password)
        {
            this.impersonateUser(userName, domainName, password);
        }

        #endregion Constructors and Finalizers

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.undoImpersonation();
            if (this.impersonationContext != null)
            {
                this.impersonationContext.Dispose();
            }
        }

        private static IntPtr logOnUser(string userName, string domainName, string password)
        {
            IntPtr token = IntPtr.Zero;
            if (NativeMethods.LogonUser(userName, domainName, password, NativeMethods.LOGON32_LOGON_INTERACTIVE, NativeMethods.LOGON32_PROVIDER_DEFAULT, ref token) != 0)
            {
                return token;
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        private static void revertToSelf()
        {
            if (!NativeMethods.RevertToSelf())
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private void createImpersonationContext(IntPtr token)
        {
            IntPtr tokenDuplicate = IntPtr.Zero;
            try
            {
                if (NativeMethods.DuplicateToken(token, 2, ref tokenDuplicate) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                using (var tempWindowsIdentity = new WindowsIdentity(tokenDuplicate))
                {
                    this.impersonationContext = tempWindowsIdentity.Impersonate();
                }
            }
            finally
            {
                if (tokenDuplicate != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(tokenDuplicate);
                }
            }
        }

        private void impersonateUser(string userName, string domainName, string password)
        {
            revertToSelf();
            IntPtr token = IntPtr.Zero;
            try
            {
                token = logOnUser(userName, domainName, password);
                this.createImpersonationContext(token);
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(token);
                }
            }
        }

        private void undoImpersonation()
        {
            if (this.impersonationContext != null)
            {
                this.impersonationContext.Undo();
            }
        }

        #endregion Methods

        private static class NativeMethods
        {
            #region Fields

            internal const int LOGON32_LOGON_INTERACTIVE = 2;

            internal const int LOGON32_PROVIDER_DEFAULT = 0;

            #endregion Fields

            #region Methods

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CloseHandle(IntPtr handle);

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);

            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern int LogonUser(string lpszUserName, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool RevertToSelf();

            #endregion Methods
        }
    }
}
