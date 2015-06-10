using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Simpleloginsystem.Models;
using System.Web.Security;

namespace Simpleloginsystem.Controllers
{
    public class UserController : Controller
    {
        private UserContext db = new UserContext();

        //
        // GET: /User/
       
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

      
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                var crypto = new SimpleCrypto.PBKDF2();

                var encrypPass = crypto.Compute(user.Password);

                var newUser = db.Users.Create();
                newUser.EmailID = user.EmailID;
                newUser.EmployeeName = user.EmployeeName;
                
                newUser.Password = crypto.Salt;
                newUser.EncryptedPassword = crypto.Salt;
                newUser.EmployeeRole = "NORMAL USER";
                newUser.DecryptedPassword = encrypPass;
                db.Users.Add(newUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);

        }
        
        [HttpGet]
        //[Authorize(Roles = "NORMAL USER")]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        
        public ActionResult Login(Login login)
        {
            if (ModelState.IsValid)
            {
                if (IsValid(login.EmailID, login.Password))
                {
                    FormsAuthentication.SetAuthCookie(login.EmailID, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Login Credentials are wrong");
                }
            }

            return View(login);
        }

        public ActionResult Logoff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }


        private bool IsValid(string email, string password)
        {
            bool isValid = false;
            var crypto = new SimpleCrypto.PBKDF2();
            var user = db.Users.FirstOrDefault(u => u.EmailID == email);
            if (user != null)
            {
                
                if (user.DecryptedPassword == crypto.Compute(password,user.EncryptedPassword))
                {
                    isValid = true;
                }
            }
            return isValid;
        }

        public JsonResult doesEmailExist(User email)
        {
            var user = db.Users.FirstOrDefault(u => u.EmailID == email.EmailID);
            return Json(user == null, JsonRequestBehavior.AllowGet);
        }
    }
}