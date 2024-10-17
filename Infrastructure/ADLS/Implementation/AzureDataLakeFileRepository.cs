using Azure.Storage.Files.DataLake;
using Core.Entities;
using Infrastructure.ADLS.Abstraction;

namespace Infrastructure.ADLS.Implementation
{
    public class AzureDataLakeFileRepository : IFileRepository<FileEntity>
    {
        private readonly DataLakeServiceClient _serviceClient;
        private readonly DataLakeFileSystemClient _fileSystemClient;
        public AzureDataLakeFileRepository(string connectionString, string fileSystemName)
        {
            _serviceClient = new DataLakeServiceClient(connectionString);
            _fileSystemClient = _serviceClient.GetFileSystemClient(fileSystemName);
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            var fileClient = _fileSystemClient.GetFileClient(fileId);
            await fileClient.DeleteAsync();
            return true;

        }

        public async Task<Stream> DownloadFileAsync(string fileId)
        {
            var fileClient = _fileSystemClient.GetFileClient(fileId);
            var response = await fileClient.ReadAsync();
            return response.Value.Content;

        }

        public async Task<IEnumerable<FileEntity>> GetFilesAsync()
        {
            var paths = _fileSystemClient.GetPathsAsync();
            var files = new List<FileEntity>();

            await foreach (var pathItem in paths)
            {
                files.Add(new FileEntity
                {

                    FileName = pathItem.Name,
                    FileUrl = _fileSystemClient.GetFileClient(pathItem.Name).Uri.AbsoluteUri
                });
            }

            return files;

        }

        public async Task<FileEntity> UploadFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                var fileClient = _fileSystemClient.GetFileClient(fileName);


                fileStream.Position = 0;


                await fileClient.UploadAsync(fileStream, overwrite: true);


                var properties = await fileClient.GetPropertiesAsync();

                return new FileEntity
                {

                    FileName = fileName,
                    FileUrl = fileClient.Uri.AbsoluteUri,
                    FileSize = properties.Value.ContentLength,
                    ContentType = properties.Value.ContentType

                };
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error uploading file to Azure Data Lake: {ex.Message}");
                throw;
            }
        }




    }
}
