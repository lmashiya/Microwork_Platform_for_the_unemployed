﻿using Microwork_Platform_for_the_unemployed.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;

namespace Microwork_Platform_for_the_unemployed.Controllers
{
    public class UserController : Controller
    {
      //Registration action
      [HttpGet]
      public ActionResult Registration()
      {
          return View();
      }
        //Registration Post action

        #region Registration

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
            return View();
        }

        #endregion

        #region Login Post

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string ReturnUrl)
        {
            string message = "";

            ViewBag.Message = message;
            using (var entity = new JobAtYourFingerTipsEntities())
            {
                var chechResult = entity.Users.FirstOrDefault(x => x.EmailAddress == login.EmailAddress);
                if (chechResult != null)
                {
                    if (string.Compare(Crypto.Hash(login.Password), chechResult.Password) == 0)
                    {
                        var timeout = login.RememberMe ? 43800 : 20;
                        var ticket = new FormsAuthenticationTicket(login.EmailAddress, login.RememberMe, timeout);
                        var encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        return RedirectToAction("Index","Home");
                    }
                    message = "Invalid login details";
                }
                else
                {
                    message = "Invalid login details";
                }

                ViewBag.Message = message;
                return View();
            }
        }

        #endregion

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
            public void SendVerificationLinkEmail(string email, string activationCode)
            {
                var verifyUrl = "/User/VerifyAccount/" + activationCode;
                if (Request.Url != null)
                {
                    var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

                    var fromEmail = new MailAddress("lehlohonolomashiyane@gmail.com","JobsAtYourFingerTips");
                    var toEmail = new MailAddress(email);
                    const string fromEmailPassword = "lubanzi123"; //replace with password
                    const string subject = "Your account has been successfully created";
                    var body = "<br/><br/> Happy to tell you that you JobAtYouFingerTips account has been sucessfully created." +
                               "Please click on the link below to activate your account<a href='"+link+"'>"+link+"</a>";

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
    }
}