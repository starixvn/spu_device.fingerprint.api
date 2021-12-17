using Device.FingerPrint.Integration.Site.Models;
using Fps.Device.FingerPrint.Features.Subscriber.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

namespace Device.FingerPrint.Integration.Site.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISubscriberServices subscriberServices;
        private readonly IAppSettings appSettings;

        public HomeController(ILogger<HomeController> logger, ISubscriberServices subscriberServices, IAppSettings appSettings)
        {
            _logger = logger;
            this.subscriberServices = subscriberServices;
            this.appSettings = appSettings;
        }

        public IActionResult Index()
        {
            //var subscribers = subscriberServices.GetSubscriber(2);
            ViewBag.SubscriberList = GetSubscriberList();
            return View();
        }

        [HttpPost]
        public IActionResult OnPost(string SubscriberId)
        {
            ViewBag.SubscriberList = GetSubscriberList();
            string strFrontEnd = @"<input type=""hidden"" id=""__sk"" name=""__sk"" value=""@Session[""fpsSessionKey""]"" />
<script async defer src=""@ViewBag.fpsScript"" type=""text / javascript""></script>";
            ViewBag.FrontEnd = strFrontEnd;
            ViewBag.BackEnd = @"private void FpsIntegration(string loginName)
    {
            if (Session[""fpsSessionKey""] == null)
            {
                var subscriberId = "+ SubscriberId + @";
                var apiUrl = """+ appSettings.ApiUrl + @""";
                var fpsService = new FpsService(subscriberId, apiUrl);

                ViewBag.fpsScript = fpsService.GetScriptUrl();

                var fpsSessionKey = fpsService.GenerateSessionKey();

                fpsService.InsertTransaction(loginName, fpsSessionKey);

                Session[""fpsSessionKey""] = fpsSessionKey;
            }
        }
            ";
            return View("Index");
        }

        private SelectList GetSubscriberList()
        {
            List<SubscriberModel> subscriberList = new List<SubscriberModel>();
            subscriberList.Add(new SubscriberModel { SubscriberId = 1, SubscriberName = "Subscriber 1" });
            subscriberList.Add(new SubscriberModel { SubscriberId = 2, SubscriberName = "Subscriber 2" });
            SelectList selectList = new SelectList(subscriberList, "SubscriberId", "SubscriberName");
            return selectList;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
