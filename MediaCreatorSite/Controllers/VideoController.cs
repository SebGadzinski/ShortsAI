using MediaCreatorSite.DataAccess;
using MediaCreatorSite.DataAccess.Constants;
using MediaCreatorSite.DataAccess.DTO;
using MediaCreatorSite.DataAccess.QueryModels;
using MediaCreatorSite.Models;
using MediaCreatorSite.Services;
using MediaCreatorSite.Utility.Attributes;
using MediaCreatorSite.Utility.Exceptions;
using MediaCreatorSite.Utility.Results;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAI_API.Moderation;
using SendGrid.Helpers.Mail;

namespace MediaCreatorSite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly IMediaCreatorDatabase _database;
        private readonly IBlobService _blobService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VideoController> _logger;

        public VideoController(IMediaCreatorDatabase database, IBlobService blobService, IEmailService emailService,  ILogger<VideoController> logger, IConfiguration configuration)
        {
            _database = database;
            _blobService = blobService;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        public class VideoIndexData
        {
            public IEnumerable<CollectableVideo> collectableVideos { get; set; } = new List<CollectableVideo>();
            public IEnumerable<VideoRow> historyVideos { get; set; } = new List<VideoRow>();
        }

        [EmailVerified]
        [HttpGet]
        public async Task<string> Get()
        {
            var result = new DataResult<VideoIndexData>() { data = new VideoIndexData()};
            try
            {
                var sessionInfo = HttpContext.Items["SessionInfo"] as SessionInfo;
                result.data.collectableVideos = await _database.GetCollectableVideosAsync(sessionInfo.user.id);
                result.data.historyVideos = await _database.GetVideoTableDataAsync(sessionInfo.user.id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Video Controller - PurchaseCredits - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }

            return result.CloseResult();
        }

        public class GetVideoinfoRequest
        {
            public int id { get; set; }
        }
        public class GetVideoinfoResponse
        {
            public string status { get; set; }
        }

        [EmailVerified]
        [HttpPost]
        [Route("GetVideoInfo")]
        public async Task<string> GetVideoInfo([FromBody] GetVideoinfoRequest model)
        {
            var result = new DataResult<GetVideoinfoResponse>() { data = new GetVideoinfoResponse()};
            try
            {
                var sessionInfo = HttpContext.Items["SessionInfo"] as SessionInfo;

                //Ensure this user is getting info for a video that is THERES
                var video = await _database.GetByIdAsync<Video, int>(model.id);
                if (video == null) throw new ObjectDoesNotExistException(nameof(Video), model.id);
                //SECURITY!
                if(!video.user_id.Equals(sessionInfo.user.id)) throw new PermissionDeniedExeption();


                var status = StatusTypes.ALL_STATUS_TYPES.First(x => x.Id == video.status_type_id);
                result.data.status = status.Name;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Video Controller - GetVideoInfo - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }

            return result.CloseResult();
        }

        public class DownloadRequest
        {
            public int id { get; set; }
            public string title { get; set; }
        }

        [EmailVerified]
        [HttpPost]
        [Route("Download")]
        public async Task<string> Download([FromBody] DownloadRequest model)
        {
            var result = new DataResult<string>();
            try
            {
                var sessionInfo = HttpContext.Items["SessionInfo"] as SessionInfo;

                //Ensure this user is getting info for a video that is THERES
                var video = await _database.GetByIdAsync<Video, int>(model.id);
                if (video == null) throw new ObjectDoesNotExistException(nameof(Video), model.id);
                //SECURITY!
                if (!video.user_id.Equals(sessionInfo.user.id)) throw new PermissionDeniedExeption();

                //Download or Send SAS
                result.data = _blobService.GenerateSasTokenForBlob($"Ready/v_{video.id}_/{video.title}.mp4");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Video Controller - Download - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }

            return result.CloseResult();
        }

        public class UploadToYoutubeRequest
        {
            public int id { get; set; }
            public string category { get; set; }
        }
        [EmailVerified]
        [HttpPost]
        [Route("UploadToYoutube")]
        public async Task<string> UploadToYoutube([FromBody] UploadToYoutubeRequest model)
        {
            var result = new DataResult<string>();
            try
            {
                var sessionInfo = HttpContext.Items["SessionInfo"] as SessionInfo;

                //If this is not me (seb.gadzy@gmail.com) give a email alert to me
                if (!sessionInfo.user.email.Equals("Youtube"))
                {
                    await _emailService.SendAlertEmail(new List<EmailAddress>() { new EmailAddress() { Email = _configuration["ContactInfo:Email:Emergency"] } },new Alert() { title= "Unauthorized Access", body=$"{sessionInfo.user.email} attempted to upload youtube video"});
                    throw new PermissionDeniedExeption();
                }

                //Ensure this user is getting info for a video that is THERES
                var video = await _database.GetByIdAsync<Video, int>(model.id);
                if (video == null) throw new ObjectDoesNotExistException(nameof(Video), model.id);

                //Ensure category is created
                var category = await _database.FirstOrDefaultAsync<Category>("title = @title", new { @title = model.category });
                if (category == null)
                {
                    category = await _database.InsertAsync(new Category()
                    {
                        title = model.category,
                        modified_by = sessionInfo.user.email,
                        modified_date = DateTime.UtcNow,
                        created_date = DateTime.UtcNow
                    });
                }

                var videoCategory = await _database.FirstOrDefaultAsync<VideoCategory>("category_id = @categoryId and video_id = @videoId", new { @categoryId = category.id, @videoId = video.id });
                if(videoCategory == null)
                {
                    videoCategory = await _database.InsertAsync(new VideoCategory() { category_id = category.id, video_id = video.id, created_date = DateTime.UtcNow });
                }

                //Download or Send SAS
                video.status_type_id = StatusTypes.UPLOAD_TO_YOUTUBE.Id;
                video.modified_date = DateTime.UtcNow;
                video.modified_by = sessionInfo.user.email;
                await _database.UpdateAsync(video);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Video Controller - Download - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }

            return result.CloseResult();
        }
    }
}
