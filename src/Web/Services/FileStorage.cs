using System;
using System.IO;
using System.Threading.Tasks;

namespace Web.Services
{
    public interface IFileStorage
    {
        Task<string> AddFile(FileData fileData);
    }

    public class FileStorage : IFileStorage
    {
        public async Task<string> AddFile(FileData fileData)
        {
            // Implement file saving logic here
            await Task.CompletedTask;
            return fileData.MetaData.Name; // Return file name or generated ID
        }
    }

    public class FileData
    {
        public Stream Content { get; }
        public FileMetaData MetaData { get; }

        public FileData(Stream content, FileMetaData metaData)
        {
            Content = content;
            MetaData = metaData;
        }
    }

    public class FileMetaData
    {
        public string Name { get; }
        public string ContentType { get; }
        public DateTime LastModified { get; }

        public FileMetaData(string name, string contentType, DateTime lastModified)
        {
            Name = name;
            ContentType = contentType;
            LastModified = lastModified;
        }
    }
}
