using System.Text;

namespace CSharp.Utils.Security.Cryptography
{
    internal static class RijndaelApi
    {
        #region Constants

        private const int BAD_CIPHER_INSTANCE = -7;

        private const int BAD_CIPHER_MODE = -4; /*  Params struct passed to

                                                 cipherInit invalid */

        private const int BAD_CIPHER_STATE = -5; /*  Cipher in wrong state (e.g., not

                                                 initialized) */

        /*  Error Codes - CHANGE POSSIBLE: inclusion of additional error codes  */

        private const int BAD_KEY_DIR = -1; /*  Key direction is invalid, e.g.,

                                                 unknown value */

        private const int BAD_KEY_INSTANCE = -3; /*  Key passed is not valid  */

        private const int BAD_KEY_MAT = -2; /*  Key material not of correct

                                                 length */

        private const int BITSPERBLOCK = 128; /* Default number of bits in a cipher block */

        private const int DIR_DECRYPT = 1; /*  Are we decrpyting?  */

        private const int DIR_ENCRYPT = 0; /*  Are we encrpyting?  */

        private const int MAXBC = 256 / 32;

        private const int MAXKC = 256 / 32;

        private const int MAXROUNDS = 14;

        private const int MAX_IV_SIZE = BITSPERBLOCK / 8; /* # bytes needed to

                                                        represent an IV  */

        /*  CHANGE POSSIBLE:  inclusion of algorithm specific defines  */

        private const int MAX_KEY_SIZE = 64; /* # of ASCII char's needed to

                                                  represent a key */

        private const int MODE_CBC = 2; /*  Are we ciphering in CBC mode?   */

        private const int MODE_CFB1 = 3; /*  Are we ciphering in 1-bit CFB mode? */

        private const int MODE_ECB = 1; /*  Are we ciphering in ECB mode?   */

        #endregion Constants

        #region Methods

        internal static int blockDecrypt(cipherInstance cipher, keyInstance key, byte[] input, int inputLen, ref byte[] outBuffer)
        {
            int i, j, t;
            var block = new byte[4][];
            block[0] = new byte[MAXBC];
            block[1] = new byte[MAXBC];
            block[2] = new byte[MAXBC];
            block[3] = new byte[MAXBC];

            if (cipher == null || key == null || key.direction == DIR_ENCRYPT || cipher.blockLen != key.blockLen)
            {
                return BAD_CIPHER_STATE;
            }

            /* check parameter consistency: */
            if (key.direction != DIR_DECRYPT || (key.keyLen != 128 && key.keyLen != 192 && key.keyLen != 256))
            {
                return BAD_KEY_MAT;
            }

            if ((cipher.mode != MODE_ECB && cipher.mode != MODE_CBC && cipher.mode != MODE_CFB1) || (cipher.blockLen != 128 && cipher.blockLen != 192 && cipher.blockLen != 256))
            {
                return BAD_CIPHER_STATE;
            }

            int numBlocks = inputLen / cipher.blockLen;

            switch (cipher.mode)
            {
                case MODE_ECB:
                    for (i = 0; i < numBlocks; i++)
                    {
                        for (j = 0; j < cipher.blockLen / 32; j++)
                        {
                            for (t = 0; t < 4; t++)
                            {
                                /* parse input stream into rectangular array */
                                block[t][j] = (byte)(input[cipher.blockLen / 8 * i + 4 * j + t] & 0xFF);
                            }
                        }

                        RijndaelAlg.rijndaelDecrypt(block, key.keyLen, cipher.blockLen, key.keySched);
                        for (j = 0; j < cipher.blockLen / 32; j++)
                        {
                            /* parse rectangular array into output ciphertext bytes */
                            for (t = 0; t < 4; t++)
                            {
                                outBuffer[cipher.blockLen / 8 * i + 4 * j + t] = block[t][j];
                            }
                        }
                    }

                    break;

                case MODE_CBC:

                    /* first block */
                    for (j = 0; j < cipher.blockLen / 32; j++)
                    {
                        for (t = 0; t < 4; t++)
                        {
                            /* parse input stream into rectangular array */
                            block[t][j] = (byte)(input[4 * j + t] & 0xFF);
                        }
                    }

                    RijndaelAlg.rijndaelDecrypt(block, key.keyLen, cipher.blockLen, key.keySched);

                    for (j = 0; j < cipher.blockLen / 32; j++)
                    {
                        /* exor the IV and parse rectangular array into output ciphertext bytes */
                        for (t = 0; t < 4; t++)
                        {
                            outBuffer[4 * j + t] = (byte)(block[t][j] ^ cipher.IV[t + 4 * j]);
                        }
                    }

                    /* next blocks */
                    for (i = 1; i < numBlocks; i++)
                    {
                        for (j = 0; j < cipher.blockLen / 32; j++)
                        {
                            for (t = 0; t < 4; t++)
                            {
                                /* parse input stream into rectangular array */
                                block[t][j] = (byte)(input[cipher.blockLen / 8 * i + 4 * j + t] & 0xFF);
                            }
                        }

                        RijndaelAlg.rijndaelDecrypt(block, key.keyLen, cipher.blockLen, key.keySched);

                        for (j = 0; j < cipher.blockLen / 32; j++)
                        {
                            /* exor previous ciphertext block and parse rectangular array
                                   into output ciphertext bytes */
                            for (t = 0; t < 4; t++)
                            {
                                outBuffer[cipher.blockLen / 8 * i + 4 * j + t] = (byte)(block[t][j] ^ input[cipher.blockLen / 8 * i + 4 * j + t - 4 * cipher.blockLen / 32]);
                            }
                        }
                    }

                    break;

                default:
                    return BAD_CIPHER_STATE;
            }

            return numBlocks * cipher.blockLen;
        }

