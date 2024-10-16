using MediaCreatorFunctions.DataAccess;
using MediaCreatorFunctions.DataAccess.Constants;
using MediaCreatorFunctions.DataAccess.Dto;
using MediaCreatorFunctions.DataAccess.DTO;
using MediaCreatorFunctions.Services;
using MediaCreatorFunctions.Utility.Constants;
using MediaCreatorFunctions.Utility.Exceptions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NAudio.Codecs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.Functions.SocialMedia
{
    public class UploadToYoutubeFunction
    {
        private readonly IMediaCreatorDatabase _database;
        private readonly IMediaService _mediaService;
        private readonly IFileService _fileService;
        private readonly IBlobService _blobService;
        private readonly IChatGPTService _chatGPTService;
        private readonly IYoutubeService _youtubeService;
        private readonly IDeepAIService _deepAIService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UploadToYoutubeFunction> _logger;

        public UploadToYoutubeFunction(IMediaCreatorDatabase database, IMediaService mediaService, IFileService fileService, IBlobService blobService, IChatGPTService chatGPTService, IYoutubeService youtubeService, IDeepAIService deepAIService, IConfiguration configuration, ILogger<UploadToYoutubeFunction> logger)
        {
            _database = database;
            _mediaService = mediaService;
            _fileService = fileService;
            _blobService = blobService;
            _chatGPTService = chatGPTService;
            _youtubeService = youtubeService;
            _deepAIService = deepAIService;
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName("UploadToYoutube")]
        public async Task UploadToYoutube([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        //public async Task RetryYoutubeUpload([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
        {
            try
            {
                #region Check if youtube production is halted
                var config = await _database.FirstOrDefaultAsync<Config>($"name = '{Configs.YOUTUBE_HALT}'");
                if (config != null)
                {
                    var date = DateTime.Parse(config.value);
                    if (date > DateTime.UtcNow)
                    {
                        _logger.LogWarning($"Youtube production halted. Waiting till: {date}");
                        return;
                    }
                }
                #endregion

                var user = await _database.FirstOrDefaultAsync<AppUser>("user_name = @name", new { @name = SocialMedias.YOUTUBE.Name });

                #region Ensure Media User Exists
                //Create media user
                if (user == null)
                {
                    user = await _database.InsertAsync(new AppUser()
                    {
                        email = SocialMedias.YOUTUBE.Name,
                        password = "media",
                        email_confirmed = true,
                        phone_number = "",
                        phone_number_confirmed = false,
                        two_factor_enabled = false,
                        user_name = SocialMedias.YOUTUBE.Name,
                        modified_date = DateTime.UtcNow,
                        modified_by = nameof(YoutubeFunction),
                        created_date = DateTime.UtcNow,
                    });
                }
                #endregion

                var uploadVideoToYoutube = await _database.FirstOrDefaultAsync<Video>("user_id = @id and status_type_id = @status_id",
                    new { @id = user.id, @status_id = StatusTypes.UPLOAD_TO_YOUTUBE.Id });

                if (uploadVideoToYoutube != null)
                {
                    var youtubeCategory = await _database.GetVideoCategory(uploadVideoToYoutube.id);
                    await _youtubeService.UploadVideo($"C:\\MediaCreatorData\\{user.id}\\{uploadVideoToYoutube.id}\\{uploadVideoToYoutube.title}\\video\\output_subtitles.mp4", youtubeCategory.title, _youtubeService.GetDescription(), uploadVideoToYoutube);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(YoutubeFunction)} - Error - {JsonConvert.SerializeObject(ex)}");
            }
        }
    }
}
