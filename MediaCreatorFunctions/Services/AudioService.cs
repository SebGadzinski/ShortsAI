using MediaCreatorFunctions.DataAccess.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using Google.Cloud.TextToSpeech.V1;
using MediaCreatorFunctions.DataAccess.Constants;
using static System.Net.Mime.MediaTypeNames;
using MediaCreatorFunctions.DataAccess;

namespace MediaCreatorFunctions.Services
{
    public interface IAudioService
    {
        Task<string> CreateAudio(string text, IEnumerable<string> voiceTags, string filePath, Guid userId, string fileName = "audio.mp3");
    }
    public class AudioService : IAudioService
    {

        private readonly ILogger<AudioService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICostService _costService;
        private readonly IFileService _fileService;
        private readonly IMediaCreatorDatabase _database;
        private readonly TextToSpeechClient _textToSpeechClient;

        private int COST_AFTER_CHARACTERS = 1000000;
        private const double COST_PER_MILLION_CHARACTERS = 5.449799999;
        private const double COST_PER_CHARACTER = COST_PER_MILLION_CHARACTERS / 1000000;  // This is the cost per character.

        public AudioService(ILogger<AudioService> logger, IConfiguration configuration, ICostService costService, IFileService fileService, IMediaCreatorDatabase database)
        {
            _logger = logger;
            _configuration = configuration;
            _textToSpeechClient = TextToSpeechClient.Create();
            _costService = costService;
            _fileService = fileService;
            _database = database;
        }

        /// <summary>
        /// Generates a mp3 file with the text given
        /// </summary>
        /// <param name="text"></param>
        /// <param name="voiceTags">Voice of the speech is based off of what is given here</param>
        /// <param name="outputFilePathWithName"></param>
        /// <returns></returns>
        public async Task<string> CreateAudio(string text, IEnumerable<string> voiceTags, string filePath, Guid userId, string fileName = "audio.mp3")
        {
            try
            {
                // Configure the voice
                VoiceSelectionParams voice = new VoiceSelectionParams
                {
                    LanguageCode = "en-US",
                    Name = "en-US-Studio-M",  // You can modify the language code as per your requirement.
                    SsmlGender = SsmlVoiceGender.Male  // You can modify the voice gender as per your requirement.
                };

                // Check if the voice tags contain either Male or Female and assign accordingly.
                foreach (string tag in voiceTags)
                {
                    if (tag.ToLower() == "female")
                    {
                        voice.Name = "en-US-Studio-O";
                        voice.SsmlGender = SsmlVoiceGender.Female;
                    }
                }

                // Configure the audio
                AudioConfig config = new AudioConfig
                {
                    AudioEncoding = AudioEncoding.Mp3,
                    Pitch = -6,
                    SpeakingRate = 1.15,
                };

                // Perform the Text-to-Speech request
                SynthesizeSpeechResponse response = await _textToSpeechClient.SynthesizeSpeechAsync(new SynthesisInput { Text = text }, voice, config);

                await _costService.AddReceipt(Stores.GOOGLE.Id, CalculateCost(text), "Google Text To Speech", userId);

                // Write the response to the output file.
                var finalPath = Path.Combine(filePath, fileName);
                _fileService.EnsureFolderExistenceFromFilePath(finalPath);
                using (Stream output = File.Create(finalPath))
                {
                    response.AudioContent.WriteTo(output);
                }
                // Return the audio object or its path
                return finalPath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AudioService - {nameof(CreateAudio)} - {ex.Message}");
                throw;
            }
        }

        public double CalculateCost(string text)
        {
            return text.Length * COST_PER_CHARACTER;
        }


    }
}
