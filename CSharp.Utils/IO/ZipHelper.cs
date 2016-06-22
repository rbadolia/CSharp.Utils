using System;
using System.IO;
using System.IO.Compression;

namespace CSharp.Utils.IO
{
    public static class ZipHelper
    {
        #region Public Methods and Operators

        public static void ZipFile(string sourceFile, string targetFile)
        {
            sourceFile = IOHelper.ResolvePath(sourceFile);
            targetFile = IOHelper.ResolvePath(targetFile);
            DoZipFile(sourceFile, targetFile);
        }

        public static void ZipFile(string sourceFile)
        {
            sourceFile = IOHelper.ResolvePath(sourceFile);
            var fi = new FileInfo(sourceFile);
            string targetFileName = fi.Directory.FullName + "\\" + fi.Name + ".zip";
            DoZipFile(sourceFile, targetFileName);
        }

        #endregion Public Methods and Operators

        #region Methods

        private static void DoZipFile(string sourceFile, string targetFile)
        {
            try
            {
                using (FileStream outputStream = File.Create(targetFile))
                {
                    using (var zipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        var buffer = new byte[2048];
                        using (FileStream inputStream = File.OpenRead(sourceFile))
                        {
                            while (true)
                            {
                                int readLength = inputStream.Read(buffer, 0, buffer.Length);
                                if (readLength > 0)
                                {
                                    zipStream.Write(buffer, 0, readLength);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        #endregion Methods
    }
}
