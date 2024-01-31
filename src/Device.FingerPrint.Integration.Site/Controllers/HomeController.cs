using Device.FingerPrint.Integration.Site.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;

namespace Device.FingerPrint.Integration.Site.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment hostingEnvironment;

        public HomeController(IWebHostEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult DownloadFile(string fileName)
        {
            string relativePath = "Upload/" + fileName;
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
