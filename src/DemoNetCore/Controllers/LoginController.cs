using DemoNetCore.Models;
using FPS.Device.FingerPrint.Net.Standard.Client;
using FPS.Device.FingerPrint.Net.Standard.Client.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace DemoNetCore.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly string subscriberName;
        private readonly string getAPIUrl;
        private readonly string postAPIUrl;

        public LoginController(IConfiguration configuration)
        {
            this.configuration = configuration;

            subscriberName = configuration.GetSection("SubscriberName").Value ?? string.Empty;
            getAPIUrl = configuration.GetSection("GetAPIUrl").Value ?? string.Empty;
            postAPIUrl = configuration.GetSection("PostAPIUrl").Value ?? string.Empty;
        }
        // GET: Login
        public ActionResult Index()
        {
            ViewBag.GetAPIUrl = String.Format("{0}/api/scr/{1}", string.IsNullOrEmpty(getAPIUrl) ? string.Empty : getAPIUrl, subscriberName);

            if (HttpContext.Session.GetString("Username") == null)
            {
                return View(new LoginModel());
            }
            return Redirect("Home");
        }

        [HttpPost]
        public ActionResult OnPost(LoginModel loginModel)
        {
            if ((loginModel.Username == "test" || loginModel.Username == "test1" || loginModel.Username == "test2") && loginModel.Password == "1234aa")
            {

                HttpContext.Session.SetString("Username", loginModel.Username);
                var fpsService = new TransactionServices(subscriberName, postAPIUrl);

                fpsService.CheckTransaction(new TransactionModel
                {
                    SessionKey = HttpContext.Request.Form["__fdi"].ToString(),
                    Action = "Login",
                    CustId = 0,
                    LoginName = loginModel.Username,
                    LoginTime = DateTime.Now,
                    SubAccId = 0,
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                    Url = HttpContext.Request.GetDisplayUrl(),
                    ClientIp = HttpContext.Connection.RemoteIpAddress.ToString()
                });

                return RedirectToAction("index", "home");
            }

            return View("Index");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("");
        }
    }
}
