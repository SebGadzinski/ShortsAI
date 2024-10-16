using MediaCreatorSite.Services;
using Newtonsoft.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Sas;
using Azure.Storage;

namespace MediaCreatorSite.Services
{
    public interface IBlobService
    {
        public Task SaveFile(byte[] fileBytes, string fullPath);
        public void EnsureFolderExistenceFolderPath(string folderPath);
        void EnsureFolderExistenceFromFilePath(string filePath);
        public List<string> GetFileNames(string directoryPath);
        Task<T> Get<T>(string filePath);
        Task Set<T>(string filePath, T obj);
        Task DeleteContent(string folderPath, string except = "");
        Task DeleteFile(string fullPath);
        string GenerateSasTokenForBlob(string blobName);
    }
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainer;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BlobService> _logger;

        public BlobService(IConfiguration configuration, ILogger<BlobService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _blobServiceClient = new BlobServiceClient(_configuration["AzureBlobStorage:ConnectionString"]);
            _blobContainer = _blobServiceClient.GetBlobContainerClient(_configuration["AzureBlobStorage:Container"]);
        }

        public async Task SaveFile(byte[] fileBytes, string fullPath)
        {
            try
            {
                using var stream = new MemoryStream(fileBytes, writable: false);
                await _blobContainer.UploadBlobAsync(fullPath, stream);
            }
            catch (Exception ex)
            {
                _logger.LogError($"BlobService - SaveFile - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        public async Task DeleteFile(string fullPath)
        {
            try
            {
                await _blobContainer.DeleteBlobIfExistsAsync(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"BlobService - DeleteFile - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        // No need to ensure folders, as blob storage will create them if they don't exist
        public void EnsureFolderExistenceFolderPath(string folderPath) { }
        public void EnsureFolderExistenceFromFilePath(string filePath) { }

        public List<string> GetFileNames(string directoryPath)
        {
            try
            {
                var blobs = _blobContainer.GetBlobs(BlobTraits.None, BlobStates.None, directoryPath);
                return blobs.Select(b => b.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"BlobService - GetFileNames - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        public async Task<T> Get<T>(string filePath)
        {
            try
            {
                var blobClient = _blobContainer.GetBlobClient(filePath);
                BlobDownloadInfo download = await blobClient.DownloadAsync();
                using (var reader = new StreamReader(download.Content))
                {
                    string content = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"BlobService - Get - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        public async Task Set<T>(string filePath, T obj)
        {
            try
            {
                var blobClient = _blobContainer.GetBlobClient(filePath);
                var content = JsonConvert.SerializeObject(obj);
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"BlobService - Set - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        public async Task DeleteContent(string folderPath, string except = "")
        {
            try
            {
                var blobs = _blobContainer.GetBlobs(prefix: folderPath);
                foreach (var blob in blobs)
                {
                    if (blob.Name != except)
                    {
                        var blobClient = _blobContainer.GetBlobClient(blob.Name);
                        await blobClient.DeleteIfExistsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"BlobService - DeleteContent - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        public string GenerateSasTokenForBlob(string blobName)
        {
            try
            {
                var blobClient = _blobContainer.GetBlobClient(blobName);
                // Create a SAS token that's valid for one hour
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = _blobContainer.Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };
                sasBuilder.StartsOn = DateTimeOffset.UtcNow;
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                // Use the key to get the SAS token
                string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_configuration["AzureBlobStorage:AccountName"], _configuration["AzureBlobStorage:AccountKey"])).ToString();

                // Construct the full URI, including the SAS token
                UriBuilder fullUri = new UriBuilder()
                {
                    Scheme = "https",
                    Host = $"{_configuration["AzureBlobStorage:AccountName"]}.blob.core.windows.net",
                    Path = $"{_blobContainer.Name}/{blobClient.Name}",
                    Query = sasToken
                };

                return fullUri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"BlobService - GenerateSasTokenForBlob - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }
    }
}
