using MediaCreatorFunctions.DataAccess;
using MediaCreatorFunctions.DataAccess.Constants;
using MediaCreatorFunctions.DataAccess.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.Services
{
    public interface IYoutubeService
    {
        Task UploadVideo(string videoPath, string playlist, string description, Video video);
        List<string> GetCategories();
        string GetDescription();
    }
    public class YoutubeService : IYoutubeService
    {
        private IMediaCreatorDatabase _database;
        private ISeleniumService _seleniumService;
        private IConfiguration _configuration;
        private ILogger<YoutubeService> _logger;

        private readonly List<string> CATEGORIES = new List<string>() { "Fight", "Top 5", "How To", "Comedy" };

        private ChromeDriver driver;

        public YoutubeService(IMediaCreatorDatabase database, ISeleniumService seleniumService, IConfiguration configuration, ILogger<YoutubeService> logger)
        {
            _database = database;
            _seleniumService = seleniumService;
            _configuration = configuration;
            _logger = logger;
        }

        public Task UploadVideo(string videoPath, string playlist, string description, Video video)
        {
            driver = _seleniumService.GetChromeDriver();

            try
            {
                video.status_type_id = StatusTypes.UPLOADING_TO_YOUTUBE.Id;
                video.modified_date = DateTime.UtcNow;
                _database.Update(video);

                driver.Navigate().GoToUrl("https://www.youtube.com");
                Thread.Sleep(2000);

                #region Login
                _seleniumService.Click(driver, By.CssSelector("a[aria-label='Sign in'"));
                _seleniumService.ClearType(driver, By.CssSelector("input[type='email'"), _configuration["Youtube:Email"]);
                _seleniumService.Click(driver, By.XPath("//*[@id=\"identifierNext\"]/div/button"));
                Thread.Sleep(5000);
                _seleniumService.ClearType(driver, By.CssSelector("input[type='password'"), _configuration["Youtube:Password"]);
                _seleniumService.Click(driver, By.XPath("//*[@id=\"passwordNext\"]/div/button"));
                #endregion

                #region Upload
                try
                {
                    driver.Navigate().GoToUrl("https://www.youtube.com/upload");
                    Thread.Sleep(2000);
                    var fileInput = _seleniumService.Get(driver, By.CssSelector("input[type='file'"));
                    fileInput.SendKeys(videoPath);
                    Thread.Sleep(2000);
                }
                catch(Exception ex)
                {
                    var config = _database.FirstOrDefault<Config>($"name = '{Configs.YOUTUBE_HALT}'");
                    var waitTill = DateTime.UtcNow.AddDays(1).ToString();
                    if (config == null)
                    {
                        config = _database.Insert(new Config() { name = Configs.YOUTUBE_HALT, value = waitTill, created_date = DateTime.UtcNow, modified_date = DateTime.UtcNow});
                    }
                    else
                    {
                        config.value = waitTill;
                        config.modified_date = DateTime.UtcNow;
                        _database.Update(config);
                    }
                    throw;
                }
                

                #region Details
                //Title
                _seleniumService.ClearType(driver, By.XPath("/html/body/ytcp-uploads-dialog/tp-yt-paper-dialog/div/ytcp-animatable[1]/ytcp-ve/ytcp-video-metadata-editor/div/ytcp-video-metadata-editor-basics/div[1]/ytcp-video-title/ytcp-social-suggestions-textbox/ytcp-form-input-container/div[1]/div[2]/div/ytcp-social-suggestion-input/div"),
                    $"{video.title}");
                Thread.Sleep(2000);
                //Description
                _seleniumService.ClearType(driver, By.XPath("/html/body/ytcp-uploads-dialog/tp-yt-paper-dialog/div/ytcp-animatable[1]/ytcp-ve/ytcp-video-metadata-editor/div/ytcp-video-metadata-editor-basics/div[2]/ytcp-video-description/div/ytcp-social-suggestions-textbox/ytcp-form-input-container/div[1]/div[2]/div/ytcp-social-suggestion-input/div"),
                    description);

                //Selecting Playlist
                _seleniumService.Click(driver, By.XPath("//*[@id=\"basics\"]/div[4]/div[3]/div[1]/ytcp-video-metadata-playlists"));
                //Wait for options to pop up
                Thread.Sleep(7000);

                var element = _seleniumService.Click(driver, By.XPath($"//label[.//span[@class='label label-text style-scope ytcp-checkbox-group' and text()='{playlist}']]"));
                //Did not have playlist
                if (element == null)
                {
                    _seleniumService.Click(driver, By.XPath($"/html/body/ytcp-playlist-dialog/tp-yt-paper-dialog/div[2]/div/ytcp-button"));
                    _seleniumService.Click(driver, By.XPath($"/html/body/ytcp-playlist-dialog/tp-yt-paper-dialog/div[2]/div/ytcp-text-menu/tp-yt-paper-dialog/tp-yt-paper-listbox/tp-yt-paper-item[1]/ytcp-ve/tp-yt-paper-item-body/div"));
                    _seleniumService.ClearType(driver, By.XPath($"/html/body/ytcp-playlist-creation-dialog/ytcp-dialog/tp-yt-paper-dialog/div[2]/div/ytcp-playlist-metadata-editor/div/div[1]/ytcp-social-suggestions-textbox/ytcp-form-input-container/div[1]/div[2]/div/ytcp-social-suggestion-input/div"), playlist);
                    _seleniumService.Click(driver, By.XPath($"/html/body/ytcp-playlist-creation-dialog/ytcp-dialog/tp-yt-paper-dialog/div[3]/div/ytcp-button[2]"));
                    Thread.Sleep(2000);
                }
                
                _seleniumService.Click(driver, By.XPath("/html/body/ytcp-playlist-dialog/tp-yt-paper-dialog/div[2]/ytcp-button[2]"));
                Thread.Sleep(2000);
                _seleniumService.Click(driver, By.XPath("/html/body/ytcp-uploads-dialog/tp-yt-paper-dialog/div/ytcp-animatable[1]/ytcp-ve/ytcp-video-metadata-editor/div/ytcp-video-metadata-editor-basics/div[5]/ytkc-made-for-kids-select/div[4]/tp-yt-paper-radio-group/tp-yt-paper-radio-button[2]/div[1]"));
                Thread.Sleep(2000);
                //Next
                _seleniumService.Click(driver, By.XPath("/html/body/ytcp-uploads-dialog/tp-yt-paper-dialog/div/ytcp-animatable[2]/div/div[2]/ytcp-button[2]"));
                Thread.Sleep(2000);
                #endregion

                #region Video Elements
                //Next
                _seleniumService.Click(driver, By.XPath("/html/body/ytcp-uploads-dialog/tp-yt-paper-dialog/div/ytcp-animatable[2]/div/div[2]/ytcp-button[2]"));
                Thread.Sleep(2000);
                #endregion

                #region Checks
                //Next
                _seleniumService.Click(driver, By.XPath("/html/body/ytcp-uploads-dialog/tp-yt-paper-dialog/div/ytcp-animatable[2]/div/div[2]/ytcp-button[2]"));
                Thread.Sleep(2000);
                #endregion

                #region Visibility
                _seleniumService.Click(driver, By.XPath("/html/body/ytcp-uploads-dialog/tp-yt-paper-dialog/div/ytcp-animatable[1]/ytcp-uploads-review/div[2]/div[1]/ytcp-video-visibility-select/div[2]/tp-yt-paper-radio-group/tp-yt-paper-radio-button[3]/div[1]"));

                //Publish
                _seleniumService.Click(driver, By.XPath("/html/body/ytcp-uploads-dialog/tp-yt-paper-dialog/div/ytcp-animatable[2]/div/div[2]/ytcp-button[3]"));
                Thread.Sleep(10000);
                #endregion

                #endregion

                video.status_type_id = StatusTypes.COMPLETE.Id;
                video.modified_date = DateTime.UtcNow;
                _database.Update(video);
            }
            catch (Exception ex)
            {
                video.status_type_id = StatusTypes.FAILED_UPLOAD_TO_YOUTUBE.Id;
                video.modified_date = DateTime.UtcNow;
                _database.Update(video);
                _logger.LogError($"{nameof(YoutubeService)} - {nameof(UploadVideo)} - Error: {JsonConvert.SerializeObject(ex)}");
            }

            driver.Close();

            return Task.CompletedTask;
        }
        public List<string> GetCategories()
        {
            return CATEGORIES;
        }

        public string GetDescription()
        {
            return $"This content was created using the mind of AI. Script, Images, and Voice are all AI generated. Categories cycling: {JsonConvert.SerializeObject(CATEGORIES)}  \n\nComment what you categories you want!";;
        }
    }
}
