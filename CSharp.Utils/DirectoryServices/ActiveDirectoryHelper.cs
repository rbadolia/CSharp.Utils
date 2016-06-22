using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Principal;
using System.Web;
using CSharp.Utils.Diagnostics;
using CSharp.Utils.Validation;

namespace CSharp.Utils.DirectoryServices
{
    public static class ActiveDirectoryHelper
    {
        private const string FIRST_NAME_PROPERTY_NAME = "givenname";

        private const string LAST_NAME_PROPERTY_NAME = "sn";

        private const string EMAIL_NAME_PROPERTY_NAME = "mail";

        private const string MIDDLE_NAME_PROPERTY_NAME = "initials";

        private static DirectoryEntry _directoryEntry = new DirectoryEntry("LDAP://ES-AREA1");

        [ThreadStatic]
        private static string _userName = null;

        public static string GetCurrentUserSamAccountName()
        {
            if (_userName != null)
            {
                return _userName;
            }
            var processType = ProcessHelper.GetCurrentProcessType();
            if (processType == ProcessType.AspDotNetApplication)
            {
                return HttpContext.Current.User.Identity.Name;
            }
            else
            {
                return WindowsIdentity.GetCurrent().Name;
            }
        }

        public static string GetUserNameOnly(string samAccountName)
        {
            Guard.ArgumentNotNullOrEmpty(samAccountName, "samAccountName");
            var splits = samAccountName.Split('\\');
            return splits[splits.Length - 1];
        }

        public static string GetCurrentUserNameOnly()
        {
            var samAccountName = GetCurrentUserSamAccountName();
            return GetUserNameOnly(samAccountName);
        }

        public static void SetCurrentUser(string userName)
        {
            if (userName == null)
            {
                _userName = null;
            }
            else
            {
                _userName = GetUserNameOnly(userName);
            }
        }

        public static UserPrincipal GetUserPrincipal(string puid)
        {
            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain);
            var userPrincipal = UserPrincipal.FindByIdentity(principalContext, puid);
            return userPrincipal;
        }

        public static ActiveDirectoryUserInfo GetActiveDirectoryInfoForCurrentUser()
        {
            var  puid = GetCurrentUserNameOnly();
            return GetActiveDirectoryUserInfo(puid);
        }

        [StateIntact]
        public static ActiveDirectoryUserInfo GetActiveDirectoryUserInfo(string samAccountName, bool searchInTrustedDomainsAsWell = false)
        {
            Guard.ArgumentNotNullOrEmpty(samAccountName, "samAccountName");
            var forest = Forest.GetCurrentForest();
            var userInfo = GetActiveDirectoryUserInfoInForest(samAccountName, forest.Name);
            if (userInfo != null && !searchInTrustedDomainsAsWell)
            {
                return userInfo;
            }

            return null;
        }

        [StateIntact]
        private static ActiveDirectoryUserInfo GetActiveDirectoryUserInfoInForest(string samAccountName, string forestName)
        {
            DirectoryEntry entry = new DirectoryEntry(string.Format("GC://{0}", forestName));
            DirectorySearcher searcher = new DirectorySearcher(entry);
            searcher.Filter = "(&(objectClass=user)(objectCategory=person)(SAMAccountName=" + samAccountName + "))";
            searcher.PropertiesToLoad.Add(FIRST_NAME_PROPERTY_NAME);
            searcher.PropertiesToLoad.Add(LAST_NAME_PROPERTY_NAME);
            searcher.PropertiesToLoad.Add(EMAIL_NAME_PROPERTY_NAME);
            searcher.PropertiesToLoad.Add(MIDDLE_NAME_PROPERTY_NAME);
            SearchResult result = searcher.FindOne();
            if (result == null)
            {
                return null;
            }

            var userInfo = new ActiveDirectoryUserInfo();
            userInfo.FirstName = GetPropertyValue(result, FIRST_NAME_PROPERTY_NAME);
            userInfo.LastName = GetPropertyValue(result, LAST_NAME_PROPERTY_NAME);
            userInfo.EmailId = GetPropertyValue(result, EMAIL_NAME_PROPERTY_NAME);
            userInfo.MiddleName = GetPropertyValue(result, MIDDLE_NAME_PROPERTY_NAME);
            return userInfo;
        }

        private static string GetPropertyValue(SearchResult result, string propertyName)
        {
            var collection = result.Properties[propertyName];
            if(collection==null || collection.Count==0)
            {
                return null;
            }

            return (string)collection[0];
        }
    }
}
