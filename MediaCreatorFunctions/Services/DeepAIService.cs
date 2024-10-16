using DeepAI;
using MediaCreatorFunctions.DataAccess;
using MediaCreatorFunctions.DataAccess.Constants;
using MediaCreatorFunctions.DataAccess.DTO;
using MediaCreatorFunctions.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.Services
{
    public interface IDeepAIService : IGeneratePictureService
    {
    }
    public class DeepAIService : IDeepAIService
    {
        private readonly ILogger<DeepAIService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;
        private readonly ICostService _costService;
        private readonly IMediaCreatorDatabase _database;
        private readonly DeepAI_API api;
        private readonly HttpClient client;

        private const double COST_PER_PICTURE = 0.05;

        public DeepAIService(ILogger<DeepAIService> logger, IConfiguration configuration, IFileService fileService, ICostService costService, IMediaCreatorDatabase database)
        {
            _logger = logger;
            _configuration = configuration;
            _fileService = fileService;
            _costService = costService;
            _database = database;
            api = new DeepAI_API(apiKey: _configuration["DeepAI:API_KEY"]);
            client = new HttpClient();
        }

        public async Task<string> GeneratePicture(string prompt,string folderPath, int width, int height, Guid userId, string filename = "picture.png")
        {
            try
            {
                var response = api.callStandardApi("text2img", new
                {
                    text = prompt,
                    grid_size = "1",
                    height = width.ToString(),
                    width = height.ToString()
                });

                await _costService.AddReceipt(Stores.DEEP_AI.Id, COST_PER_PICTURE, "Picture Generation", userId);

                //Download image from link
                byte[] imageBytes = await client.GetByteArrayAsync(response.output_url);
                string fullPath = Path.Combine(folderPath, filename);
                _fileService.EnsureFolderExistenceFromFilePath(fullPath);
                using var fileStream = File.Create(fullPath);
                await fileStream.WriteAsync(imageBytes, 0, imageBytes.Length);

                return fullPath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeepAIService - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        public bool DimensionsOk(int width, int height)
        {
            //Check to see if dimensions are between 128 and 1536.
            return width >= 128 && width <= 1536 && height >= 128 && height <= 1536;
        }

    }
}
