using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GriteAries.BK.Parse;

namespace GriteAries.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Marathon marathon = new Marathon();
            marathon.ParseLiveHockey();
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
