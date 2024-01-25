using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Integration.Site.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.Text;

namespace Device.FingerPrint.Integration.Site.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly ISubscriberServices subscriberServices;
        private readonly IAppSettings appSettings;
        
        private readonly SelectList _subscriberList;
        private readonly List<SelectListItem> _environments;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostingEnvironment, ISubscriberServices subscriberServices, IAppSettings appSettings)
        {
            _logger = logger;
            this.hostingEnvironment = hostingEnvironment;
            this.subscriberServices = subscriberServices;
            this.appSettings = appSettings;

            _subscriberList = GetSubscriberList();
            _environments = GetEnvironmentList();
        }

        public IActionResult Index()
        {
            ViewBag.SubscriberList = _subscriberList;
            ViewBag.Environments = _environments;
            return View();
        }

        private SelectList GetSubscriberList()
        {
            var subscribers = subscriberServices.GetSubscriberList();
            var selectList = new SelectList(subscribers, "SubscriberId", "SubscriberName");
            return selectList;
        }

        public List<SelectListItem> GetEnvironmentList()
        {
            // Logic để lấy danh sách môi trường từ cơ sở dữ liệu hoặc bất kỳ nguồn dữ liệu nào khác
            var environments = new List<SelectListItem>
            {
                new() { Text = "PROD", Value = "PROD" },
                new() { Text = "UAT", Value = "UAT" }
            };

            return environments;
        }

        [HttpPost]
        public IActionResult OnPost(string SubscriberId, string Environment)
        {
            ViewBag.SubscriberList = _subscriberList;
            ViewBag.Environments = _environments;

            ViewBag.FrontEndProd = GetScriptFrontEndProd();
            ViewBag.FrontEndUat = GetScriptFrontEndUat();

            ViewBag.BackEndNetStandard = GetScriptBackEndNetStandard();
            ViewBag.BackEndNetCore = GetScriptBackEndNetCore();
            ViewBag.BackEndNetFramework = GetScriptBackEndNetFramework();
            return View("Index");
        }


        private string GetScriptFrontEndProd()
        {
            string strResult = @"<script type=""text/javascript"" src=""//sc.detecas.com/di/activator.ashx"" async="""" defer=""""></script>";
            return strResult;
        }

        private string GetScriptFrontEndUat()
        {
            string strResult = @"<script type=""text/javascript"" src=""//scqa2.detecas.com/activator.ashx"" async="""" defer=""""></script>";
            return strResult;
        }


        private string GetScriptBackEndNetStandard()
        {
            string strResult = @"// Install/Update new SDK (FPS.Client.NetStandard)
// Change to new code
<a href=""@Url.Action(""DownloadScriptFile"", ""Download"")"">Download Script</a>
var fpsClient = new FPS.Client.NetStandard.Service();

fpsClient.InitialRequest({fpsAPI}, {httpRequest})
         .CheckTransaction({loginName}, {subscriberName}, {action}, {loginTime});";
            return strResult;
        }

        private string GetScriptBackEndNetCore()
        {
            string strResult = @"// Install/Update new SDK (FPS.Client.NetCore)
// Change to new code

var fpsClient = new FPS.Client.NetCore.Service();

fpsClient.InitialRequest({fpsAPI}, {httpRequest})
        .CheckTransaction({loginName}, {subscriberName}, {action}, {loginTime});";
            return strResult;
        }

        private string GetScriptBackEndNetFramework()
        {
            string strResult = @"// Install/Update new SDK (FPS.Client)
// Change to new code

var fpsClient = new FPS.Client.Service();

fpsClient.InitialRequest({fpsAPI}, {httpRequest})
         .CheckTransaction({loginName}, {subscriberName}, {action}, {loginTime});";
            return strResult;
        }

        public IActionResult DownloadScriptFile()
        {
            string relativePath = "Upload/FPS.Client.zip";
            string filePath = Path.Combine(hostingEnvironment.WebRootPath, relativePath);

            if (System.IO.File.Exists(filePath))
            {
                string fileExtension = Path.GetExtension(filePath);
                string contentType = GetContentType(fileExtension);

                return PhysicalFile(filePath, contentType, Path.GetFileName(filePath));
            }
            else
            {
                return Content("File does not exist.");
            }
        }

        private string GetContentType(string fileExtension)
        {
            switch (fileExtension)
            {
                case ".txt":
                    return "text/plain";
                case ".pdf":
                    return "application/pdf";
                case ".zip":
                    return "application/zip";
                // Thêm các trường hợp khác tùy thuộc vào định dạng tệp của bạn
                default:
                    return "application/octet-stream";
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
