using MediaCreatorFunctions.DataAccess;
using MediaCreatorFunctions.DataAccess.Constants;
using MediaCreatorFunctions.DataAccess.DTO;
using MediaCreatorFunctions.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(MediaCreator.StartUp))]
namespace MediaCreator
{
    public class StartUp : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
            });

            var settingsLocation =
#if !DEBUG
                           // set this to whatever the live setting file will be
                        "appsettings.json";
#else
            "local.settings.json";
#endif

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(settingsLocation, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            //Google
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", configuration["Google:JsonKeyPath"]);

            //Database
            var connections = new Connections(new Dictionary<string, string> { { "MediaCreatorDB", configuration["MediaCreatorDB"] } });
            ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole());
            IMediaCreatorDatabase mediaPlayerDatabase = new MediaCreatorDatabase(connections, loggerFactory.CreateLogger<MediaCreatorDatabase>());
            builder.Services.AddSingleton(mediaPlayerDatabase);

            //Services
            builder.Services.AddScoped<IChatGPTService, ChatGPTService>();
            builder.Services.AddScoped<IMediaService, MediaService>();
            builder.Services.AddScoped<IAudioService, AudioService>();
            builder.Services.AddScoped<ICostService, CostService>();
            builder.Services.AddScoped<IDeepAIService, DeepAIService>();
            builder.Services.AddScoped<IVideoService, VideoService>();
            builder.Services.AddScoped<IBlobService, BlobService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IYoutubeService, YoutubeService>();
            builder.Services.AddScoped<ISeleniumService, SeleniumService>();

            builder.Services.AddSingleton(configuration);
        }
    }
}
