using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using CSharp.Utils.Diagnostics;
using CSharp.Utils.Validation;

namespace CSharp.Utils.IO
{
    public static class IOHelper
    {
        #region Public Methods and Operators

        public static string CreateDirectory(string parentDirectoryPath, string directoryName)
        {
            var path = Path.Combine(parentDirectoryPath, directoryName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static void CreateFileWithContent(string directoryName, string fileName, string fileContent, Encoding encoding = null)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(directoryName, "directoryName");
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(fileName, "fileName");
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            var filePath = Path.Combine(directoryName, fileName);
            if (encoding == null)
            {
                File.WriteAllText(filePath, fileContent);
            }
            else
            {
                File.WriteAllText(filePath, fileContent, encoding);
            }
        }

        public static bool DeleteFile(string fileName, bool checkExists)
        {
            if (checkExists)
            {
                if (File.Exists(fileName))
                {
                    return deleteFileUnsafe(fileName);
                }

                return true;
            }

            return deleteFileUnsafe(fileName);
        }

        public static bool IsFileLocked(string fileName)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return false;
        }

        public static bool MoveFile(string source, string dest, bool checkExists)
        {
            if (checkExists)
            {
                if (File.Exists(source))
                {
                    return moveFileUnsafe(source, dest);
                }

                return true;
            }

            return moveFileUnsafe(source, dest);
        }

        public static string ResolvePath(string path)
        {
            return ResolvePath(path, ProcessHelper.RootPathOfApplication);
        }

        public static string ResolvePath(string path, string basePath)
        {
            return Path.IsPathRooted(path) ? path : Path.Combine(basePath, path);
        }

        #endregion Public Methods and Operators

        #region Methods

        private static bool deleteFileUnsafe(string fileName)
        {
            try
            {
                File.Delete(fileName);
                return true;
            }
            catch (FileNotFoundException)
            {
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        private static bool moveFileUnsafe(string source, string dest)
        {
            int attemptCount = 0;
            while (true)
            {
                try
                {
                    File.Move(source, dest);
                    return true;
                }
                catch (FileNotFoundException)
                {
                    return true;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex);
                    if (attemptCount > 1)
                    {
                        return false;
                    }

                    attemptCount++;
                    if (!DeleteFile(dest, false))
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return false;
                }
            }
        }

        #endregion Methods
    }
}
