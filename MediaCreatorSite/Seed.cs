using Dapper;
using MediaCreatorSite.DataAccess;
using MediaCreatorSite.DataAccess.Constants;
using MediaCreatorSite.DataAccess.Dto;
using MediaCreatorSite.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;

namespace MediaCreatorSite
{
    public class Seed
    {

        public const string CLEAN_DATABASE = @"
            /* TRUNCATE ALL TABLES IN A DATABASE */
            DECLARE @dropAndCreateConstraintsTable TABLE
                    (
                        DropStmt VARCHAR(MAX)
                    ,CreateStmt VARCHAR(MAX)
                    )
            /* Gather information to drop and then recreate the current foreign key constraints  */
            INSERT  @dropAndCreateConstraintsTable
            SELECT  DropStmt = 'ALTER TABLE [' + ForeignKeys.ForeignTableSchema
                    + '].[' + ForeignKeys.ForeignTableName + '] DROP CONSTRAINT ['
                    + ForeignKeys.ForeignKeyName + ']; '
                    ,CreateStmt = 'ALTER TABLE [' + ForeignKeys.ForeignTableSchema
                    + '].[' + ForeignKeys.ForeignTableName
                    + '] WITH CHECK ADD CONSTRAINT [' + ForeignKeys.ForeignKeyName
                    + '] FOREIGN KEY([' + ForeignKeys.ForeignTableColumn
                    + ']) REFERENCES [' + SCHEMA_NAME(sys.objects.schema_id)
                    + '].[' + sys.objects.[name] + ']([' + sys.columns.[name]
                    + ']); '
            FROM    sys.objects
            INNER JOIN sys.columns
                    ON ( sys.columns.[object_id] = sys.objects.[object_id] )
            INNER JOIN ( SELECT sys.foreign_keys.[name] AS ForeignKeyName
                                ,SCHEMA_NAME(sys.objects.schema_id) AS ForeignTableSchema
                                ,sys.objects.[name] AS ForeignTableName
                                ,sys.columns.[name] AS ForeignTableColumn
                                ,sys.foreign_keys.referenced_object_id AS referenced_object_id
                                ,sys.foreign_key_columns.referenced_column_id AS referenced_column_id
                            FROM   sys.foreign_keys
                            INNER JOIN sys.foreign_key_columns
                                ON ( sys.foreign_key_columns.constraint_object_id = sys.foreign_keys.[object_id] )
                            INNER JOIN sys.objects
                                ON ( sys.objects.[object_id] = sys.foreign_keys.parent_object_id )
                            INNER JOIN sys.columns
                                ON ( sys.columns.[object_id] = sys.objects.[object_id] )
                                    AND ( sys.columns.column_id = sys.foreign_key_columns.parent_column_id )
                        ) ForeignKeys
                    ON ( ForeignKeys.referenced_object_id = sys.objects.[object_id] )
                        AND ( ForeignKeys.referenced_column_id = sys.columns.column_id )
            WHERE   ( sys.objects.[type] = 'U' )
                    AND ( sys.objects.[name] NOT IN ( 'sysdiagrams' ) )
            /* SELECT * FROM @dropAndCreateConstraintsTable AS DACCT  --Test statement*/
            DECLARE @DropStatement NVARCHAR(MAX)
            DECLARE @RecreateStatement NVARCHAR(MAX)
            /* Drop Constraints */
            DECLARE Cur1 CURSOR READ_ONLY
            FOR
                    SELECT  DropStmt
                    FROM    @dropAndCreateConstraintsTable
            OPEN Cur1
            FETCH NEXT FROM Cur1 INTO @DropStatement
            WHILE @@FETCH_STATUS = 0
                    BEGIN
                        PRINT 'Executing ' + @DropStatement
                        EXECUTE sp_executesql @DropStatement
                        FETCH NEXT FROM Cur1 INTO @DropStatement
                    END
            CLOSE Cur1
            DEALLOCATE Cur1
            /* Truncate all tables in the database in the dbo schema */
            DECLARE @DeleteTableStatement NVARCHAR(MAX)
            DECLARE Cur2 CURSOR READ_ONLY
            FOR
                    SELECT  'TRUNCATE TABLE [dbo].[' + TABLE_NAME + ']'
                    FROM    INFORMATION_SCHEMA.TABLES
                    WHERE   TABLE_SCHEMA = 'dbo'
                            AND TABLE_TYPE = 'BASE TABLE'
                /* Change your schema appropriately if you don't want to use dbo */
            OPEN Cur2
            FETCH NEXT FROM Cur2 INTO @DeleteTableStatement
            WHILE @@FETCH_STATUS = 0
                    BEGIN
                        PRINT 'Executing ' + @DeleteTableStatement
                        EXECUTE sp_executesql @DeleteTableStatement
                        FETCH NEXT FROM Cur2 INTO @DeleteTableStatement
                    END
            CLOSE Cur2
            DEALLOCATE Cur2
            /* Recreate foreign key constraints  */
            DECLARE Cur3 CURSOR READ_ONLY
            FOR
                    SELECT  CreateStmt
                    FROM    @dropAndCreateConstraintsTable
            OPEN Cur3
            FETCH NEXT FROM Cur3 INTO @RecreateStatement
            WHILE @@FETCH_STATUS = 0
                    BEGIN
                        PRINT 'Executing ' + @RecreateStatement
                        EXECUTE sp_executesql @RecreateStatement
                        FETCH NEXT FROM Cur3 INTO @RecreateStatement
                    END
            CLOSE Cur3
            DEALLOCATE Cur3  ";

