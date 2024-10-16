using MediaCreatorSite;
using MediaCreatorSite.DataAccess;
using MediaCreatorSite.DataAccess.Dto;
using MediaCreatorSite.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SendGrid.Helpers.Mail;

public class Program
{
    public static IConfiguration Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Connection: " + Configuration.GetConnectionString("mediaCreator"));
        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Console.WriteLine(env);

        var host = CreateHostBuilder(args).Build();
        //DATABASE
        var connections = new Connections(new Dictionary<string, string> { { "MediaCreatorDB", Configuration.GetConnectionString("mediaCreator") } });
        ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
        .SetMinimumLevel(LogLevel.Trace)
        .AddConsole());
        IMediaCreatorDatabase database = new MediaCreatorDatabase(connections, loggerFactory.CreateLogger<MediaCreatorDatabase>());

        //IDENTITY
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = loggerFactory.CreateLogger("app");
            try
            {
                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
                //await Seed.SeedAsync(database, userManager, roleManager);
                logger.LogInformation("Finished Seeding Default Data");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "An error occurred seeding the DB");
            }
        }
        
        host.Run();
        Console.WriteLine("Application Starting");
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<StartUp>();
            });

}