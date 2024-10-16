using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.Services
{
    public interface IFileService
    {
        public Task SaveFile(byte[] fileBytes, string fullPath);
        public void EnsureFolderExistenceFolderPath(string folderPath);
        void EnsureFolderExistenceFromFilePath(string filePath);
        public List<string> GetFileNames(string directoryPath);
        Task<T> Get<T>(string filePath);
        Task Set<T>(string filePath, T obj);
        Task DeleteContent(string folderPath, string except = "");
        Task DeleteFile(string fullPath);
        string SanitizeName(string originalName);
    }
    public class FileService : IFileService
    {
        private readonly ILogger<DeepAIService> _logger;
        private readonly IConfiguration _configuration;

        private static readonly char[] InvalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
        private static readonly string[] ReservedNames =
        {
        "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5",
        "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4",
        "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        public FileService(ILogger<DeepAIService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SaveFile(byte[] fileBytes, string fullPath)
        {
            try
            {
                EnsureFolderExistenceFromFilePath(fullPath);
                using var file = new FileStream(fullPath, FileMode.Create);
                await file.WriteAsync(fileBytes, 0, fileBytes.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError($"FileService - SaveFile - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        public void EnsureFolderExistenceFolderPath(string folderPath)
        {
            try
            {
                // Check if the directory exists
                if (!Directory.Exists(folderPath))
                {
                    // If not, create the directory
                    Directory.CreateDirectory(folderPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"FileService - EnsureFolderExistenceFolderPath - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        //Ensure all folders are created for this file's path
        //Filepath example: C:\test\me\out\ss.exe
        //Here ensure these files are created test\me\out
        public void EnsureFolderExistenceFromFilePath(string filePath)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"FileService - EnsureFolderExistenceFromFilePath - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        // Get JSON object from the specified file path
        public Task<T> Get<T>(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                T result = JsonConvert.DeserializeObject<T>(json);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"FileService - Get - {JsonConvert.SerializeObject(ex)}");
                return default;
            }
        }

        // Sets JSON object at the specified file path
        public Task Set<T>(string filePath, T obj)
        {
            try
            {
                EnsureFolderExistenceFromFilePath(filePath);
                string json = JsonConvert.SerializeObject(obj);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"FileService - Set - {JsonConvert.SerializeObject(ex)}");
                throw;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes all content in the folder but not the folder itself
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="except">Allow all files or folders with this name in the directory to stay</param>
        public Task DeleteContent(string folderPath, string except = "")
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    DirectoryInfo directory = new DirectoryInfo(folderPath);
                    foreach (FileInfo file in directory.GetFiles())
                    {
                        if (file.Name != except)
                        {
                            file.Delete();
                        }
                    }
                    foreach (DirectoryInfo subdirectory in directory.GetDirectories())
                    {
                        if (subdirectory.Name != except)
                        {
                            subdirectory.Delete(true);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Directory does not exist.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"FileService - Delete Content - {JsonConvert.SerializeObject(ex)}");
                throw;
            }

            return Task.CompletedTask;
        }

        public List<string> GetFileNames(string directoryPath)
        {
            List<string> fileNames = new List<string>();

            try
            {
                if (Directory.Exists(directoryPath))
                {
                    string[] files = Directory.GetFiles(directoryPath);

                    foreach (string file in files)
                    {
                        fileNames.Add(Path.GetFileName(file));
                    }
                }
                else
                {
                    Console.WriteLine("Directory does not exist.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"FileService - GetFileNames - {JsonConvert.SerializeObject(ex)}");
                throw;
            }

            return fileNames;
        }

        public Task DeleteFile(string fullPath)
        {
            throw new NotImplementedException();
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