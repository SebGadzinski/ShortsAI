using DapperDatabaseUtility.DataAccess;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MediaCreatorSite.DataAccess.QueryModels;
using MediaCreatorSite.DataAccess.Constants;

namespace MediaCreatorSite.DataAccess
{
    public interface IMediaCreatorDatabase : IBaseDatabaseHandler {
        Task<IEnumerable<ScriptPicture>> GetScriptPictures(int id);
        Task<IEnumerable<ScriptTag>> GetScriptTags(int id);
        Task<IEnumerable<ScriptAudio>> GetScriptAudios(int id);
        IEnumerable<SingleRole> GetUserRoles(Guid userId);
        Task<IEnumerable<SingleRole>> GetUserRolesAsync(Guid userId);
        IEnumerable<SingleClaim> GetUserClaims(Guid userId);
        Task<IEnumerable<SingleClaim>> GetUserClaimsAsync(Guid userId);
        Task<IEnumerable<VideoRow>> GetVideoTableDataAsync(Guid userId);
        Task<IEnumerable<CollectableVideo>> GetCollectableVideosAsync(Guid userId);
    }
    public class MediaCreatorDatabase : BaseDatabaseHandler, IMediaCreatorDatabase
    {
        private readonly List<int> COLLECTABLE_VIDEO_STATUSES;
        private readonly List<int> HISTORICAL_VIDEO_STATUSES;

        public MediaCreatorDatabase(IDapperSqlConnectionFactory connectionFactory, ILogger<BaseDatabaseHandler> logger) : base(connectionFactory, "MediaCreatorDB", logger)
        {
            COLLECTABLE_VIDEO_STATUSES = new List<int>() { StatusTypes.WAITING.Id, StatusTypes.PROCESSING.Id, StatusTypes.COMPLETE.Id };
            HISTORICAL_VIDEO_STATUSES = StatusTypes.ALL_STATUS_TYPES.Where(x => !COLLECTABLE_VIDEO_STATUSES.Any(y => y == x.Id)).Select(x => x.Id).ToList();
        }

        private const string GET_SCRIPT_PICTURES =
            $@"Select sc.script_id, sc.id as 'script_component_id', p.id, p.url
            from ScriptComponent sc
            Inner join ScriptComponentPicture scp on sc.id = scp.script_component_id
            inner join Picture p on scp.picture_id = p.id
            where sc.script_id = @id";
        public async Task<IEnumerable<ScriptPicture>> GetScriptPictures(int id)
        {
            return await BlockAsync("MediaCreatorDatabase.GetScriptPictures", async (connection) => {
                return await connection.QueryAsync<ScriptPicture>(GET_SCRIPT_PICTURES, new { id });
            });
        }

        private const string GET_SCRIPT_TAGS =
            $@"Select sc.script_id, sc.id as 'script_component_id', t.id, t.name
            from ScriptComponent sc
            Inner join ScriptComponentTag sct on sc.id = sct.script_component_id
            inner join Tag t on sct.tag_id = t.id
            where sc.script_id = @id";
        public async Task<IEnumerable<ScriptTag>> GetScriptTags(int id)
        {
            return await BlockAsync("MediaCreatorDatabase.GetScriptTags", async (connection) => {
                return await connection.QueryAsync<ScriptTag>(GET_SCRIPT_TAGS, new { id });
            });
        }

        private string GET_SCRIPT_AUDIO =
            $@"Select sc.script_id, sc.id as 'script_component_id', a.id, a.url
            from ScriptComponent sc
            Inner join ScriptComponentAudio sca on sc.id = sca.script_component_id
            inner join Audio a on sca.audio_id = a.id
            where sc.script_id = @id";
        public async Task<IEnumerable<ScriptAudio>> GetScriptAudios(int id)
        {
            return await BlockAsync("MediaCreatorDatabase.GetScriptData", async (connection) => {
                return await connection.QueryAsync<ScriptAudio>(GET_SCRIPT_AUDIO, new { id });
            });
        }

        private const string GET_USER_ROLES = "Select r.name From [AppRole] r Inner Join AppUserRole ur on r.id = ur.role_id Where ur.user_id = @userId";

        public IEnumerable<SingleRole> GetUserRoles(Guid userId)
        {
            return Block("MediaCreatorDatabase.GetUserRoles", (connection) => connection.Query<SingleRole>(GET_USER_ROLES, new { @userId = userId }));
        }
        public async Task<IEnumerable<SingleRole>> GetUserRolesAsync(Guid userId)
        {
            return await BlockAsync("MediaCreatorDatabase.GetUserRoles", async (connection) => await connection.QueryAsync<SingleRole>(GET_USER_ROLES, new { @userId = userId }));
        }
        private const string GET_USER_CLAIMS = @"SELECT c.[name]
                                                    ,uc.value
                                                  FROM [dbo].[AppClaim] c
                                                  Inner Join AppUserClaim uc on c.id = uc.claim_id
                                                  Where uc.user_id = @userId";
        public IEnumerable<SingleClaim> GetUserClaims(Guid userId)
        {
            return Block("MediaCreatorDatabase.GetUserClaims", (connection) => connection.Query<SingleClaim>(GET_USER_CLAIMS, new { @userId = userId }));
        }
        public async Task<IEnumerable<SingleClaim>> GetUserClaimsAsync(Guid userId)
        {
            return await BlockAsync("MediaCreatorDatabase.GetUserClaimsAsync", async (connection) => await connection.QueryAsync<SingleClaim>(GET_USER_CLAIMS, new { @userId = userId }));
        }

        private const string GET_VIDEO_TABLE_DATA =
            @"SELECT 
                v.id
                ,UPPER(st.[name]) as 'status'
                ,v.[title]
                ,s.name as 'pictureStore'
                ,vo.[name] as 'voice'
                ,v.[width]
                ,v.[height]
                ,v.[created_date] as 'createdOn'
              FROM [MediaCreatorSite].[dbo].[Video] v
              Inner join StatusType st on v.status_type_id = st.id
              inner join Store s on v.picture_store_id = s.id
              inner join Voice vo on v.voice_id = vo.id
              Where v.status_type_id in @statusTypeIds";
        public async Task<IEnumerable<VideoRow>> GetVideoTableDataAsync(Guid userId)
        {
            return await BlockAsync("MediaCreatorDatabase.GetVideoTableDataAsync", async (connection) => await connection.QueryAsync<VideoRow>(GET_VIDEO_TABLE_DATA, new { @userId = userId, @statusTypeIds = HISTORICAL_VIDEO_STATUSES }));
        }

        private const string GET_COLLECTABLE_VIDEOS =
            @"SELECT 
                v.id
                ,v.[title]
                ,st.[name] as 'status'
              FROM [MediaCreatorSite].[dbo].[Video] v
              Inner join StatusType st on v.status_type_id = st.id
              Where v.user_id = @userId and v.status_type_id in @statusTypeIds";
        public async Task<IEnumerable<CollectableVideo>> GetCollectableVideosAsync(Guid userId)
        {
            return await BlockAsync("MediaCreatorDatabase.GetVideoTableDataAsync", async (connection) => await connection.QueryAsync<CollectableVideo>(GET_COLLECTABLE_VIDEOS, new { @userId = userId, @statusTypeIds = COLLECTABLE_VIDEO_STATUSES }));
        }
    }
}
