using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Microwork_Platform_for_the_unemployed.Models;

namespace Microwork_Platform_for_the_unemployed.Controllers
{
    public class EmployerUserController : Controller
    {
       // GET: EmployerUser
       [HttpGet]
        public ActionResult EmployerRegistration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EmployerRegistration([Bind(Exclude = "RegistrationDate,IsEmailVerified,ActivationCode,JobPosts")] Employer user)
        {
            var status = false;
            var message = "";
            //Model Validation
            if (ModelState.IsValid)
            {
                #region check if email exists
                var isExists = IsEmailExist(user.Email);
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
                user.RegisterDate = DateTime.Today;
                #endregion

                #region Password hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                #endregion

                user.IsEmailVerified = false;

                #region Save To Database

                using (var entity = new JobAtYourFingerTipsEntities1())
                {
                    entity.Employers.Add(user);
                    entity.SaveChanges();
                }

                #region Send Email To New User

                SendVerificationLinkEmail(user.Email, user.ActivationCode.ToString());
                message = "Registration successfully created. Account sent to your email address : " +
                          user.Email;
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

        [HttpGet]
        public ActionResult EmployerVerifyAccount(string id)
        {
            bool status = false;
            using (var entity = new JobAtYourFingerTipsEntities1())
            {
                entity.Configuration.ValidateOnSaveEnabled = false;
                var codeChecker = entity.Employers.FirstOrDefault(x => x.ActivationCode == new Guid(id));
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

        [NonAction]
        public bool IsEmailExist(string email)
        {
            using (var entity = new JobAtYourFingerTipsEntities1())
            {
                var result = entity.Employers.FirstOrDefault(x => x.Email == email);
                return result != null;
            }
        }

        public void SendVerificationLinkEmail(string email, string activationCode)
        {
            var verifyUrl = "/EmployerUser/EmployerVerifyAccount/" + activationCode;
            if (Request.Url != null)
            {
                var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

                var fromEmail = new MailAddress("lehlohonolomashiyane@gmail.com", "JobsAtYourFingerTips");
                var toEmail = new MailAddress(email);
                const string fromEmailPassword = "lubanzi123"; //replace with password
                const string subject = "Your account has been successfully created";
                var body = "<br/><br/> Happy to tell you that you JobAtYouFingerTips account has been sucessfully created." +
                           "Please click on the link below to activate your account<br/><br/><a href='" + link + "'>" + link + "</a>";

                var smtpClient = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
                };

                using (var message = new MailMessage(fromEmail, toEmail)
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