using Google.Apis.Logging;
using MediaCreatorFunctions.DataAccess;
using MediaCreatorFunctions.DataAccess.Constants;
using MediaCreatorFunctions.DataAccess.DTO;
using MediaCreatorFunctions.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.Functions
{
    public class DeleteOldVideosFunction
    {
        private readonly IMediaCreatorDatabase _database;
        private readonly IBlobService _blobService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DeleteOldVideosFunction> _logger;

        private const int CUT_OFF_HOURS = 1;

        public DeleteOldVideosFunction(IMediaCreatorDatabase database, IBlobService blobService, IConfiguration configuration, ILogger<DeleteOldVideosFunction> logger)
        {
            _database = database;
            _blobService = blobService;
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName("DeleteOldVideos")]
        public async Task DeleteOldVideos([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
        //public async Task DeleteOldVideos([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
        {
            try
            {
                //Get all videos that are completed and are past a hour old
                var cutOffDate = DateTime.UtcNow.AddHours(-CUT_OFF_HOURS);
                var oldVideos = await _database.WhereAsync<Video>("status_type_id = @completedStatus and created_date < @cutOffDate", new { @completedStatus = StatusTypes.COMPLETE.Id, cutOffDate });

                //Remove all videos and files from blob storage attached to video
                foreach(var oldVideo in oldVideos)
                {
                    await _blobService.DeleteContent($"Ready/v_{oldVideo.id}_");
                    await _blobService.DeleteFile($"Create/v_{oldVideo.id}_.txt");
                }

                //Update them to deleted in the database
                foreach(var video in oldVideos) { video.status_type_id = StatusTypes.DELETED.Id; video.modified_date = DateTime.UtcNow; video.modified_by = nameof(DeleteOldVideos); }
                await _database.BulkUpdateAsync(oldVideos);
            }
            catch(Exception ex)
            {
                _logger.LogError($"{nameof(DeleteOldVideos)} - Error - {JsonConvert.SerializeObject(ex)}");
            }
        }
    }
}
