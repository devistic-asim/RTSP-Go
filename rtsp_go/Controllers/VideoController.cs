using rtsp_go.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Xabe.FFmpeg;

namespace rtsp_go.Controllers
{
    public class VideoController : Controller
    {

        // GET: Video
        public async Task<ActionResult> Index()
        {
            bool IsStreamStart = await StartStreamAsync();

            return View();
        }

        public ActionResult StartVideo()
        {
            var name = "";

            if (Session["cameras"] != null)
            {
                List<IPCamera> ipCameras = (List<IPCamera>)Session["cameras"];
                name = ipCameras.FirstOrDefault().Name + ".m3u8.tmp";
            }

            ViewBag.Name = name;
            return View();
        }

        public string getVideo()
        {
            var name = "";

            if (Session["cameras"] != null)
            {
                List<IPCamera> ipCameras = (List<IPCamera>)Session["cameras"];
                name = ipCameras.FirstOrDefault().Name + ".m3u8.tmp";
            }

            //Response.Headers.Add("Access-Control-Allow-Origin", "*");
            //Response.Headers.Add("Cache-Control", "no-cache");

            string textOutput = "";

            try
            {
                string path = Path.Combine(Server.MapPath("~/video"), name);
                textOutput = System.IO.File.ReadAllText(path);
            }
            catch (Exception ex)
            {
            }

            return textOutput;
        }

        public bool DeleteOldFiles()
        {
            string folderName = Server.MapPath("~/video");
            string[] files = Directory.GetFiles(folderName);

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);

                if (fi.Extension == ".ts" && fi.CreationTime < DateTime.Now.AddMinutes(-2))
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return true;
        }

        private async Task<bool> StartStreamAsync()
        {
            var recordingUri = "";
            var fileName = "cam1.m3u8";

            if (Session["cameras"] != null)
            {
                List<IPCamera> ipCameras = (List<IPCamera>)Session["cameras"];

                recordingUri = ipCameras.FirstOrDefault().IP;
                fileName = ipCameras.FirstOrDefault().Name + ".m3u8";
            }

            //string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".m3u8");
            //var file = DateTime.UtcNow.ToString("dd-MM-yyyy")+"\\"+Guid.NewGuid() + ".m3u8";
            string output = "";

            string folderName = Server.MapPath("~/video");
            string LibFFMpeg = Server.MapPath("~/LibFFMpeg");

            // If directory does not exist, create it
            if (!Directory.Exists(folderName) || true)
            {
                Directory.CreateDirectory(folderName);

                output = Path.Combine(folderName, fileName);

                FFmpeg.SetExecutablesPath(LibFFMpeg, ffmpegExeutableName: "ffmpeg", ffprobeExecutableName: "ffprobe");

                var mediaInfo = await FFmpeg.GetMediaInfo(recordingUri);

                //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                var conversionResult = FFmpeg.Conversions.New()
                    .AddStream(mediaInfo.Streams)
                    //.SetInputTime(TimeSpan.FromSeconds(3))
                    .SetOutput(output)
                    .Start(/*cancellationTokenSource.Token*/);

                //Session["cancel_token"] = cancellationTokenSource;

                //await Task.Delay(5000);
                //cancellationTokenSource.Cancel();

                //End time of streaming
                //conversionResult.Result.EndTime.AddMinutes(2);

                //Allow any Cors
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

                //result.EnableRangeProcessing = true;

                return true;
            }

            return false;
        }

        public ActionResult StopStream()
        {
            //CancellationTokenSource cancellationTokenSource = (CancellationTokenSource)Session["cancel_token"];
            //cancellationTokenSource.Cancel();

            //delete directory
            try
            {
                string path = Server.MapPath("~/video");
                Directory.Delete(path);

                TempData["message"] = "Stream stopped.";

                //add directory
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
            }

            return RedirectToAction("Index", "Home");
        }
    }
}