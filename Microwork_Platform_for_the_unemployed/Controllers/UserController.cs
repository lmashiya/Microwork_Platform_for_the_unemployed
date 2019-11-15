using Microwork_Platform_for_the_unemployed.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using System.Web.UI.WebControls;

namespace Microwork_Platform_for_the_unemployed.Controllers
{
    public class UserController : Controller
    {
        //[HttpGet]
        //public ActionResult ViewResult()
        //{

        //    return View();
        //}

        
        public ActionResult ViewResult()
        {
            var user = new User();
            
            using (var entity = new JobAtYourFingerTipsEntities())
            {
                var account = entity.Users.FirstOrDefault(x => x.EmailAddress == HttpContext.User.Identity.Name);
                if (account != null)
                {
                    user.FirstName = account.FirstName;
                    user.DesiredCity = account.DesiredCity;
                    user.EmailAddress = account.EmailAddress;
                    user.MobileNumber = account.MobileNumber;
                    user.Province = account.Province;
                    user.Age = account.Age;
                    user.Province = account.Province;
                    user.City = account.City;
                    user.DateOfBirth = account.DateOfBirth;
                    user.Experience = account.Experience;
                    user.Higher = account.Higher;
                    user.Gender = account.Gender;
                    user.LastName = account.LastName;
                    user.DesiredProvince = account.DesiredProvince;
                    user.KeySkills = account.KeySkills;
                    user.DesiredProvince = account.DesiredProvince;
                }
            }

            return View(user);
        }

        //Registration action
        [HttpGet]
      public ActionResult Registration()
      {
          return View();
      }
        //Registration Post action

