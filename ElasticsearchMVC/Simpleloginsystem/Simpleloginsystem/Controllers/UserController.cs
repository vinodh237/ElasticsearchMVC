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
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Simpleloginsystem.Controllers
{
    public class Encryption
    {
        private const string _defaultKey = "*3ld+43j";
        public static string Encrypt(string toEncrypt, string key)
        {
            var des = new DESCryptoServiceProvider();
            var ms = new MemoryStream();
            VerifyKey(ref key);
            des.Key = HashKey(key, des.KeySize / 8);
            des.IV = HashKey(key, des.KeySize / 8);
            byte[] inputBytes = Encoding.UTF8.GetBytes(toEncrypt);
            var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputBytes, 0, inputBytes.Length);
            cs.FlushFinalBlock();
            return HttpServerUtility.UrlTokenEncode(ms.ToArray());
        }

        public static string Decrypt(string toDecrypt, string key)
        {
            var des = new DESCryptoServiceProvider();
            var ms = new MemoryStream();
            VerifyKey(ref key);
            des.Key = HashKey(key, des.KeySize / 8);
            des.IV = HashKey(key, des.KeySize / 8);
            byte[] inputBytes = HttpServerUtility.UrlTokenDecode(toDecrypt);
            var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputBytes, 0, inputBytes.Length);
            cs.FlushFinalBlock();
            var encoding = Encoding.UTF8;
            return encoding.GetString(ms.ToArray());
        }

        /// <summary>
        /// Make sure key is exactly 8 characters
        /// </summary>
        /// <param name="key"></param>
        private static void VerifyKey(ref string key)
        {
            if (string.IsNullOrEmpty(key))
                key = _defaultKey;

            key = key.Length > 8 ? key.Substring(0, 8) : key;

            if (key.Length < 8)
            {
                for (int i = key.Length; i < 8; i++)
                {
                    key += _defaultKey[i];
                }
            }
        }

        private static byte[] HashKey(string key, int length)
        {
            var sha = new SHA1CryptoServiceProvider();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] hash = sha.ComputeHash(keyBytes);
            byte[] truncateHash = new byte[length];
            Array.Copy(hash, 0, truncateHash, 0, length);
            return truncateHash;
        }
    }

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

       public static string FriendlyPassword()
       {
           string newPassword = Membership.GeneratePassword(8, 0);
           newPassword = Regex.Replace(newPassword, @"[^a-zA-Z0-9]", m => "9");
           return newPassword;
       }

       [HttpGet]
       public ActionResult RequestResetPasswordLink(string userName)
       {
           bool result = doesEmailVerified(userName);
           if (!result)
           {
               return RedirectToAction("EmailNotVerified", "User");
           }
           else
           {
               ViewBag.Message = "Thank you for submitting your request. Please check your email for a reset password link.";
               if (string.IsNullOrEmpty(userName))
               {
                   return View();
               }
               var existingUser = db.Register.FirstOrDefault(u => u.EmailID == userName);
               if (existingUser == null)
               {

                   return View();
               }
               SendResetEmail(existingUser);
               return View();
           }
       }

       public static void SendResetEmail(Register user)
       {
           string encrypted = Encryption.Encrypt(String.Format("{0}&{1}", user.UserName, DateTime.Now.AddMinutes(10).Ticks), user.EmailID);
           var passwordLink = "http://localhost:10096/User/ResetPassword?digest=" + HttpUtility.UrlEncode(encrypted) + ":" + user.EmailID;
           var email = new MailMessage();
           email.From = new MailAddress("ankit.bhatla@kuliza.com");
           email.To.Add(new MailAddress(user.EmailID));
           email.Subject = "Password Reset";
           email.IsBodyHtml = true;
           email.Body += "<p>A request has been recieved to reset your password. If you did not initiate the request, then please ignore this email.</p>";
           email.Body += "<p>Please click the following link to reset your password: <a href='" + passwordLink + "'>" + passwordLink + "</a></p>";
           SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
           smtpClient.Port = 587;
           smtpClient.Credentials = new System.Net.NetworkCredential("ankit.bhatla@kuliza.com", "ankit@123456");
           smtpClient.EnableSsl = true;
           try
           {
               smtpClient.Send(email);
           }
           catch (Exception ex)
           {

           }
       }
       public static void SendNewPasswordEmail(string email, string newPassword)
       {
           MailMessage mail = new MailMessage();
           mail.From = new MailAddress("ankit.bhatla@kuliza.com");
           mail.To.Add(email);
           mail.Subject = "Your new Credentials";
           var body = @"Your password has been reset. You can change this password once you have logged in. Your user name is: {0} Your reset password is: {1} To login, go to: http://localhost:10096/User/Login";
           mail.Body = string.Format(body, email, newPassword);
           SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
           smtpClient.Port = 587;
           smtpClient.Credentials = new System.Net.NetworkCredential("ankit.bhatla@kuliza.com", "ankit@123456");
           smtpClient.EnableSsl = true;

           try
           {
               smtpClient.Send(mail);
           }
           catch (Exception ex)
           {

           }
           finally
           {
               mail.Dispose();
               smtpClient.Dispose();
           }
       }

       public static string setNewPassword(string user)
       {
           string newPass = FriendlyPassword();
           return newPass;
       }


       [HttpGet]
       public ActionResult ResetPassword(string digest)
       {
           bool result = ValidateResetCode(HttpUtility.UrlDecode(digest));
           if (!result)
           {
               ViewBag.Message = "Invalid or expired link. Please try again";
               
               return View();
           }
           var part = digest.Split(':');
           string newPass = setNewPassword(part[1]);
           var crypto = new SimpleCrypto.PBKDF2();
           var encrypPass = crypto.Compute(newPass);
           var connection = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename ='|DataDirectory|\Employee.mdf';Integrated Security=True");
           string sqlquery = @"UPDATE [dbo].[tblUser] SET [Password] = @pass, [EncryptedPassword] = @encryp, [DecryptedPassword] = @decryp, [RecentPasswordReset] = @flag " +
                         @"WHERE [EmailId] = @email";
           var cmd = new SqlCommand(sqlquery, connection);
           cmd.Parameters.Add(new SqlParameter("@pass", SqlDbType.NVarChar)).Value = crypto.Salt;
           cmd.Parameters.Add(new SqlParameter("@encryp", SqlDbType.VarChar)).Value = crypto.Salt;
           cmd.Parameters.Add(new SqlParameter("@decryp", SqlDbType.NVarChar)).Value = encrypPass;
           cmd.Parameters.Add(new SqlParameter("@email", SqlDbType.VarChar)).Value = part[1];
           cmd.Parameters.Add(new SqlParameter("@flag", SqlDbType.Int)).Value = 1;
           connection.Open();
           cmd.ExecuteReader();
           SendNewPasswordEmail(part[1], newPass);
           ViewBag.Message = "Thank you. Your new password has been emailed to you. You may change it after you log in.";
           connection.Close();
           cmd.Dispose();
           return View();
       }

       public static bool ValidateResetCode(string encryptedparam)
       {
           string decrypted = "";
           var parts = encryptedparam.Split(':');
           try
           {
               decrypted = Encryption.Decrypt(parts[0], parts[1]);
           }
           catch (Exception ex)
           {
               return false;
           }
           if (parts.Length != 2)
           {
               return false;
           }
           var expires = DateTime.Now.AddHours(-1);
           long ticks = 0;
           var time = decrypted.Split('&');
           if (!long.TryParse(time[1], out ticks))
           {
               return false;
           }
           expires = new DateTime(ticks);
           if (expires < DateTime.Now)
           {
               return false;
           }
           return true;
       }

       public ActionResult ForgotPassword(ForgotPassword user)
       {
           if (ModelState.IsValid)
           {
               return RedirectToAction("RequestResetPasswordLink", "User", new { userName = user.EmailID });
           }

           return View();
       }

       public void sendRegistrationMail(string email, Register user)
       {
           try
           {
               string encrypted = Encryption.Encrypt(String.Format("{0}&{1}", user.UserName, DateTime.Now.AddMinutes(10).Ticks), user.EmailID);
               var passwordLink = "http://localhost:10096/User/Login?digest=" + HttpUtility.UrlEncode(encrypted) + ":" + user.EmailID;
               MailMessage mail = new MailMessage();
               SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
               mail.From = new MailAddress("ankit.bhatla@kuliza.com");
               mail.To.Add(email);
               mail.Subject = "Verification Mail";
               mail.Body += "<p>Please verify your password.Please click the following link to verify: </p> <a href='" + passwordLink + "'>" + passwordLink + "</a>";
               SmtpServer.Port = 587;
               SmtpServer.Credentials = new System.Net.NetworkCredential("ankit.bhatla@kuliza.com", "ankit@123456");
               SmtpServer.EnableSsl = true;
               SmtpServer.Send(mail);
           }
           catch (Exception ex)
           {

           }
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
                sendRegistrationMail(user.EmailID,user);
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

        public ActionResult NotAValidLink(string email)
        {
            ViewBag.Message = "Invalid or expired link. Please try again";
            return View();
        }


        [HttpPost]
        public ActionResult Login(Login login, string digest)
        {
            if (ModelState.IsValid)
            {
                if (digest != null)
                {
                    var part = digest.Split(':');
                    bool result = ValidateResetCode(HttpUtility.UrlDecode(digest));
                    if (!result)
                    {
                        return RedirectToAction("NotAValidLink", "User",new {email = part[1]});
                    }
                    
                    var connection = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename ='|DataDirectory|\Employee.mdf';Integrated Security=True");
                    string sqlquery = @"UPDATE [dbo].[tblUser] SET [VerifiedEmail] = @flag " +
                                      @"WHERE [EmailId] = @email";
                    var cmd = new SqlCommand(sqlquery, connection);
                    cmd.Parameters.Add(new SqlParameter("@email", SqlDbType.VarChar)).Value = part[1];
                    cmd.Parameters.Add(new SqlParameter("@flag", SqlDbType.Int)).Value = 1;
                    connection.Open();
                    cmd.ExecuteReader();
                    connection.Close();
                    cmd.Dispose();
                }
                if (IsValid(login.EmailID, login.Password) == 1)
                {
                    FormsAuthentication.SetAuthCookie(login.EmailID, false);
                    return RedirectToAction("Index", "Home");
                }
                else if (IsValid(login.EmailID, login.Password) == 2)
                {
                    FormsAuthentication.SetAuthCookie(login.EmailID, false);
                    return RedirectToAction("ChangePassword", "User", new{email = login.EmailID});
                }
                else if (IsValid(login.EmailID, login.Password) == 3)
                {

                    return RedirectToAction("EmailNotVerified", "User" );
                }
                else
                {
                    ModelState.AddModelError("", "Login Credentials are wrong");
                }
            }

            return View(login);
        }

        public ActionResult EmailNotVerified()
        {
            ViewBag.Message = "Email address not verified!!!! Please check you Email for verification";
            return View();
        }

        public ActionResult ChangePassword(string email, ChangePassword user)
        {
            if (ModelState.IsValid)
            {
                var crypto = new SimpleCrypto.PBKDF2();
                var encrypPass = crypto.Compute(user.Password);
                var connection = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename ='|DataDirectory|\Employee.mdf';Integrated Security=True");
                string sqlquery = @"UPDATE [dbo].[tblUser] SET [Password] = @pass, [EncryptedPassword] = @encryp, [DecryptedPassword] = @decryp, [RecentPasswordReset] = @flag " +
                              @"WHERE [EmailId] = @email";
                var cmd = new SqlCommand(sqlquery, connection);
                cmd.Parameters.Add(new SqlParameter("@pass", SqlDbType.NVarChar)).Value = crypto.Salt;
                cmd.Parameters.Add(new SqlParameter("@encryp", SqlDbType.VarChar)).Value = crypto.Salt;
                cmd.Parameters.Add(new SqlParameter("@decryp", SqlDbType.NVarChar)).Value = encrypPass;
                cmd.Parameters.Add(new SqlParameter("@email", SqlDbType.VarChar)).Value =email ;
                cmd.Parameters.Add(new SqlParameter("@flag", SqlDbType.Int)).Value = 0;
                connection.Open();
                cmd.ExecuteReader();
            }
            return View();
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


        private int IsValid(string email, string password)
        {
            int isValid= 0;
            var crypto = new SimpleCrypto.PBKDF2();
            var user = db.Register.FirstOrDefault(u => u.EmailID == email);
            if (user != null)
            {
                if (user.DecryptedPassword == crypto.Compute(password, user.EncryptedPassword) && user.VerifiedEmail == 0)
                {
                    isValid = 3;
                }else if (user.DecryptedPassword == crypto.Compute(password,user.EncryptedPassword)  && user.RecentPasswordReset == 0)
                {
                    isValid = 1;
                }else if (user.DecryptedPassword == crypto.Compute(password,user.EncryptedPassword)  && user.RecentPasswordReset == 1)
                {
                    isValid = 2;
                }
               
            }
            return isValid;
        }

        public JsonResult doesEmailExistRegister(Register email)
        {
            var user = db.Register.FirstOrDefault(u => u.EmailID == email.EmailID);
            return Json(user == null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult doesEmailExist(Register email)
        {
            var user = db.Register.FirstOrDefault(u => u.EmailID == email.EmailID);
            return Json(user != null, JsonRequestBehavior.AllowGet);
        }

        public bool doesEmailVerified(string email)
        {
            var user = db.Register.FirstOrDefault(u => u.EmailID == email);
            if (user.VerifiedEmail == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}