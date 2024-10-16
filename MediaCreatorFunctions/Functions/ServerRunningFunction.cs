using MediaCreatorFunctions.DataAccess;
using MediaCreatorFunctions.DataAccess.Constants;
using MediaCreatorFunctions.DataAccess.DTO;
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
    public class ServerRunningFunction
    {
        private readonly IMediaCreatorDatabase _database;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServerRunningFunction> _logger;

        public ServerRunningFunction(IMediaCreatorDatabase database, IConfiguration configuration, ILogger<ServerRunningFunction> logger)
        {
            _database = database;
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName("ServerRunning")]
        public async Task ServerRunning([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        //public async Task DeleteOldVideos([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
        {
            try
            {
                //update the datetime of when this function ran
                var config = await _database.FirstOrDefaultAsync<Config>("name = @serverRunning", new { @serverRunning = Configs.SERVER_RUNNING });
                if (config == null) config = await _database.InsertAsync(new Config()
                {
                    name = Configs.SERVER_RUNNING,
                    value = DateTime.UtcNow.ToString(),
                    created_date = DateTime.UtcNow,
                    modified_date = DateTime.UtcNow,
                });
                else
                {
                    config.value = DateTime.UtcNow.ToString();
                    config.modified_date = DateTime.UtcNow;
                    await _database.UpdateAsync(config);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ServerRunning)} - Error - {JsonConvert.SerializeObject(ex)}");
            }
        }
    }
}
