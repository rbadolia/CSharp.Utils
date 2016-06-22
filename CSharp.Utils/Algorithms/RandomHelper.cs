using System;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Algorithms
{
    public static class RandomHelper
    {
        #region Constants

        public const string LowerChars = "abcdefghijklmnopqrstuvwxyz";

        public const string LowerNumericChars = "abcdefghijklmnopqrstuvwxyz0123456789";

        public const string NumericChars = "0123456789";

        public const string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public const string UpperLowerChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public const string UpperLowerNumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public const string UpperNumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        #endregion Constants

        #region Static Fields

        private static readonly Random RandomObject = new Random();

        #endregion Static Fields

        #region Public Methods and Operators

        [CautionUsedByReflection]
        [StateIntact]
        public static string BuildRandomString(int length, string inputChars = UpperLowerNumericChars)
        {
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = inputChars[RandomObject.Next(inputChars.Length)];
            }

            return new string(chars);
        }

        #endregion Public Methods and Operators
    }
}
