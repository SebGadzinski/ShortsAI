using MediaCreatorFunctions.Services;
using Newtonsoft.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MediaCreatorFunctions.Services
{
    public interface IBlobService : IFileService { }
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainer;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BlobService> _logger;

        private static readonly char[] InvalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
        private static readonly string[] ReservedNames =
        {
        "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5",
        "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4",
        "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        public BlobService(IConfiguration configuration, ILogger<BlobService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _blobServiceClient = new BlobServiceClient(_configuration["AzureWebJobsStorage"]);
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
                var response = await _blobContainer.DeleteBlobIfExistsAsync(fullPath);
                if (!response.Value) _logger.LogWarning($"{fullPath} - File Did Not Exist");
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

        public string SanitizeName(string originalName)
        {
            // Replace invalid characters with _
            string sanitized = new string(originalName
                .Select(ch => InvalidFileNameChars.Contains(ch) ? '_' : ch)
                .ToArray());

            // Check for reserved names and append _ if necessary
            if (ReservedNames.Contains(sanitized, StringComparer.OrdinalIgnoreCase))
            {
                sanitized += "_";
            }

            return sanitized;
        }
    }
}
