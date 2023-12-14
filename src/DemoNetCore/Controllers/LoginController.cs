using DemoNetCore.Models;
using Microsoft.AspNetCore.Http;
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
        private readonly string getActivatorUrl;

        public LoginController(IConfiguration configuration)
        {
            this.configuration = configuration;

            subscriberName = configuration.GetSection("SubscriberName").Value ?? string.Empty;
            getAPIUrl = configuration.GetSection("GetAPIUrl").Value ?? string.Empty;
            postAPIUrl = configuration.GetSection("PostAPIUrl").Value ?? string.Empty;
            getActivatorUrl = configuration.GetSection("DIUrl").Value + "/activator.ashx" ?? "//fps-di.nexdev.net/activator.ashx";
        }
        // GET: Login
        public ActionResult Index()
        {
            ViewBag.GetAPIUrl = String.Format("{0}/api/s/{1}", string.IsNullOrEmpty(getAPIUrl) ? string.Empty : getAPIUrl, subscriberName);
            ViewBag.GetActivatorUrl = getActivatorUrl;

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

                FpsIntegrationVer115(loginModel.Username, "Login", "Demo Page Login", subscriberName, DateTime.Now);
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


        private void FpsIntegrationVer115(string account, string action, string actionResult, string subscriber, DateTime loginTime)
        {
            var fpsAPI = configuration.GetSection("FPS_API").Value;
            var fpsService = new FPS.Client.NetStandard.Service(); // (fpsAPI, HttpContext.Request);
            fpsService.InitialRequest(fpsAPI, HttpContext.Request).CheckTransaction(account, subscriber, action, loginTime);
        }

        private string GetHeaderIP(string param)
        {
            var headerVal = HttpContext.Request.Headers[param].ToString();
            if (!string.IsNullOrEmpty(headerVal))
            {
                return headerVal;
            }

            return string.Empty;
        }

        private string GetXForWarded()
        {
            var proxyIP = GetHeaderIP("X-Forwarded-For");

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Originating-IP");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Remote-IP");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Remote-Addr");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = HttpContext.Request.Host.ToString();
            }

            return proxyIP;
        }

        private string GetRealIp()
        {
            var proxyIP = GetHeaderIP("X-Actual-IP");

            if (string.IsNullOrEmpty(proxyIP))
            {
                var clientIps = GetXForWarded();

                if (!string.IsNullOrEmpty(clientIps))
                {
                    var ips = clientIps.Split(new char[] { ',' });

                    if (ips.Length > 1)
                    {
                        return ips[1];
                    }

                    return ips[0];
                }
                else
                {
                    return HttpContext.Request.Host.ToString();
                }
            }
            else
            {
                return proxyIP.Split(new char[] { ',' })[0];
            }
        }

        private string GetFakeIp()
        {
            var proxyIP = GetXForWarded();

            if (!string.IsNullOrEmpty(proxyIP))
            {
                var ips = proxyIP.Split(new char[] { ',' });

                if (ips[0] != GetRealIp())
                {
                    return ips[0];
                }
            }

            return string.Empty;
        }
    }
}