        public static async Task SeedAsync(IMediaCreatorDatabase database, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            try
            {
                database.Connection().Execute(CLEAN_DATABASE);
                await SeedRolesAsync(database, roleManager);
                await SeedClaimsAsync(database, userManager);
                await SeedUsersAsync(database, userManager, false);
                await SeedStoresAsync(database);
                await SeedStatusTypesAsync(database);
                await SeedVoicesAsync(database);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task SeedRolesAsync(IMediaCreatorDatabase database, RoleManager<AppRole> roleManager)
        {
            var appRoles = new List<string>() { "admin", "shopper" };
            foreach (var role in appRoles)
            {
                await database.InsertAsync(new AppRole()
                {
                    name = role,
                    created_date = DateTime.UtcNow,
                    modified_date = DateTime.UtcNow,
                    modified_by = "Seeding"
                });
            }
        }

        public static async Task SeedClaimsAsync(IMediaCreatorDatabase database, UserManager<AppUser> userManager)
        {
            var appClaimsNames = new List<string>() { "ADDRESS", "FIRST_NAME", "BILLING-ADDRESS", "LAST_NAME" };
            foreach (var claim in appClaimsNames)
            {
                await database.InsertAsync(new AppClaim()
                {
                    name = claim,
                    created_date = DateTime.UtcNow,
                    modified_date = DateTime.UtcNow,
                    modified_by = "Seeding"
                });
            }
        }

        public static async Task SeedVoicesAsync(IMediaCreatorDatabase database)
        {
            var voices = new List<string>() { "Male", "Female"};
            foreach (var voice in voices)
            {
                await database.InsertAsync(new AppClaim()
                {
                    name = voice,
                    created_date = DateTime.UtcNow,
                    modified_date = DateTime.UtcNow,
                    modified_by = "Seeding"
                });
            }
        }

        public static async Task SeedUsersAsync(IMediaCreatorDatabase database, UserManager<AppUser> userManager, bool keepOldUsers)
        {
            var myUserEmail = Program.Configuration["TestUser"];
            if (!keepOldUsers)
            {
                await database.DeleteWhereAsync("1 = 1", "AppUserClaim");
                await database.DeleteWhereAsync("1 = 1", "AppUserRole");
                await database.DeleteWhereAsync("1 = 1", "AppUserSession");
                await database.DeleteWhereAsync("1 = 1", "AppUserToken");
                //Anything else attached to user
                await database.DeleteWhereAsync("1 = 1", "AppUser");
            }
            var shopper = new KeyValuePair<string, AppUser>(Roles.SHOPPER, new AppUser()
            {
                email = "shopper@gmail.com".ToUpper(),
                user_name = "shopper@gmail.com".ToUpper(),
                email_confirmed = true,
                created_date = DateTime.UtcNow,
                modified_date = DateTime.UtcNow,
                modified_by = "Seeding"
            });
            shopper.Value.password = userManager.PasswordHasher.HashPassword(shopper.Value, "UserPassword!");
            var sebastian = new KeyValuePair<string, AppUser>(Roles.SHOPPER, new AppUser()
            {
                email = myUserEmail.ToUpper(),
                user_name = myUserEmail.ToUpper(),
                email_confirmed = true,
                created_date = DateTime.UtcNow,
                modified_date = DateTime.UtcNow,
                modified_by = "Seeding"
            });
            sebastian.Value.password = userManager.PasswordHasher.HashPassword(shopper.Value, "UserPassword!");
            var admin = new KeyValuePair<string, AppUser>(Roles.ADMIN, new AppUser()
            {
                email = "admin@gmail.com".ToUpper(),
                user_name = "admin@gmail.com".ToUpper(),
                email_confirmed = true,
                created_date = DateTime.UtcNow,
                modified_date = DateTime.UtcNow,
                modified_by = "Seeding"
            });
            admin.Value.password = userManager.PasswordHasher.HashPassword(admin.Value, "UserPassword!");

            var users = (await database.WhereAsync<AppUser>($"email like '{myUserEmail}' or email like 'shopper@gmail.com' or email like 'admin@gmail.com'")).ToDictionary(x => x.email);
            foreach (var userWithRole in new List<KeyValuePair<string, AppUser>>() { shopper, sebastian, admin })
            {
                var user = userWithRole.Value;
                if (!keepOldUsers || !users.ContainsKey(user.email))
                {
                    user = await database.InsertAsync(user);
                    users.Add(user.email, user);
                    await userManager.AddToRoleAsync(user, userWithRole.Key);
                }
            }
        }

        public static async Task SeedStoresAsync(IMediaCreatorDatabase database)
        {
            foreach (var store in Stores.ALL_STORES)
            {
                await database.InsertAsync(new Store()
                {
                    name = store.Name,
                    website = store.Website,
                    created_date = DateTime.UtcNow
                });
            }
        }

        public static async Task SeedStatusTypesAsync(IMediaCreatorDatabase database)
        {
            foreach (var statusType in StatusTypes.ALL_STATUS_TYPES)
            {
                await database.InsertAsync(new StatusType()
                {
                    name = statusType.Name,
                    created_date = DateTime.UtcNow
                });
            }
        }
    }
}
