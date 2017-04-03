using HMSApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HMSApp.Controllers
{
    public class UserController : Controller
    {
        private HMSDBDBContext db = new HMSDBDBContext();

        //
        // GET: /User/
        public ActionResult Login()
        {
            User user = new User();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include =
                                                "UserName"
                                             + "," + "Password")] User user)
        {
            var existingUser = db.Users.SingleOrDefault(u => u.UserName.Trim() == user.UserName.Trim()
                                                                             && u.Password.Trim() == user.Password.Trim());
            
            if(existingUser!=null)
            {
                Session["UserSession"] = existingUser;
                return RedirectToAction("Index", "Home");
                  
            }
            else
            {
                user.Message = "Invalid Login";
            }
           return View(user);
        }
	}
}