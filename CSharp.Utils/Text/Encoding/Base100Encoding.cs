using System;
using System.Globalization;
using System.Linq;

namespace CSharp.Utils.Text.Encoding
{
    public static class Base100Encoding
    {
        #region Constants

        public const string BASE_64_DIGITS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        public const string BASE_HUNDRED_DIGITS = "!;#$&+0123456789=?@ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz¡£¥§¿ÄÅÆÇÉÑÖØÜßàäåæèéìñòöøùü";

        #endregion Constants

        #region Public Methods and Operators

        public static int DecodeBase64ToInt32(string base64String)
        {
            long integer = getInt64(base64String, BASE_64_DIGITS);
            if (integer > int.MaxValue)
            {
                throw new OverflowException("Number too large for a 32 bit int.");
            }

            return (int)integer;
        }

        public static long DecodeBase64ToInt64(string base64String)
        {
            return getInt64(base64String, BASE_64_DIGITS);
        }

        public static int DecodeToInt32(string baseHundredNumberString)
        {
            return DecodeBase64ToInt32(baseHundredNumberString);

            /*
            Int32 toReturn;
            try
            {
                toReturn = Int32.Parse(getIntegerString(baseHundredNumberString));
            }
            catch (OverflowException oe)
            {
                throw new OverflowException("The supplied string was is too large for a 32-bit integer", oe);
            }
            return toReturn;
             */
        }

        public static long DecodeToInt64(string baseHundredNumberString)
        {
            return DecodeBase64ToInt64(baseHundredNumberString);

            /*
            Int64 toReturn;
            try
            {
                toReturn = Int64.Parse(getIntegerString(baseHundredNumberString));
            }
            catch (OverflowException oe)
            {
                throw new OverflowException("The supplied string was is too large for a 64-bit integer", oe);
            }
            return toReturn;
             */
        }

        public static string Encode(long integer, byte outputLength)
        {
            return EncodeBase64(integer, outputLength);

            /*
            if (integer > Math.Pow(Constants.BASE_HUNDRED_DIGITS.Length, outputLength)) // (95 ^ n) - 1  i.e 95 printable ASCII characters
                throw new OverflowException("Number specified too big for output length in Base 100.");

            string str = Encode(integer);

            if (str.Length < outputLength)
                return str.PadLeft(outputLength, Constants.BASE_HUNDRED_DIGITS[0]);

            return str;
             */
        }

        public static string Encode(long integer)
        {
            return EncodeBase64(integer);

            /*
            if (integer < 0)
                throw new Exception("Base 100 number must be positive");

            return getBaseHundredString(integer);
             */
        }

        public static string EncodeBase64(long integer, byte outputLength)
        {
            if (integer > Math.Pow(BASE_64_DIGITS.Length, outputLength))
            {
                throw new OverflowException("Number specified too big for output length in Base 64.");
            }

            string str = getString(integer, BASE_64_DIGITS);

            if (str.Length < outputLength)
            {
                return str.PadLeft(outputLength, BASE_64_DIGITS[0]);
            }

            return str;
        }

        public static string EncodeBase64(long integer)
        {
            if (integer < 0)
            {
                throw new Exception("Base 100 number must be positive");
            }

            return getString(integer, BASE_64_DIGITS);
        }

        #endregion Public Methods and Operators

        #region Methods

        private static string getBaseHundredString(long integer)
        {
            string number = integer.ToString(CultureInfo.InvariantCulture);
            if ((number.Length % 2) != 0)
            {
                number = number.PadLeft(number.Length + 1, '0');
            }

            var baseHundredNumber = new char[number.Length / 2];

            for (int i = 0; i < baseHundredNumber.Length; i++)
            {
                int digit = int.Parse(number.Substring(i * 2, 2));
                baseHundredNumber[i] = BASE_HUNDRED_DIGITS[digit];
            }

            return new string(baseHundredNumber);
        }

        private static long getInt64(string numberString, string digits)
        {
            int numberSystemLength = digits.Length;

            return numberString.Select((t, i) => digits.IndexOf(t) * (long)Math.Pow(numberSystemLength, numberString.Length - i - 1)).Sum();
        }

        private static string getIntegerString(string baseHundredNumberString)
        {
            return baseHundredNumberString.Aggregate(string.Empty, (current, c) => current + string.Format("{0:00}", BASE_HUNDRED_DIGITS.IndexOf(c)));
        }

        private static string getString(long integer, string digits)
        {
            int numberSystemLength = digits.Length;
            string str = string.Empty;

            while (integer >= numberSystemLength)
            {
                str = digits[(int)(integer % numberSystemLength)] + str;
                integer /= numberSystemLength;
            }

            str = digits[(int)(integer % numberSystemLength)] + str;

            return str;
        }

        #endregion Methods
    }
}
