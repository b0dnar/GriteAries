using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using GriteAries.Models;

namespace GriteAries.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Job _job = new Job();

            Task.Factory.StartNew(() => _job.RunFootball());


            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
