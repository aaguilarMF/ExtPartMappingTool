using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExtPartMappingTool.Controllers
{
    //[Authorize(Roles = "DataTeam_APPS_RW, DataTeam_APPS_RO, APP_ADMINS")]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult LandingPage()
        {
            ViewBag.Name = User.Identity.Name;
#if DEBUG
            ViewBag.DEBUG = true;
#else
            ViewBag.DEBUG = false;
#endif
            return View();
        }
        //[Authorize(Roles = "DataTeam_APPS_RW, APP_ADMINS")]
        //[AllowAnonymous]
        public ActionResult ProcessAcesPiesFiles()
        {
            return View();
        }
        
    }
}