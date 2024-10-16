using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Newtonsoft.Json;
using System.IO.Compression;
using MediaCreatorSite.DataAccess;
using MediaCreatorSite.Services;
using MediaCreatorSite.DataAccess.Dto;
using MediaCreatorSite.Identity;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;

namespace MediaCreatorSite
{
    public class StartUp
    {
        public StartUp(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                //Google
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Configuration["Google:JsonKeyPath"]);

                //DATABASE
                var connections = new Connections(new Dictionary<string, string> { { "MediaCreatorDB", Configuration.GetConnectionString("mediaCreator") } });
                ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole());
                IMediaCreatorDatabase database = new MediaCreatorDatabase(connections, loggerFactory.CreateLogger<MediaCreatorDatabase>());

                //Application
                services.AddControllersWithViews();

                services.AddControllers().AddNewtonsoftJson(options =>
                {
                    // Use the default property (Pascal) casing
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.SetMinimumLevel(LogLevel.Information);
                });

                //Helpers (for getting access to the HttpContext, UrlHelper, ActionContext inside any code that is dependent on these things)
                services.AddHttpContextAccessor();
                services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
                services.AddScoped<IUrlHelper>(provider =>
                {
                    var actionContext = provider.GetService<IActionContextAccessor>().ActionContext;
                    var urlHelperFactory = provider.GetRequiredService<IUrlHelperFactory>();
                    return urlHelperFactory.GetUrlHelper(actionContext);
                });

                //Database
                services.AddSingleton(database);

                //Services
                services.AddScoped<IEmailService, EmailService>();
                services.AddScoped<IStripeService, StripeService>();
                services.AddScoped<IBlobService, BlobService>();

                services.AddTransient<IUserStore<AppUser>, UserStore>();
                services.AddTransient<IRoleStore<AppRole>, RoleStore>();

                services.AddIdentity<AppUser, AppRole>()
                    .AddDefaultTokenProviders();

                services.Configure<BrotliCompressionProviderOptions>(options =>
                {
                    options.Level = CompressionLevel.Fastest;
                });

                services.AddDistributedMemoryCache();

                services.AddSession(options =>
                {
                    options.Cookie.Name = ".MediaCreatorSite.Session";
                    options.IdleTimeout = TimeSpan.FromDays(1);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

                services.AddResponseCompression(options =>
                {
                    options.EnableForHttps = true;
                    options.Providers.Add<BrotliCompressionProvider>();
                });

                services.AddSpaStaticFiles(configuration =>
                {
                    configuration.RootPath = "ClientApp/build";
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartUp.cs => ConfigureServices - {JsonConvert.SerializeObject(ex)}");
            }
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<StartUp> logger)
        {
            try
            {
                // Configure the HTTP request pipeline.
                if (!env.IsDevelopment() && !env.EnvironmentName.Equals("Testing"))
                {
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }
                app.UseAuthentication();
                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRouting();
                app.UseSession();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller}/{action=Index}/{id?}").RequireAuthorization();
                    endpoints.MapFallbackToFile("index.html");
                });

                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";

                    if (env.IsDevelopment())
                    {
                        spa.UseReactDevelopmentServer(npmScript: "start");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartUp.cs => Configure - {JsonConvert.SerializeObject(ex)}");
            }
        }
    }
}
