using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HMSApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserSession"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            return View();
        }

    }
}
 
