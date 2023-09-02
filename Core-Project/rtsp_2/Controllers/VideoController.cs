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
    public class VideoController : Controller
    {
        private readonly ILogger<VideoController> _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        public VideoController(ILogger<VideoController> logger, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index(string fileName = "")
        {
            ViewBag.FileName = fileName;
            return View();
        }

        public string Video(string fileName = "cam1.m3u8")
        {
            string projectRootPath = _hostingEnvironment.ContentRootPath;
            string webRootPath = _hostingEnvironment.WebRootPath;

            if (fileName == null)
            {
                fileName = "cam1.m3u8.tmp";
            }

            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Cache-Control", "no-cache");

            string output = "";

            try
            {
                string outputPath = Path.Combine(webRootPath + "\\Video", fileName);
                output = System.IO.File.ReadAllText(outputPath);
            }
            catch (Exception)
            {
            }

            return output;
        }

    }
}
