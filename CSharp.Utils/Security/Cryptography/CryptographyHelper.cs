using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Security.Cryptography
{
    public static class CryptographyHelper
    {
        #region Constants

        private const int DefaultMaxpasswordlength = 10;

        private const int DefaultMinpasswordlength = 8;

        private const string PassowrdCHARSUCASE = "ABCDEFGHJKLMNPQRSTWXYZ";

        private const string PasswordCHARSLCASE = "abcdefgijkmnopqrstwxyz";

        private const string PasswordCHARSSPECIAL = "*$-+?_&=!%{}/";

        private const string PasswordCharsnumeric = "23456789";

        #endregion Constants

        #region Static Fields

        private static readonly byte[] salt = Encoding.ASCII.GetBytes("o6806642kbM7c5");

        #endregion Static Fields

        #region Public Methods and Operators

        public static string DecryptStringAES(string encPassword, string sharedKey)
        {
            Guard.ArgumentNotNullOrEmpty(encPassword, "encPassword");
            Guard.ArgumentNotNullOrEmpty(sharedKey, "sharedKey");
            RijndaelManaged aesAlg = null;
            string password;

            try
            {
                var key = new Rfc2898DeriveBytes(sharedKey, salt);
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                byte[] bytes = Convert.FromBase64String(encPassword);

                using (var memoryStreamDecrypt = new MemoryStream(bytes))
                {
                    using (var cryptoStreamDecrypt = new CryptoStream(memoryStreamDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var streamRederDecrypt = new StreamReader(cryptoStreamDecrypt))
                        {
                            password = streamRederDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            finally
            {
                if (aesAlg != null)
                {
                    aesAlg.Clear();
                }
            }

            return password;
        }

        public static string EncryptStringAES(string password, string sharedKey)
        {
            Guard.ArgumentNotNullOrEmpty(password, "password");
            Guard.ArgumentNotNullOrEmpty(sharedKey, "sharedKey");

            string outStr; // Encrypted string to return
            RijndaelManaged aesAlg = null; // RijndaelManaged object used to encrypt the data.

            try
            {
                var key = new Rfc2898DeriveBytes(sharedKey, salt);
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var memoryStreamEncrypt = new MemoryStream())
                {
                    using (var cryptoStreamEncrypt = new CryptoStream(memoryStreamEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var streamWriterEncrypt = new StreamWriter(cryptoStreamEncrypt))
                        {
                            streamWriterEncrypt.Write(password);
                        }
                    }

                    outStr = Convert.ToBase64String(memoryStreamEncrypt.ToArray());
                }
            }
            finally
            {
                if (aesAlg != null)
                {
                    aesAlg.Clear();
                }
            }

            return outStr;
        }

        public static string GenerateRandomPassword()
        {
            return GenerateRandomPassword(DefaultMinpasswordlength, DefaultMaxpasswordlength);
        }

        public static string GenerateRandomPassword(int length)
        {
            return GenerateRandomPassword(length, length);
        }

        public static string GenerateRandomPassword(int minLength, int maxLength)
        {
            if (minLength <= 0 || maxLength <= 0 || minLength > maxLength)
            {
                return null;
            }

            var charGroups = new[] { PasswordCHARSLCASE.ToCharArray(), PassowrdCHARSUCASE.ToCharArray(), PasswordCharsnumeric.ToCharArray(), PasswordCHARSSPECIAL.ToCharArray() };
            var charsLeftInGroup = new int[charGroups.Length];
            for (int i = 0; i < charsLeftInGroup.Length; i++)
            {
                charsLeftInGroup[i] = charGroups[i].Length;
            }

            var leftGroupsOrder = new int[charGroups.Length];
            for (int i = 0; i < leftGroupsOrder.Length; i++)
            {
                leftGroupsOrder[i] = i;
            }

            var randomBytes = new byte[4];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
                int seed = (randomBytes[0] & 0x7f) << 24 | randomBytes[1] << 16 | randomBytes[2] << 8 | randomBytes[3];
                var random = new Random(seed);
                char[] password = minLength < maxLength ? new char[random.Next(minLength, maxLength + 1)] : new char[minLength];
                int lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                for (int i = 0; i < password.Length; i++)
                {
                    int nextLeftGroupsOrderIdx = lastLeftGroupsOrderIdx == 0 ? 0 : random.Next(0, lastLeftGroupsOrderIdx);
                    int nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];
                    int lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;
                    int nextCharIdx = lastCharIdx == 0 ? 0 : random.Next(0, lastCharIdx + 1);
                    password[i] = charGroups[nextGroupIdx][nextCharIdx];
                    if (lastCharIdx == 0)
                    {
                        charsLeftInGroup[nextGroupIdx] = charGroups[nextGroupIdx].Length;
                    }
                    else
                    {
                        if (lastCharIdx != nextCharIdx)
                        {
                            char temp = charGroups[nextGroupIdx][lastCharIdx];
                            charGroups[nextGroupIdx][lastCharIdx] = charGroups[nextGroupIdx][nextCharIdx];
                            charGroups[nextGroupIdx][nextCharIdx] = temp;
                        }

                        charsLeftInGroup[nextGroupIdx]--;
                    }

                    if (lastLeftGroupsOrderIdx == 0)
                    {
                        lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                    }
                    else
                    {
                        if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
                        {
                            int temp = leftGroupsOrder[lastLeftGroupsOrderIdx];
                            leftGroupsOrder[lastLeftGroupsOrderIdx] = leftGroupsOrder[nextLeftGroupsOrderIdx];
                            leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
                        }

                        lastLeftGroupsOrderIdx--;
                    }
                }

                return new string(password);
            }
        }

        #endregion Public Methods and Operators
    }
}
