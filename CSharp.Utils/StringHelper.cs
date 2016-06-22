using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using CSharp.Utils.Reflection;

namespace CSharp.Utils
{
    public static class StringHelper
    {
        #region Static Fields

        private static string _defaultDateFormat = "dd/MM/yyyy HH:mm:ss.ff";

        #endregion Static Fields

        #region Public Properties

        public static string DefaultDateFormat
        {
            get { return _defaultDateFormat; }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _defaultDateFormat = value;
                }
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public static HashSet<string> BuildHashSet(this string s, StringSplitOptions stringSplitOptions, 
            params char[] separators)
        {
            string[] splits = s.Split(separators, stringSplitOptions);
            var hashSet = new HashSet<string>();
            foreach (string split in splits)
            {
                hashSet.Add(split);
            }

            return hashSet;
        }

        public static HashSet<string> BuildHashSet(this string s, char separator = ',', 
            StringSplitOptions stringSplitOptions = StringSplitOptions.RemoveEmptyEntries)
        {
            return BuildHashSet(s, stringSplitOptions, ',');
        }

        public static bool ContainsByIgnoringCase(this IEnumerable<string> strings, string s)
        {
            foreach (string v in strings)
            {
                if (string.Compare(s, v, true, CultureInfo.InvariantCulture) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static string ConvertDateTimeToString(this DateTime dateTime)
        {
            return dateTime.ToString(_defaultDateFormat, CultureInfo.InvariantCulture);
        }

        public static string ExceptionToString(Exception ex)
        {
            var sb = new StringBuilder();
            sb.Append(ex);
            sb.Append("\r\n");
            ex = ex.InnerException;
            int depth = 1;
            while (ex != null)
            {
                sb.Append("Inner Exception: ");
                sb.Append(depth.ToString(CultureInfo.InvariantCulture));
                sb.Append("\r\n");
                sb.Append(ex);
                sb.Append("\r\n");
                depth++;
                ex = ex.InnerException;
            }

            return sb.ToString();
        }

        public static string GenerateUniqueString()
        {
            return Guid.NewGuid().ToString().Replace("-", null);
        }

        public static string GetMatchByIgnoringCase(this IEnumerable<string> strings, string s)
        {
            foreach (string v in strings)
            {
                if (string.Compare(s, v, true, CultureInfo.InvariantCulture) == 0)
                {
                    return v;
                }
            }

            return null;
        }

        [CautionUsedByReflection]
        public static string GetObjectAsString(object o)
        {
            return o == null ? null : o.ToString();
        }

        public static string GetObjectsAsString(string name, IEnumerable enumerable)
        {
            using (var writer = new StringWriter())
            {
                WriteObjectsAsString(name, enumerable, writer);
                return writer.ToString();
            }
        }

        public static int IndexOfByIgnoringCase(IList<string> strings, string s)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                if (string.Compare(s, strings[i], true, CultureInfo.InvariantCulture) == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool Match(string s, string pattern, bool emptyMatch)
        {
            int index = pattern.IndexOf('*');
            if (index < 0)
            {
                return s.Equals(pattern);
            }

            if (index > s.Length || !s.Substring(0, index).Equals(pattern.Substring(0, index)))
            {
                return false;
            }

            int nextStartIndex = pattern.Length - index - 1;
            if (nextStartIndex > s.Length)
            {
                return false;
            }

            int remaining = s.Length - nextStartIndex;
            return remaining >= index && (emptyMatch || remaining != index) &&
                   s.Substring(remaining, s.Length - remaining)
                       .Equals(pattern.Substring(index + 1, pattern.Length - index - 1));
        }

        public static void MutableAssign(this string oldValue, string newValue)
        {
            if (oldValue.Length == newValue.Length)
            {
                mutableAssign(oldValue, newValue);
            }
            else
            {
                throw new ArgumentException(@"oldValue and newValue are not in equal size");
            }
        }

        public static void MutableCamelCase(this string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                MutableSetChar(s, 0, char.ToLower(s[0]));
            }
        }

        public static void MutablePascalCase(this string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                MutableSetChar(s, 0, char.ToUpper(s[0]));
            }
        }

        public static void MutableReplace(this string s, char oldChar, char newChar)
        {
            int length = s.Length;
            GCHandle handle = GCHandle.Alloc(s, GCHandleType.Pinned);
            IntPtr iPtr = handle.AddrOfPinnedObject();
            unsafe
            {
                var p = (char*) iPtr.ToPointer();
                for (int i = 0; i < length; i++)
                {
                    if (*p == oldChar)
                    {
                        *p = newChar;
                    }

                    p++;
                }
            }

            handle.Free();
        }

        public static void MutableReplace(this string s, string oldValue, string newValue)
        {
            if (oldValue.Length == newValue.Length)
            {
                int length = s.Length;
                GCHandle handle = GCHandle.Alloc(s, GCHandleType.Pinned);
                IntPtr iPtr = handle.AddrOfPinnedObject();
                unsafe
                {
                    var p = (char*) iPtr.ToPointer();
                    int previousIndex = 0;
                    while (true)
                    {
                        int index = s.IndexOf(oldValue, previousIndex, StringComparison.Ordinal);
                        if (index > -1)
                        {
                            char* q = p + index;
                            foreach (char c in newValue)
                            {
                                *q = c;
                                q++;
                            }

                            previousIndex = index;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                handle.Free();
            }
            else
            {
                throw new ArgumentException("oldValue and newValue are not in equal size");
            }
        }

        public static void MutableReverse(string s)
        {
            GCHandle handle = GCHandle.Alloc(s, GCHandleType.Pinned);
            IntPtr iPtr = handle.AddrOfPinnedObject();
            int len = s.Length/2;
            unsafe
            {
                var p = (char*) iPtr.ToPointer();
                for (int i = 0; i < len; i++)
                {
                    char c = *(p + i);
                    *(p + i) = s[s.Length - i - 1];
                    *(p + s.Length - i - 1) = c;
                }
            }

            handle.Free();
        }

        public static void MutableSetChar(this string s, int at, char c)
        {
            int length = s.Length;
            GCHandle handle = GCHandle.Alloc(s, GCHandleType.Pinned);
            IntPtr iPtr = handle.AddrOfPinnedObject();
            unsafe
            {
                var p = (char*) iPtr.ToPointer();
                *(p + at) = c;
            }

            handle.Free();
        }

        public static void MutableToLower(this string s)
        {
            mutableToLowerOrUpper(s, false);
        }

        public static void MutableToUpper(this string s)
        {
            mutableToLowerOrUpper(s, true);
        }

        public static void MutableUpdate(this string s, int at, string updateString)
        {
            int length = s.Length;
            GCHandle handle = GCHandle.Alloc(s, GCHandleType.Pinned);
            IntPtr iPtr = handle.AddrOfPinnedObject();
            unsafe
            {
                var p = (char*) iPtr.ToPointer();
                p += at;
                foreach (char c in updateString)
                {
                    *p = c;
                    p++;
                }
            }

            handle.Free();
        }

        public static string PutSpaceAtUpperCasing(this string s)
        {
            var chars = new char[s.Length + (s.Length/2)];
            int cIndex = 0;
            int sIndex = 0;
            while (sIndex < s.Length)
            {
                if (char.IsUpper(s[sIndex]))
                {
                    chars[cIndex++] = ' ';
                    chars[cIndex++] = s[sIndex++];
                    while (sIndex < s.Length && char.IsUpper(s[sIndex]))
                    {
                        chars[cIndex++] = s[sIndex++];
                    }
                }

                if (sIndex < s.Length)
                {
                    if (s[sIndex] == '_')
                    {
                        chars[cIndex++] = ' ';
                        sIndex++;
                    }
                    else
                    {
                        chars[cIndex++] = s[sIndex++];
                    }
                }
            }

            int trim = char.IsUpper(s[0]) ? 1 : 0;
            var s1 = new string(chars, trim, cIndex - trim);
            return s1;
        }

        [CautionUsedByReflection]
        public static string Replace(string inputString, string oldValue, string newValue, bool ignoreCase = false)
        {
            string result = null;
            if (inputString != null)
            {
                if (!ignoreCase)
                {
                    result = inputString.Replace(oldValue, newValue);
                }
                else
                {
                    string s = newValue ?? string.Empty;
                    result = Regex.Replace(inputString, oldValue.Replace("$", "[$]"), s, RegexOptions.IgnoreCase);
                    if (result == string.Empty)
                    {
                        result = newValue;
                    }
                }
            }

            return result;
        }

        public static string Reverse(string s)
        {
            var ch = new char[s.Length];
            int len = s.Length/2;
            for (int i = 0; i <= len; i++)
            {
                ch[i] = s[s.Length - i - 1];
                ch[s.Length - i - 1] = s[i];
            }

            return new string(ch);
        }

        public static string SecondsToString(long seconds)
        {
            var ts = new TimeSpan(seconds*10000000);
            return TimeSpanToString(ts);
        }

        public static List<string> SplitString(this string s, int startPosition, char openingChar, char closingChar)
        {
            int openCount = 0;
            int openIndex = -1;
            var strings = new List<string>();
            for (int i = startPosition; i < s.Length; i++)
            {
                if (s[i] == openingChar)
                {
                    openCount++;
                    if (openCount == 1)
                    {
                        openIndex = i;
                    }
                }
                else
                {
                    if (s[i] == closingChar)
                    {
                        openCount--;
                        if (openCount == 0)
                        {
                            strings.Add(s.Substring(openIndex + 1, i - openIndex - 1));
                        }
                    }
                }
            }

            return strings;
        }

        public static List<string> SplitString(TextReader reader, char openingChar, char closingChar, int? count)
        {
            int openCount = 0;
            var strings = new List<string>();
            var buffer = new char[256];
            var b = new char[1];
            int i = -1;
            int counted = 0;
            while (true)
            {
                if (reader.Read(b, 0, 1) == 1)
                {
                    if (b[0] == openingChar)
                    {
                        openCount++;
                    }
                    else
                    {
                        if (b[0] == closingChar)
                        {
                            openCount--;
                            if (openCount == 0)
                            {
                                var s = new string(buffer, 0, i + 1);
                                strings.Add(s);
                                i = -1;
                                counted++;
                                if (count != null && count.Value == counted)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            i++;
                            if (buffer.Length <= i)
                            {
                                Array.Resize(ref buffer, buffer.Length*2);
                            }

                            buffer[i] = b[0];
                        }
                    }
                }
            }

            return strings;
        }

        public static string TimeSpanToString(TimeSpan ts)
        {
            var builder = new StringBuilder();
            if (ts.Seconds < 10)
            {
                builder.Insert(0, ":0" + ts.Seconds);
            }
            else
            {
                builder.Insert(0, ":" + ts.Seconds);
            }

            if (ts.Minutes < 10)
            {
                builder.Insert(0, ":0" + ts.Minutes);
            }
            else
            {
                builder.Insert(0, ":" + ts.Minutes);
            }

            int num = ts.Hours + (ts.Days*0x18);
            if (num < 10)
            {
                builder.Insert(0, "0" + num);
            }
            else
            {
                builder.Insert(0, num.ToString(CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        public static string ToCamelCase(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                string sCopy = string.Copy(s);
                MutableCamelCase(sCopy);
                return sCopy;
            }

            return s;
        }

        public static string ToPascalCase(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                string sCopy = string.Copy(s);
                MutablePascalCase(sCopy);
                return sCopy;
            }

            return s;
        }

        [CautionUsedByReflection]
        public static string Trim(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            string inputString = obj.ToString().Trim();
            return inputString == string.Empty ? null : inputString;
        }

        public static void WriteObjectsAsString(string name, IEnumerable enumerable, TextWriter writer)
        {
            writer.Write("\r\n");
            writer.Write(name);
            writer.Write("\r\n--------------------------------------\r\n");
            if (enumerable != null)
            {
                foreach (object v in enumerable)
                {
                    if (v != null)
                    {
                        writer.Write(v);
                        writer.Write("\r\n");
                    }
                }
            }

            writer.Write("--------------------------------------\r\n");
        }

        #endregion Public Methods and Operators

        #region Methods

        public static byte[] StringToByteArray(string input)
        {
            if (input == null)
            {
                return null;
            }

            return Convert.FromBase64String(input);
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            return Convert.ToBase64String(bytes);
        }

        private static void mutableAssign(this string oldValue, string newValue)
        {
            int length = newValue.Length;
            GCHandle handle = GCHandle.Alloc(oldValue, GCHandleType.Pinned);
            IntPtr iPtr = handle.AddrOfPinnedObject();
            unsafe
            {
                var p = (char*) iPtr.ToPointer();
                for (int i = 0; i < length; i++)
                {
                    *p = newValue[i];
                    p++;
                }
            }

            handle.Free();
        }

        private static void mutableToLowerOrUpper(string s, bool isToUpper)
        {
            int length = s.Length;
            GCHandle handle = GCHandle.Alloc(s, GCHandleType.Pinned);
            IntPtr iPtr = handle.AddrOfPinnedObject();
            unsafe
            {
                var p = (char*) iPtr.ToPointer();
                for (int i = 0; i < length; i++)
                {
                    *p = isToUpper ? char.ToUpper(*p) : char.ToLower(*p);
                    p++;
                }
            }

            handle.Free();
        }

        #endregion Methods

    }
}
