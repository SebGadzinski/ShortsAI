using MediaCreatorFunctions.DataAccess;
using MediaCreatorFunctions.DataAccess.Constants;
using MediaCreatorFunctions.DataAccess.DTO;
using MediaCreatorFunctions.Models;
using MediaCreatorFunctions.Utility.Constants;
using MediaCreatorFunctions.Utility.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace MediaCreatorFunctions.Services
{
    /// <summary>
    /// Start once we get the videos down. Lets make the product first lol
    /// </summary>
    public interface IMediaService
    {
        Task<string> ProcessVideo(Video video, int width, int height, int voice_id, IGeneratePictureService pictureService, Guid userId);
        Task DeleteWork(Guid userId);
    }
    /// <summary>
    /// Handles mass media functionality
    /// </summary>
    public class MediaService : IMediaService
    {
        private readonly ILogger<MediaService> _logger;
        private readonly IChatGPTService _chatGPTService;
        private readonly IDeepAIService _deepAIService;
        private readonly IAudioService _audioService;
        private readonly IVideoService _videoService;
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly IMediaCreatorDatabase _database;

        public MediaService(ILogger<MediaService> logger, IChatGPTService chatGPTService, IDeepAIService deepAIService, IAudioService audioService,
            IVideoService videoService, IFileService fileService, IConfiguration configuration, IMediaCreatorDatabase database)
        {
            _logger = logger;
            _chatGPTService = chatGPTService;
            _deepAIService = deepAIService;
            _audioService = audioService;
            _videoService = videoService;
            _fileService = fileService;
            _configuration = configuration;
            _database = database;
        }

        public async Task<string> ProcessVideo(Video video, int width, int height, int voice_id, IGeneratePictureService pictureService, Guid userId)
        {
            try
            {
                //State it is now processing
                video.status_type_id = StatusTypes.PROCESSING.Id;
                video.modified_date = DateTime.UtcNow;
                video.modified_by = nameof(MediaService);
                await _database.UpdateAsync(video);

                var voice = Voices.ALL_VOICES.FirstOrDefault(x => x.Id == voice_id);
                if (voice == null) throw new ObjectDoesNotExistException("Voice", voice_id);

                //Validate picture demensions are fine
                if (!pictureService.DimensionsOk(width, height)) throw new DimensionsNotOkException();

                //Ensure working directory exists
                var workingDirectory = $"{_configuration["WorkingDirectory"]}\\{video.user_id}\\{video.id}\\{video.title}";
                _fileService.EnsureFolderExistenceFolderPath(workingDirectory);

                //Get the script content
                var scriptData = await _chatGPTService.CreateContentScriptAsync(video.title, userId);

                //Create scenes
                var scenes = new List<Scene>();
                for (var i = 0; i < scriptData.Components.Count; i++)
                {
                    var scene = new Scene() { text = scriptData.Components[i].Text };
                    //Create Pictures
                    scene.pictureFilePath = await pictureService.GeneratePicture(scriptData.Components[i].PictureText, $"{workingDirectory}\\s{i}", width, height, userId);
                    //Generate Audio File
                    scriptData.Components[i].Tags.Add(voice.Name);
                    scene.audioFilePath = await _audioService.CreateAudio(scriptData.Components[i].Text, scriptData.Components[i].Tags, $"{workingDirectory}\\s{i}", userId);
                    scenes.Add(scene);
                }

                return await _videoService.GenerateVideoFromScenes(scenes, $"{workingDirectory}\\video");
            }
            catch (Exception ex)
            {
                _logger.LogError($"MediaService - {nameof(ProcessVideo)} - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }

        public async Task DeleteWork(Guid userId)
        {
            try
            {
                var deleteDirectoryData = $"{_configuration["WorkingDirectory"]}\\{userId}";
                await _fileService.DeleteContent(deleteDirectoryData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"MediaService - {nameof(DeleteWork)} - {JsonConvert.SerializeObject(ex)}");
                throw;
            }
        }
    }
}
