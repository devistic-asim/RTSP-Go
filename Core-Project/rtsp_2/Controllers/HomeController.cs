using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using rtsp_2.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Xabe.FFmpeg;

namespace rtsp_2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(ILogger<HomeController> logger, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            ViewBag.Message = TempData["message"];
            return View();
        }

        public async Task<IActionResult> StartStream(IPCamera model)
        {
            string projectRootPath = _hostingEnvironment.ContentRootPath;
            string webRootPath = _hostingEnvironment.WebRootPath;

            string output = "";

            string recordingUri = model.IP;
            var fileName = model.Name + ".m3u8";

            string folderName = Path.Combine(webRootPath + "\\Video");

            if (!Directory.Exists(folderName) || true)
            {
                Directory.CreateDirectory(folderName);
                output = Path.Combine(webRootPath + "\\Video", fileName);

                //set ffmpeg exe
                FFmpeg.SetExecutablesPath(projectRootPath + "\\LibFFMpeg\\", ffmpegExeutableName: "ffmpeg", ffprobeExecutableName: "ffprobe");

                var mediaInfo = await FFmpeg.GetMediaInfo(recordingUri);

                var conversionResult = FFmpeg.Conversions.New()
                    .AddStream(mediaInfo.Streams)
                    .SetOutput(output)
                    .Start();

                //End time of streaming
                //conversionResult.Result.EndTime.AddMinutes(2);

                // Allow any Cors
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                Response.Headers.Add("Cache-Control", "no-cache");

                int i = 0;
                // Open the file, and read the stream to return to the client
                while (i == 0)
                {
                    try
                    {
                        FileStreamResult alpha = new FileStreamResult(System.IO.File.Open(output, FileMode.Open, FileAccess.Read, FileShare.Read), "application/x-mpegURL");
                        i++;
                    }
                    catch (Exception)
                    {
                    }
                }

                FileStreamResult result = new FileStreamResult(System.IO.File.Open(output, FileMode.Open, FileAccess.Read, FileShare.Read), "application/x-mpegURL");
                result.EnableRangeProcessing = true;

                ViewBag.FileName =  "cam1.m3u8";
                return View();
            }
            TempData["message"] = "Something went wrong.";
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<FileStreamResult> StartStream()
        {
            //string deviceIp = "rtsp://192.168.1.102";
            //string recordingUri = "rtsp://asim:pass@192.168.1.105:1935/";
            string recordingUri = "rtsp://asim:pass@192.168.1.101:1935/";
            //string recordingUri = "rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mp4";

            //string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".m3u8");
            //var file = DateTime.UtcNow.ToString("dd-MM-yyyy")+"\\"+Guid.NewGuid() + ".m3u8";
            var file = "cam1.m3u8";
            string output = "";

            //string folderName = Path.Combine("C:\\Local Disk D\\Visual Studio\\repos\\Clients\\RTSP\\.net projects\\core\\rtsp_2\\wwwroot\\video\\\", DateTime.UtcNow.ToString("dd-MM-yyyy"));
            string folderName = Path.Combine("C:\\Local Disk D\\Visual Studio\\repos\\Clients\\RTSP\\.net projects\\core\\rtsp_2\\wwwroot\\video\\");
            // If directory does not exist, create it
            if (!Directory.Exists(folderName) || true)
            {
                Directory.CreateDirectory(folderName);

                //string output = Path.Combine("C:\\\Local Disk D\\\Visual Studio\\\repos\\\Clients\\\RTSP\\\9\\\rtsp_2\\\rtsp_2\\\wwwroot\\\video\\\", file);
                output = Path.Combine("C:\\Local Disk D\\Visual Studio\\repos\\Clients\\RTSP\\.net projects\\core\\rtsp_2\\wwwroot\\video\\", file);

                FFmpeg.SetExecutablesPath("C:\\Local Disk D\\Visual Studio\\repos\\Clients\\RTSP\\9\\rtsp_2\\rtsp_2\\LibFFMpeg\\", ffmpegExeutableName: "ffmpeg", ffprobeExecutableName: "ffprobe");

                var mediaInfo = await FFmpeg.GetMediaInfo(recordingUri);

                var conversionResult = FFmpeg.Conversions.New()
                    .AddStream(mediaInfo.Streams)
                    .SetOutput(output)
                    .Start();

                //End time of streaming
                //conversionResult.Result.EndTime.AddMinutes(2);

                // Allow any Cors
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                Response.Headers.Add("Cache-Control", "no-cache");

                int i = 0;
                // Open the file, and read the stream to return to the client
                while (i == 0)
                {
                    try
                    {
                        FileStreamResult alpha = new FileStreamResult(System.IO.File.Open(output, FileMode.Open, FileAccess.Read, FileShare.Read), "application/x-mpegURL");
                        i++;
                    }
                    catch (Exception)
                    {
                    }
                }

                FileStreamResult result = new FileStreamResult(System.IO.File.Open(output, FileMode.Open, FileAccess.Read, FileShare.Read), "application/x-mpegURL");
                result.EnableRangeProcessing = true;

                return result;
            }

            return null;
        }

    }
}
