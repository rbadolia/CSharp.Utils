using System.IO;
using System.Text;
using CSharp.Utils.Validation;

namespace CSharp.Utils.IO
{
    public class WindowsFileStore : IFileStore
    {
        public WindowsFileStore()
        {
            this.RootPath=@"C:\temp";
        }

        public WindowsFileStore(string rootPath)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(rootPath, "rootPath");
            this.RootPath = rootPath;
        }

        public string RootPath { get; set; }

        public void StoreFile(Stream inputStream, string filePath, bool overwrite, Encoding encoding = null)
        {
            Guard.ArgumentNotNull(inputStream, "inputStream");
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(filePath, "filePath");
            var fullPath = IOHelper.ResolvePath(filePath, this.RootPath);
            var directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (StreamWriter writer = encoding == null
                ? new StreamWriter(fullPath)
                : new StreamWriter(fullPath, false, encoding))
            {
                inputStream.CopyTo(writer.BaseStream);
                writer.Close();
            }
        }

        public void GetFile(string filePath, Stream streamToWriteInto)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(filePath, "filePath");
            Guard.ArgumentNotNull(streamToWriteInto, "streamToWriteInto");
            var fullPath = IOHelper.ResolvePath(filePath, this.RootPath);
            using (StreamReader reader = new StreamReader(fullPath))
            {
                reader.BaseStream.CopyTo(streamToWriteInto);
                reader.Close();
            }
        }

        public void DeleteFile(string filePath)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(filePath, "filePath");
            var fullPath = IOHelper.ResolvePath(filePath, this.RootPath);
            File.Delete(fullPath);
        }

        public void DeleteDirectory(string directoryPath)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(directoryPath, "directoryPath");
            var fullPath = IOHelper.ResolvePath(directoryPath, this.RootPath);
            Directory.Delete(fullPath);
        }
    }
}
