using MediaCreatorFunctions.DataAccess;
using MediaCreatorFunctions.DataAccess.DTO;
using Newtonsoft.Json;
using System.Diagnostics;
using MediaCreatorFunctions.Utility.Exceptions;
using MediaCreatorFunctions.Models;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace MediaCreatorFunctions.Services
{
    /// <summary>
    /// Going to have to add functionality to create videos from pictures and text, might need a Audio Service as well
    /// </summary>
    public interface IVideoService
    {
        Task<string> GenerateVideoFromScenes(List<Scene> scenes, string videoFolderPath);
    }
    public class VideoService : IVideoService
    {

        private readonly ILogger<VideoService> _logger;
        private readonly IFileService _fileService;
        private readonly IMediaCreatorDatabase _database;
        private readonly IConfiguration _configuration;

        private readonly HashSet<string> POSSIBLE_MUSIC_TYPES = new HashSet<string>();

        public VideoService(ILogger<VideoService> logger, IFileService fileService, IMediaCreatorDatabase database, IConfiguration configuration)
        {
            _logger = logger;
            _fileService = fileService;
            _database = database;
            _configuration = configuration;

            POSSIBLE_MUSIC_TYPES = _fileService.GetFileNames(_configuration["WorkingDirectory"] + "\\Music").ToHashSet();
        }

        public async Task<string> GenerateVideoFromScenes(List<Scene> scenes, string videoFolderPath)
        {
            //Generate image sequence and audio files
            await _fileService.DeleteContent(videoFolderPath);
            _fileService.EnsureFolderExistenceFolderPath(videoFolderPath);
            string imagesListPath = $"{videoFolderPath}\\images.txt";
            string audioListPath = $"{videoFolderPath}\\audio.txt";
            string subtitlesPath = $"{videoFolderPath}\\subtitles.srt";

            using (StreamWriter swImages = new StreamWriter(imagesListPath), swAudio = new StreamWriter(audioListPath), swSubtitles = new StreamWriter(subtitlesPath))
            {
                TimeSpan currentTime = TimeSpan.Zero;
                for (var i = 0; i < scenes.Count; i++)
                {
                    var audioDuration = GetAudioDuration(scenes[i].audioFilePath);

                    await swImages.WriteAsync($"file '{scenes[i].pictureFilePath}'\n");
                    await swImages.WriteAsync($"duration {audioDuration.TotalSeconds}\n");

                    await swAudio.WriteLineAsync($"file '{scenes[i].audioFilePath}'\n");

                    await swSubtitles.WriteLineAsync($"{i + 1}\n{currentTime:hh\\:mm\\:ss\\,fff} --> {(currentTime + audioDuration):hh\\:mm\\:ss\\,fff}\n{scenes[i].text}\n");
                    currentTime += audioDuration;
                }
            }

            // Combine audio files into one
            string outputAuduiPath = $"{videoFolderPath}\\outputAudio.mp3";
            var combineAudioStartInfo = new ProcessStartInfo
            {
                FileName = _configuration["ffmpeg"],
                Arguments = $"-f concat -safe 0 -i \"{audioListPath}\" -c copy \"{outputAuduiPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            await RunProcess(combineAudioStartInfo);

            // Create video from image list
            string outputImagesPath = $"{videoFolderPath}\\outputImages.mp4";
            var createVideoImagesStartInfo = new ProcessStartInfo
            {
                FileName = _configuration["ffmpeg"],
                Arguments = $"-f concat -safe 0 -i \"{imagesListPath}\" -c:v libx264 -pix_fmt yuv420p \"{outputImagesPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            await RunProcess(createVideoImagesStartInfo);

            // Merge audio and image video into a single video
            string outputVideoPath = $"{videoFolderPath}\\output.mp4";
            var mergeStartInfo = new ProcessStartInfo
            {
                FileName = _configuration["ffmpeg"],
                Arguments = $"-i \"{outputImagesPath}\" -i \"{outputAuduiPath}\" -c:v copy -c:a aac -map 0:v:0 -map 1:a:0 \"{outputVideoPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            await RunProcess(mergeStartInfo);

            string outputVideoWithSubtitlesPath = $"{videoFolderPath}\\output_subtitles.mp4";

            var addSubtitlesStartInfo = new ProcessStartInfo
            {
                FileName = _configuration["ffmpeg"],
                Arguments = $"-i output.mp4 -vf subtitles=subtitles.srt output_subtitles.mp4",
                WorkingDirectory= videoFolderPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            await RunProcess(addSubtitlesStartInfo);

            //Assuming we are creating video with id=1, duration=60 seconds, status_type_id=1, created_date=DateTime.Now
            return outputVideoWithSubtitlesPath;
        }

        public async Task RunProcess(ProcessStartInfo startInfo)
        {
            using (var process = new Process { StartInfo = startInfo })
            {
                var errorData = "";
                process.OutputDataReceived += (sender, args) =>
                {
                    _logger.LogInformation(args.Data); // Do something with the output
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    _logger.LogError(args.Data);
                    errorData += args.Data; // Do something with the error
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg exited with code {errorData}");
                }
            }
        }



        public static TimeSpan GetAudioDuration(string filePath)
        {
            using (var reader = new NAudio.Wave.AudioFileReader(filePath))
            {
                return reader.TotalTime;
            }
        }


    }
}
