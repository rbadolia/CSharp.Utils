using System;
using System.Text;

namespace CSharp.Utils.Security.Cryptography
{
    public static class AESEncryption
    {
        #region Constants

        private const int BITSPERBLOCK = 128; /* Default number of bits in a cipher block */

        #endregion Constants

        #region Static Fields

        private static readonly Random _Random = new Random();

        private static readonly byte[] salt = Encoding.ASCII.GetBytes("o6806642kbM7c5");

        #endregion Static Fields

        #region Enums

        private enum Direction
        {
            EEncrypt = 0, /*  Are we ciphering in ECB mode?   */

            EDecrypt
        }

        private enum EncryptionMode
        {
            EModeECB = 1, /*  Are we ciphering in ECB mode?   */

            EModeCBC, /*  Are we ciphering in CBC mode?   */
        }

        #endregion Enums

        #region Public Methods and Operators

        public static string CBCBase64DecodeAndDecrypt(string aEncodedEncryptedText, string aKeyMaterial)
        {
            byte[] decoded = Convert.FromBase64String(aEncodedEncryptedText);
            return Decrypt(decoded, aKeyMaterial, null, EncryptionMode.EModeCBC);
        }

        public static string CBCBase64DecodeAndDecrypt(string aEncodedEncryptedText, string aKeyMaterial, string aInitVector)
        {
            byte[] decoded = Convert.FromBase64String(aEncodedEncryptedText);
            return Decrypt(decoded, aKeyMaterial, aInitVector, EncryptionMode.EModeCBC);
        }

        public static string CBCDecrypt(byte[] aEncryptedText, string aKeyMaterial)
        {
            return Decrypt(aEncryptedText, aKeyMaterial, null, EncryptionMode.EModeCBC);
        }

        public static string CBCDecrypt(byte[] aEncryptedText, string aKeyMaterial, string aInitVector)
        {
            return Decrypt(aEncryptedText, aKeyMaterial, aInitVector, EncryptionMode.EModeCBC);
        }

        /************************************************************************************************************************************/
        /************************************************************************************************************************************/

        public static byte[] CBCEncrypt(string aPlainText, string aKeyMaterial)
        {
            return Encrypt(aPlainText, aKeyMaterial, null, EncryptionMode.EModeCBC);
        }

        public static byte[] CBCEncrypt(string aPlainText, string aKeyMaterial, string aInitVector)
        {
            return Encrypt(aPlainText, aKeyMaterial, aInitVector, EncryptionMode.EModeCBC);
        }

        public static string CBCEncryptAndBase64Encode(string aPlainText, string aKeyMaterial)
        {
            byte[] encrypted = Encrypt(aPlainText, aKeyMaterial, null, EncryptionMode.EModeCBC);
            return Convert.ToBase64String(encrypted, 0, encrypted.Length);
        }

        public static string CBCEncryptAndBase64Encode(string aPlainText, string aKeyMaterial, string aInitVector)
        {
            byte[] encrypted = Encrypt(aPlainText, aKeyMaterial, aInitVector, EncryptionMode.EModeCBC);
            return Convert.ToBase64String(encrypted, 0, encrypted.Length);
        }

        public static string GenerateRandomAESkey()
        {
            string toRet = string.Empty;
            for (int i = 0; i < 32; i++)
            {
                int r = GetRandom(0, 15);
                if (r > 9)
                {
                    r = 65 + (r - 10);
                }
                else
                {
                    r += 48;
                }

                var c = (char)r;
                if (r % 2 == 0)
                {
                    toRet += c;
                }
                else
                {
                    toRet = c + toRet;
                }
            }

            return toRet;
        }

        #endregion Public Methods and Operators

        #region Methods

