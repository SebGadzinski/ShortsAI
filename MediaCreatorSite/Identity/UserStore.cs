using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Identity;
using DapperDatabaseUtility.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;
using MediaCreatorSite.DataAccess.Dto;
using MediaCreatorSite.DataAccess;

namespace MediaCreatorSite.Identity
{

    public class UserStore : IUserStore<AppUser>, IUserEmailStore<AppUser>, IUserPhoneNumberStore<AppUser>,
        IUserTwoFactorStore<AppUser>, IUserPasswordStore<AppUser>, IUserRoleStore<AppUser>
    {
        private readonly IMediaCreatorDatabase _database;
        private readonly ISqlUtilityService _sqlUtilityService;
        private const string LAST_MODIFIED_BY = "UserStore";

        public UserStore()
        {
            var connections = new Connections(new Dictionary<string, string> { { "MediaCreatorDB", Program.Configuration.GetConnectionString("mediaCreator") } });
            ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole());
            _database = new MediaCreatorDatabase(connections, loggerFactory.CreateLogger<MediaCreatorDatabase>());
            _sqlUtilityService = new SqlUtilityService();
        }

        public async Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            user = await _database.BlockAsync("CreateAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                var queryString = "";
                return await connection.QueryFirstOrDefaultAsync<AppUser>(_database.InsertString(ref queryString, user));
            });

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _database.BlockAsync("DeleteAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QueryFirstOrDefaultAsync<AppUser>("DELETE FROM [AppUser] WHERE [id] = @id", user);
            });
            return IdentityResult.Success;
        }

        public async Task<AppUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _database.BlockAsync("FindByIdAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QueryFirstOrDefaultAsync<AppUser>($"Select {_sqlUtilityService.GetAllSelectValues<AppUser>()} From AppUser Where id = '{userId}'");
            });
        }

        /// <summary>
        /// In this case we are using "normalized user name as email"
        /// </summary>
        /// <param name="normalizedUserName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AppUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _database.BlockAsync("FindByNameAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QueryFirstOrDefaultAsync<AppUser>($"Select {_sqlUtilityService.GetAllSelectValues<AppUser>()} From AppUser Where email = '{normalizedUserName}'");
            });
        }

        public Task<string> GetNormalizedUserNameAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.user_name.ToUpper());
        }

        public Task<string> GetUserIdAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.id.ToString());
        }

        public Task<string> GetUserNameAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.user_name);
        }

        public Task SetNormalizedUserNameAsync(AppUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.user_name = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(AppUser user, string userName, CancellationToken cancellationToken)
        {
            user.user_name = userName;
            return Task.FromResult(0);
        }

        public async Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _database.BlockAsync("UpdateAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                var queryString = "";
                return await connection.QueryFirstOrDefaultAsync<AppUser>(_database.UpdateString(ref queryString, user));
            });

            return IdentityResult.Success;
        }

        public Task SetEmailAsync(AppUser user, string email, CancellationToken cancellationToken)
        {
            user.email = email.ToUpper();
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.email);
        }

        public Task<bool> GetEmailConfirmedAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.email_confirmed);
        }

        public Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.email_confirmed = confirmed;
            return Task.FromResult(0);
        }

        public async Task<AppUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _database.BlockAsync("FindByEmailAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QueryFirstOrDefaultAsync<AppUser>($"Select {_sqlUtilityService.GetAllSelectValues<AppUser>()} From AppUser Where email = '{normalizedEmail}'");
            });
        }

        public Task<string> GetNormalizedEmailAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.email.ToUpper());
        }

        public Task SetNormalizedEmailAsync(AppUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.email = normalizedEmail;
            return Task.FromResult(0);
        }

        public Task SetPhoneNumberAsync(AppUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.phone_number = phoneNumber;
            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.phone_number);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.phone_number_confirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.phone_number_confirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(AppUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.two_factor_enabled = enabled;
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.two_factor_enabled);
        }

        public Task SetPasswordHashAsync(AppUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.password = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.password);
        }

        public Task<bool> HasPasswordAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.password != null);
        }

        public async Task AddToRoleAsync(AppUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _database.BlockAsync("AddToRoleAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                var normalizedName = roleName.ToLower();
                var roleId = await connection.ExecuteScalarAsync<Guid?>($"SELECT [Id] FROM [AppRole] WHERE [name] = '{normalizedName}'");
                if (!roleId.HasValue)
                    throw new Exception($"Role does not exist: {roleName}");

                try
                {
                    var queryString = "";
                    await connection.ExecuteAsync(_database.InsertString(ref queryString, new AppUserRole()
                    {
                        role_id = roleId.Value,
                        user_id = user.id,
                        created_date = DateTime.UtcNow,
                        modified_date = DateTime.UtcNow,
                        modified_by = LAST_MODIFIED_BY
                    }));
                }
                catch (Exception ex) {}
            });
        }

        public async Task RemoveFromRoleAsync(AppUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _database.BlockAsync("RemoveFromRoleAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);

                var roleId = await connection.ExecuteScalarAsync<Guid?>("SELECT [id] FROM [AppRole] WHERE [name] = @normalizedName", new { normalizedName = roleName.ToLower() });
                if (!roleId.HasValue)
                    await connection.ExecuteAsync($"DELETE FROM [AppUserRole] WHERE [user_id] = @userId AND [role_id] = @roleId", new { userId = user.id, roleId });
            });
        }

        public async Task<IList<string>> GetRolesAsync(AppUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _database.BlockAsync("GetRolesAsync", async (connection) =>
            {
                await connection.OpenAsync(cancellationToken);
                var queryResults = await connection.QueryAsync<string>("SELECT r.[name] FROM [AppRole] r INNER JOIN [AppUserRole] ur ON ur.[role_id] = r.id " +
                    "WHERE ur.user_id = @userId", new { @userId = user.id });

                return queryResults.ToList();
            });
        }

        public async Task<bool> IsInRoleAsync(AppUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _database.BlockAsync("IsInRoleAsync", async (connection) =>
            {
                var roleId = await connection.ExecuteScalarAsync<Guid?>("SELECT [id] FROM [AppRole] WHERE [name] = @normalizedName", new { normalizedName = roleName.ToLower() });
                if (roleId == default(Guid)) return false;
                var matchingRoles = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM [AppUserRole] WHERE [user_id] = @userId AND [role_id] = @roleId",
                    new { userId = user.id, roleId });

                return matchingRoles > 0;
            });
        }

        public async Task<IList<AppUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _database.BlockAsync("GetUsersInRoleAsync", async (connection) =>
            {
                var queryResults = await connection.QueryAsync<AppUser>("SELECT u.* FROM [AppUser] u " +
                    "INNER JOIN [AppUserRole] ur ON ur.[user_id] = u.[id] INNER JOIN [AppRole] r ON r.[id] = ur.[role_id] WHERE r.[name] = @normalizedName",
                    new { normalizedName = roleName.ToLower() });

                return queryResults.ToList();
            });
        }

        public void Dispose()
        {
            // Nothing to dispose.
        }
    }
}
