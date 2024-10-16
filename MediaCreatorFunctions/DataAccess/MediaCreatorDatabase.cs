using DapperDatabaseUtility.DataAccess;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MediaCreatorFunctions.DataAccess.DTO;

namespace MediaCreatorFunctions.DataAccess
{
    public interface IMediaCreatorDatabase : IBaseDatabaseHandler {
        public Task<Category> GetVideoCategory(int videoId);
    }
    public class MediaCreatorDatabase : BaseDatabaseHandler, IMediaCreatorDatabase
    {
        public MediaCreatorDatabase(IDapperSqlConnectionFactory connectionFactory, ILogger<BaseDatabaseHandler> logger) : base(connectionFactory, "MediaCreatorDB", logger)
        {
        }

        private readonly string GET_VIDEO_CATEGORY =
            $@"Select {_sqlUtilityService.GetAllSelectValues<Category>("c")}
            from Category c
            Inner join VideoCategory vc on vc.category_id = c.id
            inner join Video v on vc.video_id = v.id
            where v.id = @videoId";
        public async Task<Category> GetVideoCategory(int videoId)
        {
            return await BlockAsync("MediaCreatorDatabase.GetScriptTags", async (connection) => {
                return await connection.QueryFirstOrDefaultAsync<Category>(GET_VIDEO_CATEGORY, new { videoId });
            });
        }
    }
}
