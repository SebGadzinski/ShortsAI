using DapperDatabaseUtility.Services;
using Microsoft.AspNetCore.Identity;
using System.Data.SqlClient;
using Dapper;
using MediaCreatorSite.DataAccess;
using MediaCreatorSite.DataAccess.Dto;

namespace MediaCreatorSite.Identity
{
    public class RoleStore : IRoleStore<AppRole>
    {
        private readonly IMediaCreatorDatabase _database;
        private readonly ISqlUtilityService _sqlUtilityService;

        public RoleStore()
        {
            var connections = new Connections(new Dictionary<string, string> { { "MediaCreatorDB", Program.Configuration.GetConnectionString("mediaCreator") } });
            ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole());
            _database = new MediaCreatorDatabase(connections, loggerFactory.CreateLogger<MediaCreatorDatabase>());
            _sqlUtilityService = new SqlUtilityService();
        }

        public async Task<IdentityResult> CreateAsync(AppRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _database.BlockAsync("CreateAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                var queryString = "";
                role = await connection.QueryFirstOrDefaultAsync<AppRole>(_database.InsertString(ref queryString, role));
            });

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(AppRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _database.BlockAsync("UpdateAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                var queryString = "";
                return await connection.QueryFirstOrDefaultAsync<AppRole>(_database.UpdateString(ref queryString, role));
            });

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(AppRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _database.BlockAsync("DeleteAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QueryFirstOrDefaultAsync<AppUser>("DELETE FROM [AppRole] WHERE [id] = @id", role);
            });

            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.id.ToString());
        }

        public Task<string> GetRoleNameAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.name);
        }

        public Task SetRoleNameAsync(AppRole role, string roleName, CancellationToken cancellationToken)
        {
            role.name = roleName;
            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedRoleNameAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.name.ToLower());
        }

        public Task SetNormalizedRoleNameAsync(AppRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.name = normalizedName.ToLower();
            return Task.FromResult(0);
        }

        public async Task<AppRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _database.BlockAsync("FindByIdAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QueryFirstOrDefaultAsync<AppRole>($"Select {_sqlUtilityService.GetAllSelectValues<AppRole>()} From AppRole Where id = '{roleId}'");
            });
        }

        public async Task<AppRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _database.BlockAsync("FindByNameAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QueryFirstOrDefaultAsync<AppRole>($"Select {_sqlUtilityService.GetAllSelectValues<AppRole>()} From AppRole Where name = '{normalizedRoleName}'");
            });
        }

        public void Dispose()
        {
            // Nothing to dispose.
        }
    }
}