        internal static int blockEncrypt(cipherInstance cipher, keyInstance key, byte[] input, int inputLen, ref byte[] outBuffer)
        {
            int i, j, t;
            var block = new byte[4][];
            block[0] = new byte[MAXBC];
            block[1] = new byte[MAXBC];
            block[2] = new byte[MAXBC];
            block[3] = new byte[MAXBC];

            /* check parameter consistency: */
            if (key == null || key.direction != DIR_ENCRYPT || (key.keyLen != 128 && key.keyLen != 192 && key.keyLen != 256))
            {
                return BAD_KEY_MAT;
            }

            if (cipher == null || (cipher.mode != MODE_ECB && cipher.mode != MODE_CBC && cipher.mode != MODE_CFB1) || (cipher.blockLen != 128 && cipher.blockLen != 192 && cipher.blockLen != 256))
            {
                return BAD_CIPHER_STATE;
            }

            int numBlocks = inputLen / cipher.blockLen;

            switch (cipher.mode)
            {
                case MODE_ECB:
                    for (i = 0; i < numBlocks; i++)
                    {
                        for (j = 0; j < cipher.blockLen / 32; j++)
                        {
                            for (t = 0; t < 4; t++)
                            {
                                /* parse input stream into rectangular array */
                                block[t][j] = (byte)(input[cipher.blockLen / 8 * i + 4 * j + t] & 0xFF);
                            }
                        }

                        RijndaelAlg.rijndaelEncrypt(block, key.keyLen, cipher.blockLen, key.keySched);
                        for (j = 0; j < cipher.blockLen / 32; j++)
                        {
                            /* parse rectangular array into output ciphertext bytes */
                            for (t = 0; t < 4; t++)
                            {
                                outBuffer[cipher.blockLen / 8 * i + 4 * j + t] = block[t][j];
                            }
                        }
                    }

                    break;

                case MODE_CBC:
                    for (j = 0; j < cipher.blockLen / 32; j++)
                    {
                        for (t = 0; t < 4; t++)
                        {
                            /* parse initial value into rectangular array */
                            block[t][j] = (byte)(cipher.IV[t + 4 * j] & 0xFF);
                        }
                    }

                    for (i = 0; i < numBlocks; i++)
                    {
                        for (j = 0; j < cipher.blockLen / 32; j++)
                        {
                            for (t = 0; t < 4; t++)
                            {
                                /* parse input stream into rectangular array and exor with
                                   IV or the previous ciphertext */
                                block[t][j] ^= (byte)(input[cipher.blockLen / 8 * i + 4 * j + t] & 0xFF);
                            }
                        }

                        RijndaelAlg.rijndaelEncrypt(block, key.keyLen, cipher.blockLen, key.keySched);
                        for (j = 0; j < cipher.blockLen / 32; j++)
                        {
                            /* parse rectangular array into output ciphertext bytes */
                            for (t = 0; t < 4; t++)
                            {
                                outBuffer[cipher.blockLen / 8 * i + 4 * j + t] = block[t][j];
                            }
                        }
                    }

                    break;

                default:
                    return BAD_CIPHER_STATE;
            }

            return numBlocks * cipher.blockLen;
        }

