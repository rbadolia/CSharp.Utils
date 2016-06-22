using System.Configuration;
using System.Globalization;
using System.Threading;

namespace CSharp.Utils.Globalization
{
    public static class CultureHelper
    {
        #region Public Properties

        public static string CultureNameAppSettingsKey
        {
            get
            {
                return "CultureName";
            }
        }

        public static string DefaultCulture
        {
            get
            {
                return "en-US";
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public static void SetCulture()
        {
            string culture = ConfigurationManager.AppSettings[CultureNameAppSettingsKey];
            SetCulture(culture);
        }

        public static void SetCulture(string culture)
        {
            SetCulture(culture, Thread.CurrentThread);
        }

        public static void SetCulture(Thread thread)
        {
            string culture = ConfigurationManager.AppSettings[CultureNameAppSettingsKey];
            SetCulture(culture, Thread.CurrentThread);
        }

        public static void SetCulture(string culture, Thread thread)
        {
            thread.CurrentCulture = thread.CurrentUICulture = new CultureInfo(string.IsNullOrEmpty(culture) ? DefaultCulture : culture);
        }

        #endregion Public Methods and Operators
    }
}
