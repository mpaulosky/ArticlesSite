namespace Web.Services
{
    public interface IFileStorage
    {
        Task<string> AddFile(FileData fileData);
    }

    public class FileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorage> _logger;

        public FileStorage(IWebHostEnvironment environment, ILogger<FileStorage> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> AddFile(FileData fileData)
        {
            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate a unique filename to prevent collisions
                var extension = Path.GetExtension(fileData.MetaData.Name);
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                // Save the file
                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await fileData.Content.CopyToAsync(fileStream);
                }

                _logger.LogInformation("File saved successfully: {FileName}", uniqueFileName);
                return uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {FileName}", fileData.MetaData.Name);
                throw;
            }
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