        internal static int cipherInit(cipherInstance cipher, byte mode, byte[] IV)
        {
            int i;

            if ((mode == MODE_ECB) || (mode == MODE_CBC) || (mode == MODE_CFB1))
            {
                cipher.mode = mode;
            }
            else
            {
                return BAD_CIPHER_MODE;
            }

            if (IV != null)
            {
                for (i = 0; i < cipher.blockLen / 8; i++)
                {
                    int t = IV[2 * i];
                    int j;
                    if ((t >= '0') && (t <= '9'))
                    {
                        j = (t - '0') << 4;
                    }
                    else if ((t >= 'a') && (t <= 'f'))
                    {
                        j = (t - 'a' + 10) << 4;
                    }
                    else if ((t >= 'A') && (t <= 'F'))
                    {
                        j = (t - 'A' + 10) << 4;
                    }
                    else
                    {
                        return BAD_CIPHER_INSTANCE;
                    }

                    t = IV[2 * i + 1];
                    if ((t >= '0') && (t <= '9'))
                    {
                        j ^= t - '0';
                    }
                    else if ((t >= 'a') && (t <= 'f'))
                    {
                        j ^= t - 'a' + 10;
                    }
                    else if ((t >= 'A') && (t <= 'F'))
                    {
                        j ^= t - 'A' + 10;
                    }
                    else
                    {
                        return BAD_CIPHER_INSTANCE;
                    }

                    cipher.IV[i] = (byte)j;
                }
            }
            else
            {
                for (i = 0; i < cipher.blockLen / 8; i++)
                {
                    cipher.IV[i] = 0;
                }
            }

            return 1;
        }

        internal static int cipherUpdateRounds(ref cipherInstance cipher, ref keyInstance key, byte[] input, int inputLen, ref byte[] outBuffer, int rounds)
        {
            int j, t;
            var block = new byte[4][];
            block[0] = new byte[MAXBC];
            block[1] = new byte[MAXBC];
            block[2] = new byte[MAXBC];
            block[3] = new byte[MAXBC];

            if (cipher == null || key == null || cipher.blockLen != key.blockLen)
            {
                return BAD_CIPHER_STATE;
            }

            for (j = 0; j < cipher.blockLen / 32; j++)
            {
                for (t = 0; t < 4; t++)
                {
                    /* parse input stream into rectangular array */
                    block[t][j] = (byte)(input[4 * j + t] & 0xFF);
                }
            }

            switch (key.direction)
            {
                case DIR_ENCRYPT:
                    RijndaelAlg.rijndaelEncryptRound(block, key.keyLen, cipher.blockLen, key.keySched, rounds);
                    break;

                case DIR_DECRYPT:
                    RijndaelAlg.rijndaelDecryptRound(block, key.keyLen, cipher.blockLen, key.keySched, rounds);
                    break;

                default:
                    return BAD_KEY_DIR;
            }

            for (j = 0; j < cipher.blockLen / 32; j++)
            {
                /* parse rectangular array into output ciphertext bytes */
                for (t = 0; t < 4; t++)
                {
                    outBuffer[4 * j + t] = block[t][j];
                }
            }

            return 1;
        }

