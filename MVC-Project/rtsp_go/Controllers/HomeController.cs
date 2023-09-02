using rtsp_go.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace rtsp_go.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = TempData["message"];
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddIP(IPCamera model)
        {
            if (ModelState.IsValid)
            {
                if (Session["cameras"] == null)
                {
                    Session["cameras"] = new List<IPCamera>();
                }

                //get old session values
                List<IPCamera> ipCameras = (List<IPCamera>)Session["cameras"];

                ipCameras.Add(model);

                Session["cameras"] = ipCameras;
                TempData["message"] = "New ip camera added.";
            }
            else
            {
                TempData["message"] = "Please fill the form properly.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult RemoveAllCameras()
        {
            Session.Remove("cameras");
            TempData["message"] = "All ip cameras removed.";

            return RedirectToAction("Index");
        }

    }
}