using MediaCreatorFunctions.DataAccess;
using MediaCreatorFunctions.DataAccess.Constants;
using MediaCreatorFunctions.DataAccess.Dto;
using MediaCreatorFunctions.DataAccess.DTO;
using MediaCreatorFunctions.Services;
using MediaCreatorFunctions.Utility.Constants;
using MediaCreatorFunctions.Utility.Exceptions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NAudio.Codecs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.Functions.SocialMedia
{
    public class YoutubeFunction
    {
        private readonly IMediaCreatorDatabase _database;
        private readonly IMediaService _mediaService;
        private readonly IFileService _fileService;
        private readonly IBlobService _blobService;
        private readonly IChatGPTService _chatGPTService;
        private readonly IYoutubeService _youtubeService;
        private readonly IDeepAIService _deepAIService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<YoutubeFunction> _logger;

        public YoutubeFunction(IMediaCreatorDatabase database, IMediaService mediaService, IFileService fileService, IBlobService blobService, IChatGPTService chatGPTService, IYoutubeService youtubeService, IDeepAIService deepAIService, IConfiguration configuration, ILogger<YoutubeFunction> logger)
        {
            _database = database;
            _mediaService = mediaService;
            _fileService = fileService;
            _blobService = blobService;
            _chatGPTService = chatGPTService;
            _youtubeService = youtubeService;
            _deepAIService = deepAIService;
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName("Youtube")]
        public async Task Youtube([TimerTrigger("0 0 */6 * * *")] TimerInfo myTimer)
        //public async Task Youtube([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
        {
            try
            {
                #region Check if youtube production is halted
                var config = await _database.FirstOrDefaultAsync<Config>($"name = '{Configs.YOUTUBE_HALT}'");
                if (config != null)
                {
                    var date = DateTime.Parse(config.value);
                    if (date > DateTime.UtcNow)
                    {
                        _logger.LogWarning($"Youtube production halted. Waiting till: {date}");
                        return;
                    }
                }
                #endregion

                var user = await _database.FirstOrDefaultAsync<AppUser>("user_name = @name", new { @name = SocialMedias.YOUTUBE.Name });

                #region Ensure Media User Exists
                //Create media user
                if (user == null)
                {
                    user = await _database.InsertAsync(new AppUser()
                    {
                        email = SocialMedias.YOUTUBE.Name,
                        password = "media",
                        email_confirmed = true,
                        phone_number = "",
                        phone_number_confirmed = false,
                        two_factor_enabled= false,
                        user_name = SocialMedias.YOUTUBE.Name,
                        modified_date = DateTime.UtcNow,
                        modified_by = nameof(YoutubeFunction),
                        created_date = DateTime.UtcNow,
                    });
                }
                #endregion

                #region Create the video
                //Get existing videos
                var videos = (await _database.WhereAsync<Video>("user_id = @id", new { user.id })).ToDictionary(x => x.title);
                var battleVideos = videos.Values.Where(x => x.title.Contains("Battle -")).Select(x => {
                    var chars = GetCharactersFromBattleString(x.title);
                    return new HashSet<string>() { chars.Item1, chars.Item2 };
                }).ToList();
                //Get video from chatgpt
                var random = new Random();
                var categories = _youtubeService.GetCategories();
                var todaysCategory = categories.ElementAt(random.Next(categories.Count));
                _logger.LogInformation($"Category: {todaysCategory}");

                var videoTitle = "";
                var alreadyUsedVideoTitles = new HashSet<string>();
                while (true)
                {
                    var prePrompt = "";
                    if (todaysCategory.Equals("Fight"))
                    {
                        prePrompt = "Pick two RANDOM anime characters and return a string of 'Battle - Person_1 vs Person_2'";
                    }
                    else if (todaysCategory.Equals("Top 5"))
                    {
                        prePrompt = "Pick a RANDOM popular category and return a string of 'Top 5 - Category'";
                    }
                    else if (todaysCategory.Equals("How To"))
                    {
                        prePrompt = "Pick a RANDOM popular category and return a string of 'How To - Category'";
                    }
                    else
                    {
                        prePrompt = $"Your response should be a random title of a video in the category of {todaysCategory}.";
                    }

                    var prompt = alreadyUsedVideoTitles.Any()
                    ? prePrompt
                    : $"{prePrompt}. It cannot be related to any of these: {JsonConvert.SerializeObject(alreadyUsedVideoTitles)}";
                    

                    videoTitle = await _chatGPTService.GetResponseAsync(prompt, "Youtube", user.id);
                    videoTitle = _fileService.SanitizeName(videoTitle.Replace("\"", "").Replace("'", ""));

                    if (!videos.ContainsKey(videoTitle)) break;
                    else if (todaysCategory.Equals("Fight"))
                    {
                        var chars = GetCharactersFromBattleString(videoTitle);
                        if (battleVideos.Any(y => y.Contains(chars.Item1) && y.Contains(chars.Item2)))
                        {
                            alreadyUsedVideoTitles.Add($"Battle - {chars.Item1} vs {chars.Item2}");
                            alreadyUsedVideoTitles.Add($"Battle - {chars.Item2} vs {chars.Item1}");
                        }
                    }
                    else alreadyUsedVideoTitles.Add(videoTitle);
                }

                _logger.LogInformation($"Video Title: {videoTitle}");
                var category = await _database.FirstOrDefaultAsync<Category>("title = @name", new { @name = todaysCategory });
                if(category == null)
                {
                    category = await _database.InsertAsync(new Category() { 
                        title = todaysCategory, 
                        created_date = DateTime.UtcNow, 
                        modified_date = DateTime.UtcNow, 
                        modified_by = nameof(YoutubeFunction) 
                    });
                }
                var video = new Video()
                {
                    title = videoTitle,
                    picture_store_id = Stores.CHAT_GPT.Id,
                    voice_id = random.Next(2) == 0 ? Voices.FEMALE.Id : Voices.MALE.Id,
                    user_id = user.id,
                    width = 512,
                    height = 512,
                    status_type_id = StatusTypes.PROCESSING.Id,
                    created_date= DateTime.UtcNow,
                    modified_date = DateTime.UtcNow,
                    modified_by=nameof(YoutubeFunction),
                };
                video = await _database.InsertAsync(video);
                await _database.InsertAsync(new VideoCategory()
                {
                    video_id = video.id,
                    category_id = category.id,
                    created_date = DateTime.UtcNow
                });

                var videoFileUrl = "";
                try
                {
                    videoFileUrl = await _mediaService.ProcessVideo(video, video.width, video.height, video.voice_id, _chatGPTService, video.user_id);
                }catch(Exception ex)
                {
                    //Delete video so it can restart
                    _logger.LogWarning("Video Failed To Process - Deleting Video From Database");
                    _database.DeleteWhere($"video_id = {video.id}", "VideoCategory");
                    _database.DeleteWhere($"id = {video.id}", "Video");
                    throw;
                }
                #endregion

                #region Submit the video
                await _youtubeService.UploadVideo(videoFileUrl, todaysCategory, _youtubeService.GetDescription(), video);
                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(YoutubeFunction)} - Error - {JsonConvert.SerializeObject(ex)}");
            }
        }

        private Tuple<string, string> GetCharactersFromBattleString(string title)
        {
            var startIndex = 9;
            var firstCharacter = "";
            var secondCharacter = "";
            var checkVS = "";

            try
            {
                for (var i = startIndex; i < title.Length; i++)
                {
                    checkVS = $"{title[i]}{title[i + 1]}{title[i + 2]}{title[i + 3]}";
                    if (checkVS.Equals(" vs "))
                    {
                        secondCharacter = title.Substring(i + 4);
                        break;
                    }
                    else if(checkVS.Equals(" vs."))
                    {
                        secondCharacter = title.Substring(i + 5);
                        break;
                    }
                    firstCharacter += title[i];
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
            }

            return new Tuple<string, string>(firstCharacter, secondCharacter);
        }
    }
}
