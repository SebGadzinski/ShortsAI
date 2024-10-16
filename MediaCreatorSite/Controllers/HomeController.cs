using EllipticCurve.Utils;
using Grpc.Core;
using MediaCreatorSite.DataAccess;
using MediaCreatorSite.DataAccess.Constants;
using MediaCreatorSite.DataAccess.DTO;
using MediaCreatorSite.Models;
using MediaCreatorSite.Services;
using MediaCreatorSite.Utility.Attributes;
using MediaCreatorSite.Utility.Exceptions;
using MediaCreatorSite.Utility.Extensions;
using MediaCreatorSite.Utility.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MediaCreatorSite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IMediaCreatorDatabase _database;
        private readonly IBlobService _blobService;
        private readonly ILogger<HomeController> _logger;

        private readonly List<int> VideoInProgressStatusTypeIds = new List<int>() { StatusTypes.WAITING.Id, StatusTypes.PROCESSING.Id};
        private const double VideoCreditCharge = 1.00;

        public HomeController(IMediaCreatorDatabase database, IBlobService blobService, ILogger<HomeController> logger)
        {
            _database = database;
            _blobService = blobService;
            _logger = logger;
        }

        public class CreateVideoModel
        {
            public string title { get; set; }
            public int width { get; set; } = 600;
            public int height { get; set; } = 600;
            public string picture_store { get; set; } = Stores.DEEP_AI.Name;
            public string voice { get; set; } = Voices.MALE.Name;
        }

        [EmailVerified]
        [HttpPost]
        [Route("CreateVideo")]
        public async Task<string> CreateVideo([FromBody] CreateVideoModel model)
        {
            var result = new BaseResult();
            try
            {
                //Verify video creation server is running
                var lastRun = await _database.FirstOrDefaultAsync<Config>("name = @name", new { @name = Configs.SERVER_RUNNING });
                if (lastRun == null || (DateTime.UtcNow - DateTime.Parse(lastRun.value)).TotalMinutes > 1.5) throw new VideoServerIsDownException(lastRun == null ? "Unknown" : lastRun.value + " UTC");
                
                var session = HttpContext.Items["SessionInfo"] as SessionInfo;

                //Ensure the title is ok to use
                VerifyTitleIsOkToSave(model.title);

                //Ensure no Existing videos are waiting or in progress for this person
                var existingProcessingVideo = await _database.FirstOrDefaultAsync<Video>("user_id = @userId and status_type_id in @statusTypeIds", new { @userId = session?.user?.id, @statusTypeIds = VideoInProgressStatusTypeIds });
                if (existingProcessingVideo != null) throw new VideoInProgressException(StatusTypes.ALL_STATUS_TYPES.First(x => x.Id == existingProcessingVideo.status_type_id).Name);

                //Validate user has enough credits again
                var credit = await _database.FirstOrDefaultAsync<Credit>("user_id = @userId", new { @userId = session.user.id });
                if (credit == null) throw new ObjectDoesNotExistException("Credit - user_id", session.user.id);
                if (credit.amount < VideoCreditCharge) throw new NotEnoughCreditsException();

                var video = new Video()
                {
                    user_id = session.user.id,
                    title = model.title,
                    status_type_id = StatusTypes.WAITING.Id,
                    picture_store_id = Stores.ALL_STORES.First(x => x.Name.Equals(model.picture_store)).Id,
                    voice_id = Voices.ALL_VOICES.First(x => x.Name.Equals(model.voice)).Id,
                    height = model.height,
                    width = model.width,
                    created_date = DateTime.UtcNow,
                    modified_date = DateTime.UtcNow,
                    modified_by = this.GetUserEmail()
                };
                video = await _database.InsertAsync(video);
                //Send file to blob storage
                await _blobService.SaveFile(new byte[0], $"Create/v_{video.id}_.txt");
            }
            catch(Exception ex)
            {
                _logger.LogError($"Home Controller - SignUp - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            return result.CloseResult();
        }

        public class ServerRunningResult
        {
            public bool isRunning = false;
            public DateTime lastCheck = DateTime.MinValue;
        }
        [HttpGet]
        [Route("ServerRunning")]
        public async Task<string> ServerRunning()
        {
            var result = new DataResult<ServerRunningResult>() { data = new ServerRunningResult() };
            try
            {
                var lastRun = await _database.FirstOrDefaultAsync<Config>("name = @name", new { @name = Configs.SERVER_RUNNING });
                if(lastRun != null)
                {
                    result.data.lastCheck = DateTime.Parse(lastRun.value);
                    result.data.isRunning = (DateTime.UtcNow - result.data.lastCheck).TotalMinutes < 1.5;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Home Controller - SignUp - Error: {JsonConvert.SerializeObject(ex)}");
                result.exception = ex;
            }
            return result.CloseResult();
        }

        /// <summary>
        /// Verifies that the title is ok to save to a file system
        /// </summary>
        /// <param name="title">The title to verify</param>
        private void VerifyTitleIsOkToSave(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title cannot be null or empty.");
            }

            // Check for any invalid characters in the title
            var invalidCharacters = Path.GetInvalidFileNameChars();
            if (title.IndexOfAny(invalidCharacters) != -1)
            {
                throw new ArgumentException("Title contains invalid characters.");
            }
        }

    }
}
