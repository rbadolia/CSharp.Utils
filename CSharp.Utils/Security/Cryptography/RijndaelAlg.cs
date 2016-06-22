using System;
using System.Collections.Generic;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Security.Cryptography
{
    internal static class RijndaelAlg
    {
        #region Constants

        private const int MAXBC = 256 / 32;

        private const int MAXKC = 256 / 32;

        private const int MAXROUNDS = 14;

        #endregion Constants

        #region Public Methods and Operators

        public static int rijndaelDecrypt(byte[][] a, int keyBits, int blockBits, byte[][][] rk)
        {
            checkArray(a, 4, MAXBC);
            checkArray(rk, MAXROUNDS + 1, 4, MAXBC);
            int r, BC, ROUNDS;

            switch (blockBits)
            {
                case 128:
                    BC = 4;
                    break;

                case 192:
                    BC = 6;
                    break;

                case 256:
                    BC = 8;
                    break;

                default:
                    return -2;
            }

            switch (keyBits >= blockBits ? keyBits : blockBits)
            {
                case 128:
                    ROUNDS = 10;
                    break;

                case 192:
                    ROUNDS = 12;
                    break;

                case 256:
                    ROUNDS = 14;
                    break;

                default:
                    return -3; /* this cannot happen */
            }

            /* To decrypt: apply the inverse operations of the encrypt routine,
             *             in opposite order
             *
             * (AddRoundKey is an involution: it 's equal to its inverse)
             * (the inverse of Substitution with table S is Substitution with
                 *           the inverse table of S)
             * (the inverse of ShiftRows is ShiftRows over a suitable distance)
             */

            /* First the special round:
             *   without InvMixColumns
             *   with extra AddRoundKey
             */
            AddRoundKey(a, rk[ROUNDS], (byte)BC);
            Substitution(a, Constants.Si, (byte)BC);
            ShiftRows(a, 1, (byte)BC);

            /* ROUNDS-1 ordinary rounds
             */
            for (r = ROUNDS - 1; r > 0; r--)
            {
                AddRoundKey(a, rk[r], (byte)BC);
                InvMixColumns(a, (byte)BC);
                Substitution(a, Constants.Si, (byte)BC);
                ShiftRows(a, 1, (byte)BC);
            }

            /* End with the extra key addition
             */
            AddRoundKey(a, rk[0], (byte)BC);

            return 0;
        }

        public static int rijndaelDecryptRound(byte[][] a, int keyBits, int blockBits, byte[][][] rk, int rounds)
        {
            checkArray(a, 4, MAXBC);
            checkArray(rk, MAXROUNDS + 1, 4, MAXBC);
            int r, BC, ROUNDS;

            switch (blockBits)
            {
                case 128:
                    BC = 4;
                    break;

                case 192:
                    BC = 6;
                    break;

                case 256:
                    BC = 8;
                    break;

                default:
                    return -2;
            }

            switch (keyBits >= blockBits ? keyBits : blockBits)
            {
                case 128:
                    ROUNDS = 10;
                    break;

                case 192:
                    ROUNDS = 12;
                    break;

                case 256:
                    ROUNDS = 14;
                    break;

                default:
                    return -3; /* this cannot happen */
            }

            /* make number of rounds sane */
            if (rounds > ROUNDS)
            {
                rounds = ROUNDS;
            }

            /* First the special round:
             *   without InvMixColumns
             *   with extra AddRoundKey
             */
            AddRoundKey(a, rk[ROUNDS], (byte)BC);
            Substitution(a, Constants.Si, (byte)BC);
            ShiftRows(a, 1, (byte)BC);

            /* ROUNDS-1 ordinary rounds
             */
            for (r = ROUNDS - 1; r > rounds; r--)
            {
                AddRoundKey(a, rk[r], (byte)BC);
                InvMixColumns(a, (byte)BC);
                Substitution(a, Constants.Si, (byte)BC);
                ShiftRows(a, 1, (byte)BC);
            }

            if (rounds == 0)
            {
                /* End with the extra key addition
                 */
                AddRoundKey(a, rk[0], (byte)BC);
            }

            return 0;
        }

        public static int rijndaelEncrypt(byte[][] a, int keyBits, int blockBits, byte[][][] rk)
        {
            checkArray(a, 4, MAXBC);
            checkArray(rk, MAXROUNDS + 1, 4, MAXBC);

            /* Encryption of one block.
             */
            int r, BC, ROUNDS;

            switch (blockBits)
            {
                case 128:
                    BC = 4;
                    break;

                case 192:
                    BC = 6;
                    break;

                case 256:
                    BC = 8;
                    break;

                default:
                    return -2;
            }

            switch (keyBits >= blockBits ? keyBits : blockBits)
            {
                case 128:
                    ROUNDS = 10;
                    break;

                case 192:
                    ROUNDS = 12;
                    break;

                case 256:
                    ROUNDS = 14;
                    break;

                default:
                    return -3; /* this cannot happen */
            }

            /* begin with a key addition
             */
            AddRoundKey(a, rk[0], (byte)BC);

            /* ROUNDS-1 ordinary rounds
             */
            for (r = 1; r < ROUNDS; r++)
            {
                Substitution(a, Constants.S, (byte)BC); // *** p.henkel: added aes::
                ShiftRows(a, 0, (byte)BC);
                MixColumns(a, (byte)BC);
                AddRoundKey(a, rk[r], (byte)BC);
            }

            /* Last round is special: there is no MixColumns
             */
            Substitution(a, Constants.S, (byte)BC); // *** p.henkel: added aes::
            ShiftRows(a, 0, (byte)BC);
            AddRoundKey(a, rk[ROUNDS], (byte)BC);

            return 0;
        }

        public static int rijndaelEncryptRound(byte[][] a, int keyBits, int blockBits, byte[][][] rk, int rounds)
        {
            checkArray(a, 4, MAXBC);
            checkArray(rk, MAXROUNDS + 1, 4, MAXBC);
            int r, BC, ROUNDS;

            switch (blockBits)
            {
                case 128:
                    BC = 4;
                    break;

                case 192:
                    BC = 6;
                    break;

                case 256:
                    BC = 8;
                    break;

                default:
                    return -2;
            }

            switch (keyBits >= blockBits ? keyBits : blockBits)
            {
                case 128:
                    ROUNDS = 10;
                    break;

                case 192:
                    ROUNDS = 12;
                    break;

                case 256:
                    ROUNDS = 14;
                    break;

                default:
                    return -3; /* this cannot happen */
            }

            /* make number of rounds sane */
            if (rounds > ROUNDS)
            {
                rounds = ROUNDS;
            }

            /* begin with a key addition
             */
            AddRoundKey(a, rk[0], (byte)BC);

            /* at most ROUNDS-1 ordinary rounds
             */
            for (r = 1; (r <= rounds) && (r < ROUNDS); r++)
            {
                Substitution(a, Constants.S, (byte)BC); // *** p.henkel: added aes::
                ShiftRows(a, 0, (byte)BC);
                MixColumns(a, (byte)BC);
                AddRoundKey(a, rk[r], (byte)BC);
            }

            /* if necessary, do the last, special, round:
             */
            if (rounds == ROUNDS)
            {
                Substitution(a, Constants.S, (byte)BC); // *** p.henkel: added aes::
                ShiftRows(a, 0, (byte)BC);
                AddRoundKey(a, rk[ROUNDS], (byte)BC);
            }

            return 0;
        }

        #endregion Public Methods and Operators

        #region Methods

        internal static int rijndaelKeySched(byte[][] k, int keyBits, int blockBits, byte[][][] W)
        {
            checkArray(k, 4, MAXKC);
            checkArray(W, MAXROUNDS + 1, 4, MAXBC);

            /* Calculate the necessary round keys
             * The number of calculations depends on keyBits and blockBits
             */
            int KC, BC, ROUNDS;
            int i, j, rconpointer = 0;
            var tk = new byte[4][];
            tk[0] = new byte[MAXKC];
            tk[1] = new byte[MAXKC];
            tk[2] = new byte[MAXKC];
            tk[3] = new byte[MAXKC];

            switch (keyBits)
            {
                case 128:
                    KC = 4;
                    break;

                case 192:
                    KC = 6;
                    break;

                case 256:
                    KC = 8;
                    break;

                default:
                    return -1;
            }

            switch (blockBits)
            {
                case 128:
                    BC = 4;
                    break;

                case 192:
                    BC = 6;
                    break;

                case 256:
                    BC = 8;
                    break;

                default:
                    return -2;
            }

            switch (keyBits >= blockBits ? keyBits : blockBits)
            {
                case 128:
                    ROUNDS = 10;
                    break;

                case 192:
                    ROUNDS = 12;
                    break;

                case 256:
                    ROUNDS = 14;
                    break;

                default:
                    return -3; /* this cannot happen */
            }

            for (j = 0; j < KC; j++)
            {
                for (i = 0; i < 4; i++)
                {
                    tk[i][j] = k[i][j];
                }
            }

            int t = 0;

            /* copy values into round key array */
            for (j = 0; (j < KC) && (t < (ROUNDS + 1) * BC); j++, t++)
            {
                for (i = 0; i < 4; i++)
                {
                    W[t / BC][i][t % BC] = tk[i][j];
                }
            }

            while (t < (ROUNDS + 1) * BC)
            {
                /* while not enough round key material calculated */
                /* calculate new values */
                for (i = 0; i < 4; i++)
                {
                    tk[i][0] ^= Constants.S[tk[(i + 1) % 4][KC - 1]]; // *** p.henkel: added aes::
                }

                tk[0][0] ^= Constants.rcon[rconpointer++];

                if (KC != 8)
                {
                    for (j = 1; j < KC; j++)
                    {
                        for (i = 0; i < 4; i++)
                        {
                            tk[i][j] ^= tk[i][j - 1];
                        }
                    }
                }
                else
                {
                    for (j = 1; j < KC / 2; j++)
                    {
                        for (i = 0; i < 4; i++)
                        {
                            tk[i][j] ^= tk[i][j - 1];
                        }
                    }

                    for (i = 0; i < 4; i++)
                    {
                        tk[i][KC / 2] ^= Constants.S[tk[i][KC / 2 - 1]]; // *** p.henkel: added aes::
                    }

                    for (j = KC / 2 + 1; j < KC; j++)
                    {
                        for (i = 0; i < 4; i++)
                        {
                            tk[i][j] ^= tk[i][j - 1];
                        }
                    }
                }

                /* copy values into round key array */
                for (j = 0; (j < KC) && (t < (ROUNDS + 1) * BC); j++, t++)
                {
                    for (i = 0; i < 4; i++)
                    {
                        W[t / BC][i][t % BC] = tk[i][j];
                    }
                }
            }

            return 0;
        }

        private static void AddRoundKey(IList<byte[]> a, IList<byte[]> rk, byte BC)
        {
            /* Exor corresponding text input and round key input bytes
             */
            checkArray(a, 4, MAXBC);
            checkArray(rk, 4, MAXBC);
            int i;

            for (i = 0; i < 4; i++)
            {
                int j;
                for (j = 0; j < BC; j++)
                {
                    a[i][j] ^= rk[i][j];
                }
            }
        }

        private static void InvMixColumns(IList<byte[]> a, byte BC)
        {
            /* Mix the four bytes of every column in a linear way
             * This is the opposite operation of Mixcolumns
             */
            checkArray(a, 4, MAXBC);
            var b = new byte[4][];
            b[0] = new byte[MAXBC];
            b[1] = new byte[MAXBC];
            b[2] = new byte[MAXBC];
            b[3] = new byte[MAXBC];
            int i, j;

            for (j = 0; j < BC; j++)
            {
                for (i = 0; i < 4; i++)
                {
                    b[i][j] = (byte)(mul(0xe, a[i][j]) ^ mul(0xb, a[(i + 1) % 4][j]) ^ mul(0xd, a[(i + 2) % 4][j]) ^ mul(0x9, a[(i + 3) % 4][j]));
                }
            }

            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < BC; j++)
                {
                    a[i][j] = b[i][j];
                }
            }
        }

        private static void MixColumns(IList<byte[]> a, byte BC)
        {
            /* Mix the four bytes of every column in a linear way
             */
            checkArray(a, 4, MAXBC);
            var b = new byte[4][];
            b[0] = new byte[MAXBC];
            b[1] = new byte[MAXBC];
            b[2] = new byte[MAXBC];
            b[3] = new byte[MAXBC];
            int i, j;

            for (j = 0; j < BC; j++)
            {
                for (i = 0; i < 4; i++)
                {
                    b[i][j] = (byte)(mul(2, a[i][j]) ^ mul(3, a[(i + 1) % 4][j]) ^ a[(i + 2) % 4][j] ^ a[(i + 3) % 4][j]);
                }
            }

            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < BC; j++)
                {
                    a[i][j] = b[i][j];
                }
            }
        }

        private static void ShiftRows(IList<byte[]> a, byte d, byte BC)
        {
            /* Row 0 remains unchanged
             * The other three rows are shifted a variable amount
             */
            checkArray(a, 4, MAXBC);
            var tmp = new byte[MAXBC];
            int i;

            for (i = 1; i < 4; i++)
            {
                int j;
                for (j = 0; j < BC; j++)
                {
                    tmp[j] = a[i][(j + Constants.shifts[(BC - 4) >> 1, i, d]) % BC];
                }

                for (j = 0; j < BC; j++)
                {
                    a[i][j] = tmp[j];
                }
            }
        }

        private static void Substitution(IList<byte[]> a, IList<byte> box, byte BC)
        {
            /* Replace every byte of the input by the byte at that place
             * in the nonlinear S-box.
                 * This routine implements SubBytes and InvSubBytes
             */
            checkArray(a, 4, MAXBC);
            if (box.Count != 256)
            {
                throw new ArgumentException(@"Incorrect size", "box");
            }

            int i;

            for (i = 0; i < 4; i++)
            {
                int j;
                for (j = 0; j < BC; j++)
                {
                    a[i][j] = box[a[i][j]];
                }
            }
        }

        private static void checkArray(ICollection<byte[]> aArray, int dimension1, int dimension2)
        {
            Guard.ArgumentNotNull(aArray, "aArray");
            if (aArray.Count != dimension1)
            {
                throw new ArgumentException(@"Invalid size", "aArray");
            }

            foreach (var arr in aArray)
            {
                if (arr == null)
                {
                    throw new ArgumentException(@"Invalid size", "aArray");
                }

                if (arr.Length != dimension2)
                {
                    throw new ArgumentException(@"Invalid size", "aArray");
                }
            }
        }

        private static void checkArray(ICollection<byte[][]> aArray, int dimension1, int dimension2, int dimension3)
        {
            Guard.ArgumentNotNull(aArray, "aArray");
            if (aArray.Count != dimension1)
            {
                throw new ArgumentException(@"Invalid size", "aArray");
            }

            foreach (var arr in aArray)
            {
                if (arr == null)
                {
                    throw new ArgumentException(@"Invalid size", "aArray");
                }

                if (arr.Length != dimension2)
                {
                    throw new ArgumentException(@"Invalid size", "aArray");
                }

                foreach (var arr2 in arr)
                {
                    if (arr2 == null)
                    {
                        throw new ArgumentException(@"Invalid size", "aArray");
                    }

                    if (arr2.Length != dimension3)
                    {
                        throw new ArgumentException(@"Invalid size", "aArray");
                    }
                }
            }
        }

        private static byte mul(byte a, byte b)
        {
            /* multiply two elements of GF(2^m)
             * needed for MixColumn and InvMixColumn
             */
            if (a > 0 && b > 0)
            {
                return Constants.Alogtable[(Constants.Logtable[a] + Constants.Logtable[b]) % 255];
            }

            return 0;
        }

        #endregion Methods

        private static class Constants
        {
            #region Static Fields

            public static readonly byte[] Alogtable = { 1, 3, 5, 15, 17, 51, 85, 255, 26, 46, 114, 150, 161, 248, 19, 53, 95, 225, 56, 72, 216, 115, 149, 164, 247, 2, 6, 10, 30, 34, 102, 170, 229, 52, 92, 228, 55, 89, 235, 38, 106, 190, 217, 112, 144, 171, 230, 49, 83, 245, 4, 12, 20, 60, 68, 204, 79, 209, 104, 184, 211, 110, 178, 205, 76, 212, 103, 169, 224, 59, 77, 215, 98, 166, 241, 8, 24, 40, 120, 136, 131, 158, 185, 208, 107, 189, 220, 127, 129, 152, 179, 206, 73, 219, 118, 154, 181, 196, 87, 249, 16, 48, 80, 240, 11, 29, 39, 105, 187, 214, 97, 163, 254, 25, 43, 125, 135, 146, 173, 236, 47, 113, 147, 174, 233, 32, 96, 160, 251, 22, 58, 78, 210, 109, 183, 194, 93, 231, 50, 86, 250, 21, 63, 65, 195, 94, 226, 61, 71, 201, 64, 192, 91, 237, 44, 116, 156, 191, 218, 117, 159, 186, 213, 100, 172, 239, 42, 126, 130, 157, 188, 223, 122, 142, 137, 128, 155, 182, 193, 88, 232, 35, 101, 175, 234, 37, 111, 177, 200, 67, 197, 84, 252, 31, 33, 99, 165, 244, 7, 9, 27, 45, 119, 153, 176, 203, 70, 202, 69, 207, 74, 222, 121, 139, 134, 145, 168, 227, 62, 66, 198, 81, 243, 14, 18, 54, 90, 238, 41, 123, 141, 140, 143, 138, 133, 148, 167, 242, 13, 23, 57, 75, 221, 124, 132, 151, 162, 253, 28, 36, 108, 180, 199, 82, 246, 1 };

            public static readonly byte[] Logtable = { 0, 0, 25, 1, 50, 2, 26, 198, 75, 199, 27, 104, 51, 238, 223, 3, 100, 4, 224, 14, 52, 141, 129, 239, 76, 113, 8, 200, 248, 105, 28, 193, 125, 194, 29, 181, 249, 185, 39, 106, 77, 228, 166, 114, 154, 201, 9, 120, 101, 47, 138, 5, 33, 15, 225, 36, 18, 240, 130, 69, 53, 147, 218, 142, 150, 143, 219, 189, 54, 208, 206, 148, 19, 92, 210, 241, 64, 70, 131, 56, 102, 221, 253, 48, 191, 6, 139, 98, 179, 37, 226, 152, 34, 136, 145, 16, 126, 110, 72, 195, 163, 182, 30, 66, 58, 107, 40, 84, 250, 133, 61, 186, 43, 121, 10, 21, 155, 159, 94, 202, 78, 212, 172, 229, 243, 115, 167, 87, 175, 88, 168, 80, 244, 234, 214, 116, 79, 174, 233, 213, 231, 230, 173, 232, 44, 215, 117, 122, 235, 22, 11, 245, 89, 203, 95, 176, 156, 169, 81, 160, 127, 12, 246, 111, 23, 196, 73, 236, 216, 67, 31, 45, 164, 118, 123, 183, 204, 187, 62, 90, 251, 96, 177, 134, 59, 82, 161, 108, 170, 85, 41, 157, 151, 178, 135, 144, 97, 190, 220, 252, 188, 149, 207, 205, 55, 63, 91, 209, 83, 57, 132, 60, 65, 162, 109, 71, 20, 42, 158, 93, 86, 242, 211, 171, 68, 17, 146, 217, 35, 32, 46, 137, 180, 124, 184, 38, 119, 153, 227, 165, 103, 74, 237, 222, 197, 49, 254, 24, 13, 99, 140, 128, 192, 247, 112, 7 };

            public static readonly byte[] S = { 99, 124, 119, 123, 242, 107, 111, 197, 48, 1, 103, 43, 254, 215, 171, 118, 202, 130, 201, 125, 250, 89, 71, 240, 173, 212, 162, 175, 156, 164, 114, 192, 183, 253, 147, 38, 54, 63, 247, 204, 52, 165, 229, 241, 113, 216, 49, 21, 4, 199, 35, 195, 24, 150, 5, 154, 7, 18, 128, 226, 235, 39, 178, 117, 9, 131, 44, 26, 27, 110, 90, 160, 82, 59, 214, 179, 41, 227, 47, 132, 83, 209, 0, 237, 32, 252, 177, 91, 106, 203, 190, 57, 74, 76, 88, 207, 208, 239, 170, 251, 67, 77, 51, 133, 69, 249, 2, 127, 80, 60, 159, 168, 81, 163, 64, 143, 146, 157, 56, 245, 188, 182, 218, 33, 16, 255, 243, 210, 205, 12, 19, 236, 95, 151, 68, 23, 196, 167, 126, 61, 100, 93, 25, 115, 96, 129, 79, 220, 34, 42, 144, 136, 70, 238, 184, 20, 222, 94, 11, 219, 224, 50, 58, 10, 73, 6, 36, 92, 194, 211, 172, 98, 145, 149, 228, 121, 231, 200, 55, 109, 141, 213, 78, 169, 108, 86, 244, 234, 101, 122, 174, 8, 186, 120, 37, 46, 28, 166, 180, 198, 232, 221, 116, 31, 75, 189, 139, 138, 112, 62, 181, 102, 72, 3, 246, 14, 97, 53, 87, 185, 134, 193, 29, 158, 225, 248, 152, 17, 105, 217, 142, 148, 155, 30, 135, 233, 206, 85, 40, 223, 140, 161, 137, 13, 191, 230, 66, 104, 65, 153, 45, 15, 176, 84, 187, 22 };

            public static readonly byte[] Si = { 82, 9, 106, 213, 48, 54, 165, 56, 191, 64, 163, 158, 129, 243, 215, 251, 124, 227, 57, 130, 155, 47, 255, 135, 52, 142, 67, 68, 196, 222, 233, 203, 84, 123, 148, 50, 166, 194, 35, 61, 238, 76, 149, 11, 66, 250, 195, 78, 8, 46, 161, 102, 40, 217, 36, 178, 118, 91, 162, 73, 109, 139, 209, 37, 114, 248, 246, 100, 134, 104, 152, 22, 212, 164, 92, 204, 93, 101, 182, 146, 108, 112, 72, 80, 253, 237, 185, 218, 94, 21, 70, 87, 167, 141, 157, 132, 144, 216, 171, 0, 140, 188, 211, 10, 247, 228, 88, 5, 184, 179, 69, 6, 208, 44, 30, 143, 202, 63, 15, 2, 193, 175, 189, 3, 1, 19, 138, 107, 58, 145, 17, 65, 79, 103, 220, 234, 151, 242, 207, 206, 240, 180, 230, 115, 150, 172, 116, 34, 231, 173, 53, 133, 226, 249, 55, 232, 28, 117, 223, 110, 71, 241, 26, 113, 29, 41, 197, 137, 111, 183, 98, 14, 170, 24, 190, 27, 252, 86, 62, 75, 198, 210, 121, 32, 154, 219, 192, 254, 120, 205, 90, 244, 31, 221, 168, 51, 136, 7, 199, 49, 177, 18, 16, 89, 39, 128, 236, 95, 96, 81, 127, 169, 25, 181, 74, 13, 45, 229, 122, 159, 147, 201, 156, 239, 160, 224, 59, 77, 174, 42, 245, 176, 200, 235, 187, 60, 131, 83, 153, 97, 23, 43, 4, 126, 186, 119, 214, 38, 225, 105, 20, 99, 85, 33, 12, 125 };

            public static readonly byte[] rcon = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a, 0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91 };

            public static readonly byte[,,] shifts = { { { 0, 0 }, { 1, 3 }, { 2, 2 }, { 3, 1 } }, { { 0, 0 }, { 1, 5 }, { 2, 4 }, { 3, 3 } }, { { 0, 0 }, { 1, 7 }, { 3, 5 }, { 4, 4 } } };

            public static byte[,] iG = { { 0x0e, 0x09, 0x0d, 0x0b }, { 0x0b, 0x0e, 0x09, 0x0d }, { 0x0d, 0x0b, 0x0e, 0x09 }, { 0x09, 0x0d, 0x0b, 0x0e } };

            #endregion Static Fields
        }
    }
}
