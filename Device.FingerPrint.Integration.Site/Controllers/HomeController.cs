using Device.FingerPrint.Integration.Site.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using System.Collections.Generic;
using System.Diagnostics;

namespace Device.FingerPrint.Integration.Site.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.SubscriberList = GetSubscriberList();
            return View();
        }

        [HttpPost]
        public IActionResult OnPost(string Id)
        {
            ViewBag.SubscriberList = GetSubscriberList();
            string strFrontEnd = @"<input type=""hidden"" id=""__sk"" name=""__sk"" value=""@Session[""fpsSessionKey""]"" />
<script async defer src=""@ViewBag.fpsScript"" type=""text / javascript""></script>";
            ViewBag.FrontEnd = strFrontEnd;
            ViewBag.BackEnd = @"private void FpsIntegration(string loginName)
        {
                if (Session[""fpsSessionKey""] == null)
                {
                    var subscriberId = "+ Id + @";
                    var apiUrl = https://spu-fps-api-base.nexdev.net;
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
            List<SubscriberModel> MyList = new List<SubscriberModel>();
            MyList.Add(new SubscriberModel { Id = 1, Name = "Subscriber 1" });
            MyList.Add(new SubscriberModel { Id = 2, Name = "Subscriber 2" });
            SelectList SubscriberList = new SelectList(MyList, "Id", "Name");
            return SubscriberList;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
