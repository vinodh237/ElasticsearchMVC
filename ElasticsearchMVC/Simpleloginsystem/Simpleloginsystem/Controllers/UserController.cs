using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Simpleloginsystem.Models;
using System.Web.Security;
using Nest;

namespace Simpleloginsystem.Controllers
{
    public class UserController : Controller
    {
        private UserContext db = new UserContext();

        //
        // GET: /User/
       
        public ActionResult Index()
        {
            return View(db.Register.ToList());
        }

       [RedirectAuthenticatedRequests]
        public ActionResult Register()
        {
            
            return View();
            
        }

        //
        // POST: /User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Register user)
        {
            if (ModelState.IsValid)
            {
                var crypto = new SimpleCrypto.PBKDF2();
                var encrypPass = crypto.Compute(user.Password);
                var newUser = db.Register.Create();
                newUser.EmailID = user.EmailID;
                newUser.UserName = user.UserName;
                newUser.Password = crypto.Salt;
                newUser.EncryptedPassword = crypto.Salt;
                newUser.UserRole = "NORMAL USER";
                newUser.DecryptedPassword = encrypPass;
                db.Register.Add(newUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }
        
        [HttpGet]
        //[Authorize(Roles = "NORMAL USER")]
        [RedirectAuthenticatedRequests]
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

      //  [Authorize(Roles = "Admin")]
        public ActionResult ReIndex()
        {
            var node = new Uri("http://localhost:9200");

            var settings = new ConnectionSettings(
                node
                
            );

            var client = new ElasticClient(settings);


            //client.CreateIndex("my-app");
            //client.Map<Register>(c => c.MapFromAttributes());
            foreach (var user in db.Register)
            {
                //client.Index(user);
                client.Index(user, i => i
    .Index("my-app")
    .Type("register")
    .Id(user.UserID)
);

            }
            return RedirectToAction("Index");
        }

//        private static ElasticClient ElasticClient
//{
//    get
//    {
        
       
//        var node = new Uri("http://localhost:9200");
//          var settings = new ConnectionSettings(
//                node,
//                defaultIndex: "my-application"
//            );

//            return new ElasticClient(settings);
//    }
//}
        public ActionResult Search(string searchString)
        {

            var node = new Uri("http://localhost:9200");

            var settings = new ConnectionSettings(
                node,
                defaultIndex: "my-app"
            );

            var client = new ElasticClient(settings);
            var result = client.Search<Register>(body =>
                body.Query(query =>
                query.QueryString(qs => qs.Query(searchString))));
            // List<Register> register = new List<Register>();
            //register.Add(result.Documents.ToList());
            // re
            //{
            //   //EmailID = q,
            //   //DecryptedPassword = q,
            //   //EncryptedPassword = q,
            //   //Password =q,
            //   //UserName =       q,
            //   //UserRole = "Normal User",
            //   return result.Documents.ToList();


            //    //Name = "Search results for " + q,
            //    //Albums = result.Documents.ToList()
            //};

            List<Register> viewModelList = new List<Register>((result.Documents.ToList()));
            
            Console.WriteLine("Queried");
            return View("Index", viewModelList);
        }

        public ActionResult Edit(int id = 0)
        {
            Register register = db.Register.Find(id);
            if (register == null)
            {
                return HttpNotFound();
            }
            return View(register);
        }

        //
        // POST: /Fake/Edit/5

        [HttpPost]
        public ActionResult Edit(Register register)
        {
            if (ModelState.IsValid)
            {
                db.Entry(register).State = EntityState.Modified;
              
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View( register);
        }


        private bool IsValid(string email, string password)
        {
            bool isValid = false;
            var crypto = new SimpleCrypto.PBKDF2();
            var user = db.Register.FirstOrDefault(u => u.EmailID == email);
            if (user != null)
            {
                
                if (user.DecryptedPassword == crypto.Compute(password,user.EncryptedPassword))
                {
                    isValid = true;
                }
            }
            return isValid;
        }

        public JsonResult doesEmailExist(Register email)
        {
            var user = db.Register.FirstOrDefault(u => u.EmailID == email.EmailID);
            return Json(user == null, JsonRequestBehavior.AllowGet);
        }
    }
}