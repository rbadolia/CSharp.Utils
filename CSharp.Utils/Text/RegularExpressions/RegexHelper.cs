using System.Text.RegularExpressions;

namespace CSharp.Utils.Text.RegularExpressions
{
    public static class RegexHelper
    {
        public const string EnglishAlphaNumericRegex = "^[a-zA-Z0-9]*$";

        private static string WildcardToRegexString(string pattern)
        {
            string result = Regex.Escape(pattern).Replace(@"\*", ".+?").Replace(@"\?", ".");

            if (result.EndsWith(".+?"))
            {
                result = result.Remove(result.Length - 3, 3);
                result += ".*";
            }

            return result;
        }

        public static bool IsWildcardMatched(string s, string wildcard, bool isCaseSensitive)
        {
            Regex regex = BuildRegexFromWildcard(wildcard, isCaseSensitive);
            return regex.IsMatch(s);
        }

        public static Regex BuildRegexFromWildcard(string wildcard, bool isCaseSensitive)
        {
            string wildcardRegex = WildcardToRegexString(wildcard);
            if (isCaseSensitive)
            {
                return new Regex(wildcardRegex);
            }

            return new Regex(wildcardRegex, RegexOptions.IgnoreCase);
        }
    }
}
