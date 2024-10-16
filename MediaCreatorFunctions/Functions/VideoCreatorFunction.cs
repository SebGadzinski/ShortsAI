using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediaCreatorFunctions.DataAccess;
using MediaCreatorFunctions.DataAccess.Constants;
using MediaCreatorFunctions.DataAccess.Dto;
using MediaCreatorFunctions.DataAccess.DTO;
using MediaCreatorFunctions.Services;
using MediaCreatorFunctions.Utility.Exceptions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MediaCreatorFunctions.Functions
{
    public class VideoCreatorFunction
    {

        private readonly IMediaCreatorDatabase _database;
        private readonly IMediaService _mediaService;
        private readonly IBlobService _blobService;
        private readonly IChatGPTService _chatGPTService;
        private readonly IDeepAIService _deepAIService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VideoCreatorFunction> _logger;

        private const double VideoCreditCharge = 1.00;
        private readonly HashSet<int> PROCESS_STATUS_TYPES = new HashSet<int>() { StatusTypes.WAITING.Id };

        public VideoCreatorFunction(IMediaCreatorDatabase database, IMediaService mediaService, IBlobService blobService, IChatGPTService chatGPTService, IDeepAIService deepAIService, IConfiguration configuration, ILogger<VideoCreatorFunction> logger)
        {
            _database = database;
            _mediaService = mediaService;
            _blobService = blobService;
            _chatGPTService = chatGPTService;
            _deepAIService = deepAIService;
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName("VideoCreatorFunction")]
        public async Task Run([BlobTrigger("videos/Create/{videoString}", Connection = "AzureWebJobsStorage")] Stream myBlob, string videoString)
        {
            Video video = null;
            try
            {
                var videoId = Convert.ToInt32(videoString.Split("_")[1]);
                video = await _database.FirstOrDefaultAsync<Video>("id = @videoId", new { videoId });
                if (video == null) throw new ObjectDoesNotExistException("Video", videoId);

                //Validate user has enough credits again
                var credit = await _database.FirstOrDefaultAsync<Credit>("user_id = @userId", new { @userId = video.user_id });
                if (credit == null) throw new ObjectDoesNotExistException("Credit - user_id", video.user_id);
                if (credit.amount < VideoCreditCharge) throw new NotEnoughCreditsException();

                if (PROCESS_STATUS_TYPES.Contains(video.status_type_id))
                {
                    await _mediaService.DeleteWork(video.user_id);

                    var user = await _database.FirstOrDefaultAsync<AppUser>("id = @userId", new { @userId = video.user_id });
                    if (user == null) throw new ObjectDoesNotExistException("User", video.user_id);
                    //Check if user is locked out or in bad state and dont allow if so

                    IGeneratePictureService pictureService = video.picture_store_id.Equals(Stores.CHAT_GPT.Id) ? _chatGPTService : _deepAIService;
                    var videoFileUrl = await _mediaService.ProcessVideo(video, video.width, video.height, video.voice_id, pictureService, video.user_id);

                    //Add video to videos/Ready/videoId
                    await _blobService.SaveFile(File.ReadAllBytes(videoFileUrl), $"Ready/v_{video.id}_/{video.title}.mp4");

                    await _mediaService.DeleteWork(video.user_id);

                    video.status_type_id = StatusTypes.COMPLETE.Id;
                    await _database.UpdateAsync(video);

                    //Take away credits for video
                    credit.amount -= VideoCreditCharge;
                    credit.modified_date = DateTime.UtcNow;
                    credit.modified_by = nameof(VideoCreatorFunction);
                    await _database.UpdateAsync(credit);
                    await _database.InsertAsync(new CreditPurchaseHistory()
                    {
                        credit_id = credit.id,
                        user_id = user.id,
                        amount = Convert.ToDouble(VideoCreditCharge),
                        created_date = DateTime.UtcNow,
                        modified_date = DateTime.UtcNow,
                        modified_by = nameof(VideoCreatorFunction),
                    });
                }
            }
            catch (Exception ex)
            {
                //If the video not was found set it to failed
                if(video == null)
                {
                    _logger.LogError($"VideoCreationFunction Could Not Get Video Error: {videoString}");
                }
                else
                {
                    video.status_type_id = StatusTypes.FAILED.Id;
                    await _database.UpdateAsync(video);
                }
                _logger.LogError($"VideoCreationFunction  Error: {JsonConvert.SerializeObject(ex)}");
            }
            finally
            {
                //Delete the video from the /Create and put the video in /Ready
                await _blobService.DeleteFile($"Create/{videoString}");
            }
        }
    }
}
