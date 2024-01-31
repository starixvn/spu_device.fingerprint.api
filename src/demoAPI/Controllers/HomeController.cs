using System.IO;
using System.Web.Mvc;

namespace demoAPI.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DownloadFile(string fileName)
        {
            string relativePath = "Upload/" + fileName;
            string filePath = Path.Combine(Server.MapPath("~/Content/"), relativePath);

            if (System.IO.File.Exists(filePath))
            {
                string fileExtension = Path.GetExtension(filePath);
                string contentType = GetContentType(fileExtension);

                return File(filePath, contentType, Path.GetFileName(filePath));
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
    }
}