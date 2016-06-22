using System.Collections.Concurrent;
using CSharp.Utils.Validation;

namespace CSharp.Utils.DirectoryServices
{
    public static class ActiveDirectoryUserCache
    {
        private static ConcurrentDictionary<string, ActiveDirectoryUserInfo> _dictionary = new ConcurrentDictionary<string, ActiveDirectoryUserInfo>();

        public static ActiveDirectoryUserInfo GetActiveDirectoryUserInfo(string samAccountName)
        {
            Guard.ArgumentNotNullOrEmpty(samAccountName, "samAccountName");
            samAccountName = samAccountName.ToUpper();
            return _dictionary.GetOrAdd(samAccountName, (string an)=>
            {
                return ActiveDirectoryHelper.GetActiveDirectoryUserInfo(an);
            });
        }
    }
}
