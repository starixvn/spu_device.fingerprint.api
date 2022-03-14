using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using FPS.Device.FingerPrint.Net.Standard.Client;

namespace DemoNetCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration configuration;
        public HomeController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return Redirect("Login");
            }

            FpsIntegration(HttpContext.Session.GetString("Username"));

            return View();
        }

        private void FpsIntegration(string loginName)
        {
            if (HttpContext.Session.GetString("IsPushTrans") == null)
            {
                var subscriberName = configuration.GetSection("SubscriberName").Value.ToString();
                var apiUrl = configuration.GetSection("ApiUrl").Value.ToString();
                var fpsService = new TransactionServices(subscriberName, apiUrl);

                ViewBag.fpsScript = fpsService.GetScriptUrl();

                var userAgent = Request.Headers["User-Agent"].ToString();
                var hostUrl = Request.Scheme.ToString() + "://" + Request.Host.ToString();
                fpsService.Insert("Login", loginName, DateTime.Now, userAgent, hostUrl);

                HttpContext.Session.SetString("IsPushTrans", "Ok");
            }
        }
    }
}