        private static string Decrypt(byte[] aEncryptedText, string aKeyMaterial, string aInitVector, EncryptionMode mode)
        {
            RijndaelApi.keyInstance keyInst = MakeKey(aKeyMaterial, Direction.EDecrypt);
            var cipherInst = new RijndaelApi.cipherInstance { blockLen = BITSPERBLOCK };
            cipherInst.blockLen = BITSPERBLOCK;
            byte[] IV;
            if (aInitVector != null && aInitVector.Trim() != string.Empty)
            {
                IV = Encoding.Default.GetBytes(aInitVector);
            }
            else
            {
                IV = null;
            }

            int initStatus = RijndaelApi.cipherInit(cipherInst, (byte)mode, IV);
            if (initStatus != 1)
            {
                throw new Exception("Error initializing cipher");
            }

            var output = new byte[aEncryptedText.Length];
            int numBlocks = aEncryptedText.Length / (BITSPERBLOCK / 8);
            int decStatus = RijndaelApi.blockDecrypt(cipherInst, keyInst, aEncryptedText, numBlocks * BITSPERBLOCK, ref output);

            string tmpOutput = Encoding.UTF8.GetString(output, 0, output.Length);
            int nullCharLoc = tmpOutput.IndexOf('\0');
            if (nullCharLoc > 0)
            {
                return tmpOutput.Substring(0, nullCharLoc);
            }

            return tmpOutput;
        }

        private static byte[] Encrypt(string aPlainText, string aKeyMaterial, string aInitVector, EncryptionMode mode)
        {
            RijndaelApi.keyInstance keyInst = MakeKey(aKeyMaterial, Direction.EEncrypt);
            var cipherInst = new RijndaelApi.cipherInstance { blockLen = BITSPERBLOCK };
            byte[] IV;
            if (aInitVector != null && aInitVector.Trim() != string.Empty)
            {
                IV = Encoding.Default.GetBytes(aInitVector);
            }
            else
            {
                IV = null;
            }

            int initStatus = RijndaelApi.cipherInit(cipherInst, (byte)mode, IV);
            if (initStatus != 1)
            {
                throw new Exception("Error initializing cipher");
            }

            byte[] test = Encoding.UTF8.GetBytes(aPlainText);
            int textLength = test.Length;
            int numBlocks = textLength / (BITSPERBLOCK / 8);
            if (textLength % (BITSPERBLOCK / 8) != 0)
            {
                numBlocks++;
            }

            string toRet = aPlainText;
            var input = new byte[numBlocks * (BITSPERBLOCK / 8)];
            Encoding.UTF8.GetBytes(aPlainText, 0, aPlainText.Length, input, 0);
            var output = new byte[numBlocks * (BITSPERBLOCK / 8)];

            RijndaelApi.blockEncrypt(cipherInst, keyInst, input, numBlocks * BITSPERBLOCK, ref output);

            return output;
        }

        private static int GetRandom(int min, int max)
        {
            return _Random.Next(min, max);
        }

        private static RijndaelApi.keyInstance MakeKey(string aKeyMaterial, Direction aDirection)
        {
            var toRet = new RijndaelApi.keyInstance();
            if (aKeyMaterial.Length % 2 != 0)
            {
                throw new Exception("Bad key material length");
            }

            int i = 0;
            foreach (char c in aKeyMaterial)
            {
                string pos = aKeyMaterial.Substring(i++, 1);
                bool check = (pos[0] >= 'A' && pos[0] <= 'F') || (pos[0] >= '0' && pos[0] <= '9') || (pos[0] >= 'a' && pos[0] <= 'f');
                if (!check)
                {
                    throw new Exception("Bad key material data");
                }
            }

            int keyLengthInBits = aKeyMaterial.Length * 4;
            if ((keyLengthInBits != 128) && (keyLengthInBits != 192) && (keyLengthInBits != 256))
            {
                throw new Exception("Bad key material length");
            }

            toRet.blockLen = BITSPERBLOCK;
            RijndaelApi.makeKey(toRet, (byte)aDirection, keyLengthInBits, aKeyMaterial);

            return toRet;
        }

        #endregion Methods
    }
}
