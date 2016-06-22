using System.IO;
using System.Text;

namespace CSharp.Utils.IO
{
    public interface IFileStore
    {
        void StoreFile(Stream inputStream, string filePath, bool overwrite, Encoding encoding = null);

        void GetFile(string filePath, Stream streamToWriteInto);

        void DeleteFile(string filePath);

        void DeleteDirectory(string directoryPath);
    }
}