        /*  Function protoypes  */
        /*  CHANGED: makeKey(): parameter blockLen added
                                this parameter is absolutely necessary if you want to
                    setup the round keys in a variable block length setting
                 cipherInit(): parameter blockLen added (for obvious reasons)
         */

        internal static int makeKey(keyInstance key, byte direction, int keyLen, string keyMaterial)
        {
            var k = new byte[4][];
            k[0] = new byte[MAXKC];
            k[1] = new byte[MAXKC];
            k[2] = new byte[MAXKC];
            k[3] = new byte[MAXKC];
            int i;

            if (key == null)
            {
                return BAD_KEY_INSTANCE;
            }

            if ((direction == DIR_ENCRYPT) || (direction == DIR_DECRYPT))
            {
                key.direction = direction;
            }
            else
            {
                return BAD_KEY_DIR;
            }

            if ((keyLen == 128) || (keyLen == 192) || (keyLen == 256))
            {
                key.keyLen = keyLen;
            }
            else
            {
                return BAD_KEY_MAT;
            }

            if (!string.IsNullOrEmpty(keyMaterial))
            {
                key.keyMaterial = Encoding.Default.GetBytes(keyMaterial);
            }

            /* initialize key schedule: */
            for (i = 0; i < key.keyLen / 8; i++)
            {
                int t = key.keyMaterial[2 * i];
                int j;
                if ((t >= '0') && (t <= '9'))
                {
                    j = (t - '0') << 4;
                }
                else if ((t >= 'a') && (t <= 'f'))
                {
                    j = (t - 'a' + 10) << 4;
                }
                else if ((t >= 'A') && (t <= 'F'))
                {
                    j = (t - 'A' + 10) << 4;
                }
                else
                {
                    return BAD_KEY_MAT;
                }

                t = key.keyMaterial[2 * i + 1];
                if ((t >= '0') && (t <= '9'))
                {
                    j ^= t - '0';
                }
                else if ((t >= 'a') && (t <= 'f'))
                {
                    j ^= t - 'a' + 10;
                }
                else if ((t >= 'A') && (t <= 'F'))
                {
                    j ^= t - 'A' + 10;
                }
                else
                {
                    return BAD_KEY_MAT;
                }

                k[i % 4][i / 4] = (byte)j;
            }

            RijndaelAlg.rijndaelKeySched(k, key.keyLen, key.blockLen, key.keySched);

            return 1;
        }

        #endregion Methods

        /*  The structure for cipher information */

        internal class cipherInstance
        {
            /*  Add any algorithm specific parameters needed here  */
            #region Fields

            public byte[] IV; /* A possible Initialization Vector for

      					ciphering */

            public int blockLen; /* Sample: Handles non-128 bit block sizes

      					(if available) */

            public byte mode; /* MODE_ECB, MODE_CBC, or MODE_CFB1 */

            #endregion Fields

            #region Constructors and Finalizers

            public cipherInstance()
            {
                this.IV = new byte[MAX_IV_SIZE];
                this.mode = 0;
                this.blockLen = 0;
            }

            #endregion Constructors and Finalizers
        }

        internal class keyInstance
        {
            /*  The following parameters are algorithm dependent, replace or
                  add as necessary  */
            #region Fields

            public int blockLen; /* block length */

            public byte direction; /*  Key used for encrypting or decrypting? */

            public int keyLen; /*  Length of the key  */

            public byte[] keyMaterial; /*  Raw key data in ASCII,

                                            e.g., user input or KAT values */

            public byte[][][] keySched; /* key schedule		*/

            #endregion Fields

            #region Constructors and Finalizers

            public keyInstance()
            {
                this.direction = 0;
                this.keyLen = 0;
                this.keyMaterial = new byte[MAX_KEY_SIZE + 1];
                this.blockLen = -1;
                this.keySched = new byte[MAXROUNDS + 1][][];
                for (int i = 0; i < MAXROUNDS + 1; i++)
                {
                    this.keySched[i] = new byte[4][];
                    for (int j = 0; j < 4; j++)
                    {
                        this.keySched[i][j] = new byte[MAXBC];
                    }
                }
            }

            #endregion Constructors and Finalizers
        }
    }
}
