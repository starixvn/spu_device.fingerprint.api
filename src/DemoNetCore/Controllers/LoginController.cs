using DemoNetCore.Models;
using FPS.Client.NetStandard.Models;
using FPS.Device.FingerPrint.Net.Standard.Client;
using FPS.Device.FingerPrint.Net.Standard.Client.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using static System.Collections.Specialized.BitVector32;

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

                var loginTime = DateTime.Now;
                fpsService.CheckTransaction(new TransactionModel
                {
                    SessionKey = HttpContext.Request.Form["__fdi"].ToString(),
                    Action = "Login",
                    CustId = 0,
                    LoginName = loginModel.Username,
                    LoginTime = loginTime,
                    SubAccId = 0,
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                    Url = HttpContext.Request.GetDisplayUrl(),
                    ClientIp = HttpContext.Connection.RemoteIpAddress.ToString()
                });


                FpsIntegration102(loginModel.Username, "Login", string.Empty, subscriberName, loginTime);

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


        private void FpsIntegration102(string account, string action, string actionResult, string subscriber, DateTime loginTime)
        {
            var fpsAPI = configuration.GetSection("FPS_API").Value;
            var fpsService = new FPS.Client.NetStandard.Service(fpsAPI);

            var ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            var requestIP = ip.Equals("::1") ? "127.0.0.1" : ip.Split(',')[0];

            TimeSpan diff = DateTime.UtcNow - DateTime.Now;

            var httpRequestData = new HttpRequestData()
            {
                AcceptTypes = Request.Headers["Accept"].ToString() ?? string.Empty,
                UserAgent = Request.Headers["User-Agent"].ToString() ?? string.Empty,
                TotalMinutes = diff.TotalMinutes.ToString(),
                RequestIP = requestIP,
                Di = Request.Form["__di"].ToString(),
                Token = Request.Form["__tk"].ToString(),
                Url = Request.Scheme.ToString() + "://" + Request.Host.ToString(),
                DebugInfo = Request.Form["detecas-analysis"].ToString(),
                CaptchaResult = Request.Form["captcha_result"].ToString()
            };

            fpsService.CheckTransaction(httpRequestData, account, action, actionResult, subscriber, loginTime);
        }
    }
}