        #region Registration
        [HttpPost]
        public ActionResult Registration([Bind(Exclude = "RegistrationDate,IsEmailVerified,ActivationCode,Resume")] User user)
        {
            var status = false;
            var message = "";
            //Model Validation
            if (ModelState.IsValid)
            {
                #region check if email exists
                var isExists = IsEmailExist(user.EmailAddress);
                if (isExists)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code

                user.ActivationCode = Guid.NewGuid();

                #endregion

                #region Set Registration date
                user.RegistrationDate = DateTime.Today;
                #endregion

                #region Password hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                #endregion

                user.IsEmailVerified = false;
                user.Resume = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

                #region Save To Database

                using (var entity = new JobAtYourFingerTipsEntities())
                {
                    entity.Users.Add(user);
                    entity.SaveChanges();
                }

                #region Send Email To New User

                SendVerificationLinkEmail(user.EmailAddress, user.ActivationCode.ToString());
                message = "Registration successfully created. Account sent to your email address : " +
                          user.EmailAddress;
                status = true;

                #endregion

                #endregion
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.Exception));
                }
                message = "Invalid State";

            }

            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(user);
        }
 

        #endregion 
        #region VerifyAccount

        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool status = false;
            using (var entity = new JobAtYourFingerTipsEntities())
            {
                entity.Configuration.ValidateOnSaveEnabled = false;
                var codeChecker = entity.Users.FirstOrDefault(x => x.ActivationCode == new Guid(id));
                if (codeChecker != null)
                {
                    codeChecker.IsEmailVerified = true;
                    entity.SaveChanges();
                    status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }

            ViewBag.Status = status;
            return View();
        }

        #endregion

        #region Login
        [HttpGet]
        public ActionResult Login()
        {
            //Speaker.Speak("Welcome to the Login Page");
            return View();
        }

        #endregion


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string returnUrl, Employer employerLogin)
        {
        
             string message = "";

            using (var entityEmployer = new JobAtYourFingerTipsEntities1())
            {
                if(employerLogin.Email == null)
                    employerLogin.Email = login.EmailAddress;
                var checkResultEmployer = entityEmployer.Employers.FirstOrDefault(x => x.Email == employerLogin.Email);
                if (checkResultEmployer != null)
                {
                    if (string.Compare(Crypto.Hash(login.Password), checkResultEmployer.Password) == 0)
                    {
                        var timeout = login.RememberMe ? 43800 : 20;
                        var ticket = new FormsAuthenticationTicket(login.EmailAddress, login.RememberMe, timeout);
                        var encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        return RedirectToAction("EmployerIndex", "EmployerUser");

                    }
                }

                using (var entity = new JobAtYourFingerTipsEntities())
                {
                    var chechResult = entity.Users.FirstOrDefault(x => x.EmailAddress == login.EmailAddress);
                    if (chechResult != null)
                    {
                        if (string.Compare(Crypto.Hash(login.Password), chechResult.Password) == 0)
                        {
                            var timeout = login.RememberMe ? 43800 : 20;
                            var ticket =
                                new FormsAuthenticationTicket(login.EmailAddress, login.RememberMe, timeout);
                            var encrypted = FormsAuthentication.Encrypt(ticket);
                            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                            cookie.Expires = DateTime.Now.AddMinutes(timeout);
                            cookie.HttpOnly = true;
                            Response.Cookies.Add(cookie);

                            if (Url.IsLocalUrl(returnUrl))
                            {
                                return Redirect(returnUrl);
                            }
                            return RedirectToAction("Index", "Home");
                        }
                        message = "The email address or password that you've entered doesn't match any account. Sign up for an account";

                    }
                    else
                    {
                        message = "The email address or password that you've entered doesn't match any account. Sign up for an account";
                    }


                }

            }

            
            ViewBag.Message = message;
            return View();
        }


            //Logout
            [Authorize]
            [HttpPost]
            public ActionResult Logout()
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "User");
            }

            [NonAction]
            public bool IsEmailExist(string email)
            {
                using (var entity = new JobAtYourFingerTipsEntities())
                {
                    var result = entity.Users.FirstOrDefault(x => x.EmailAddress == email);
                    return result != null;
                }
            }

            [NonAction]
            public void SendVerificationLinkEmail(string email, string activationCode,string emailFor = "VerifyAccount")
            {
                var verifyUrl = "/User/"+ emailFor +"/" + activationCode;
                if (Request.Url != null)
                {
                    var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

                    var fromEmail = new MailAddress("lehlohonolomashiyane@gmail.com","JobsAtYourFingerTips");
                    var toEmail = new MailAddress(email);
                    const string fromEmailPassword = "lubanzi123"; //replace with password
                    var subject = "";
                    var body = "";
                    if (emailFor == "VerifyAccount")
                    {
                        subject = "Your account has been successfully created";
                        body = "<br/><br/> Happy to tell you that you JobAtYouFingerTips account has been sucessfully created." +
                                   "Please click on the link below to activate your account<br/><br/><a href='" + link + "'>" + link + "</a>";
                    }
                    else if (emailFor == "ResetPassword")
                    {
                        subject = "Reset Password for account";
                        body = "<br/><br/> Reset password request has been received." +
                               "Please click on the link below to activate your account<br/><br/><a href=" + link + ">Reset Password Link</a>";
                    }
                    

                    var smtpClient = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromEmail.Address,fromEmailPassword)
                    };

                    using (var message = new MailMessage(fromEmail,toEmail)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    }) 
                        smtpClient.Send(message);
                }
            }
            [HttpGet]
            public ActionResult ForgotPassword()
            {
                return View();
            }

            [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            string message = "";
            bool status = false;
            using (var entity = new JobAtYourFingerTipsEntities1())
            {
                var account = entity.Employers.FirstOrDefault(x => x.Email == email);
                if (account != null)
                {
                    //Send email for reset password
                    string resetCode = Guid.NewGuid().ToString();
                    SendVerificationLinkEmail(account.Email, resetCode, "ResetPassword");
                    account.ResetPasswordCode = resetCode ;
                    entity.Configuration.ValidateOnSaveEnabled = false;
                    entity.SaveChanges();
                }
                else
                {
                    message = "Account not found";
                }
            }
            using (var entity = new JobAtYourFingerTipsEntities())
            {
                var account = entity.Users.FirstOrDefault(x => x.EmailAddress == email);
                if (account != null)
                {
                    //Send email for reset password
                    string resetCode = Guid.NewGuid().ToString();
                    SendVerificationLinkEmail(account.EmailAddress,resetCode,"ResetPassword");
                    account.ResetPasswordCode = resetCode;
                    entity.Configuration.ValidateOnSaveEnabled = false;
                    entity.SaveChanges();
                }
                else
                {
                    message = "Account not found";
                }
            }

            ViewBag.Message = message;
            return View();
        }

        public ActionResult ResetPassword(string id)
        {
            using (var entity = new JobAtYourFingerTipsEntities())
            {
                var user = entity.Users.FirstOrDefault(x => x.ResetPasswordCode == id);
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
               
            }
            using (var entity = new JobAtYourFingerTipsEntities1())
            {
                var user = entity.Employers.FirstOrDefault(x => x.ResetPasswordCode == id);
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [NonAction]
        public void SaySomething(string text)
        {
            var speech = new SpeechSynthesizer();
            speech.Speak(text);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (var entity = new JobAtYourFingerTipsEntities())
                {
                    var user = entity.Users.FirstOrDefault(x => x.ResetPasswordCode == model.ResetCode);
                    if (user != null)
                    {
                        user.Password = Crypto.Hash(model.NewPassword);
                        user.ResetPasswordCode = "";
                        entity.Configuration.ValidateOnSaveEnabled = false;
                        entity.SaveChanges();
                        message = "New password updated successfully";
                    }
                }
            }
            else
            {
                message = "Invalid";
            }

            ViewBag.Message = message;
            return View(model);
        }
    }
}