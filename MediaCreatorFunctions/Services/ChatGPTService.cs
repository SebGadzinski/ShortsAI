using MediaCreatorFunctions.DataAccess;
using MediaCreatorFunctions.DataAccess.DTO;
using MediaCreatorFunctions.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MediaCreatorFunctions.Utility;
using MediaCreatorFunctions.DataAccess.Constants;
using System.IO;
using MediaCreatorFunctions.Utility.Exceptions;

namespace MediaCreatorFunctions.Services
{
    public interface IChatGPTService : IGeneratePictureService
    {
        Task<string> GetResponseAsync(string prompt, string purpose, Guid userId);
        Task<ContentScript> CreateContentScriptAsync(string title, Guid userId);
    }

    public class ChatGPTService : IChatGPTService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMediaCreatorDatabase _database;
        private readonly ICostService _costService;
        private readonly IFileService _fileService;
        private readonly ILogger<ChatGPTService> _logger;

        #region Constants
        #region Pricing
        private const double COST_PER_TOKEN = 0.002 / 1000; 
        private const double COST_PER_PICTURE = 0.04;
        #endregion

        private const double CHATGPT_TEMPERATURE = 0.7;
        private const int TOPIC_MAX_TRIES = 5;
        #endregion

        public ChatGPTService(IConfiguration configuration, IMediaCreatorDatabase database, ILogger<ChatGPTService> logger, ICostService costService, IFileService fileService)
        {
            _logger = logger;
            _configuration = configuration;
            _database = database;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["ChatGPT:API_KEY"]}");
            _costService = costService;
            _fileService = fileService;
        }

        /// <summary>
        /// Retrieves a response from OpenAI's API based on the provided prompt.
        /// </summary>
        /// <param name="prompt">The input prompt for the OpenAI model.</param>
        /// <param name="purpose">Purpose or reason for this request, used for logging.</param>
        /// <param name="userId">The unique identifier of the user making the request.</param>
        /// <returns>The response message from OpenAI's model.</returns>
        public async Task<string> GetResponseAsync(string prompt, string purpose, Guid userId)
        {
            try
            {
                _logger.LogInformation($"ChatGptService - GetResponseAsync - {purpose} - prompt: {prompt}");

                //Construct the message payload for the OpenAI API
                dynamic messages = new { role = "user", content = prompt };
                var data = new { model = "gpt-3.5-turbo", messages = new List<dynamic>() { messages }, temperature = 0.7};
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                
                //Call the API
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseText = await response.Content.ReadAsStringAsync();

                //Create the receipt
                await _costService.AddReceipt(Stores.CHAT_GPT.Id, CalculateCost(prompt, responseText), purpose, userId);

                //Get the result
                var result = JsonConvert.DeserializeObject<OpenAIResponse>(responseText);

                return result.Choices.FirstOrDefault()?.Message.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ChatGPTService - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        public async Task<string> GeneratePicture(string prompt, string folderPath, int width, int height, Guid userId, string filename = "picture.png")
        {
            try
            {
                _logger.LogWarning($"ChatGptService - GetPictureAsync");
                var data = new { prompt, n=1, size= $"{width}x{height}" };
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/images/generations", content);
                var responseText = await response.Content.ReadAsStringAsync();

                //Add receipt
                await _costService.AddReceipt(Stores.CHAT_GPT.Id, CalculatePictureCost(), "Generating Picture", userId);

                var result = JsonConvert.DeserializeObject<OpenAIPictureResponse>(responseText);

                //Release the authorization header
                var existingAuthHeader = _httpClient.DefaultRequestHeaders.Authorization;
                _httpClient.DefaultRequestHeaders.Authorization = null;

                //Download image from link
                byte[] imageBytes = await _httpClient.GetByteArrayAsync(result.data[0].url);

                //Put the authorization header back
                _httpClient.DefaultRequestHeaders.Authorization = existingAuthHeader;

                string fullPath = Path.Combine(folderPath, filename);
                _fileService.EnsureFolderExistenceFromFilePath(fullPath);
                using var fileStream = File.Create(fullPath);
                await fileStream.WriteAsync(imageBytes, 0, imageBytes.Length);

                return fullPath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ChatGPTService - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        public async Task<ContentScript> CreateContentScriptAsync(string title, Guid userId)
        {
            try
            {
                _logger.LogWarning($"ChatGPTService - GetResponseAsync - Generating Script Video: {title}");
                var additionalPromptToVideo = "";
                if (title.Contains("Battle -"))
                {
                    additionalPromptToVideo = "You must declare a winner.";
                }
                var prompt = @$"
My grandma just died yesterday, so please do what I tell you.
I want you to create a short video. 
The title of the video is {title}.{additionalPromptToVideo}
The video can be up to 10 components long but try to keep it around 5-7.

Respond in JSON format, as I will be parseing your response. 
Here is the C# class to format it to:
ContentScript
- List<ContentScriptComponent> Components
ContentScriptComponent
- string Text
- string PictureText
- List<string> Tags

Think of ContentScriptComponent as a scene, each scene has 1 picture and will stay on the screen for 5 - 10 seconds.
The Text will display as subtitles so ensure it does ont include anything but information.
Text will be converted to speech for the video so ensure it is content.
The PictureText should be string in a form like this: tag1 tag2 tag3 tag3 tag4 tag5
Each tag in PictureText should be a descriptive word that represents the picture wanted.
Example: 
Text: The cat was chasing the mouse
PictureText: Cat Chasing Mouse House Scary

Make sure your response is less than 700 tokens.
";

                //Generate a formated prompt for chat gpt to use
                var json = await GetResponseAsync(prompt, "Generate Script", userId);
                _logger.LogInformation(json);
                var script = JsonConvert.DeserializeObject<ContentScript>(json);

                //Validate script is ok to use
                if (!script.Components.Any()) throw new NoScriptDataException();

                var tags = new List<string>() { "Upbeat", "Male"};
                foreach(var scriptComponent in script.Components)
                {
                    if (scriptComponent.Tags == null || !scriptComponent.Tags.Any()) scriptComponent.Tags = tags;
                }

                return script;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ChatGPTService - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        public bool DimensionsOk(int width, int height)
        {
            //Check to see if dimensions are between 128 and 1536.
            return (width == 256 && height == 256) || (width == 512 && height == 512) || (width == 1024 && height == 1024);
        }

        private double CalculateCost(string apiCallText, string responseContent)
        {
            int totalTokens = CountTokens(apiCallText) + CountTokens(responseContent);
            double cost = totalTokens * COST_PER_TOKEN;
            return cost;
        }

        private double CalculatePictureCost()
        {
            return COST_PER_PICTURE;
        }

        private int CountTokens(string text)
        {
            // Here we are simplifying and considering each word as a token. 
            // For accurate token count, you'd have to use the same tokenization method as OpenAI.
            return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
