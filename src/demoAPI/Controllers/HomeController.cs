using Fps.Device.FingerPrint.Client.Net.Framework;
using System.Web.Mvc;

namespace demoAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFpsService fpsService;

        public HomeController()
        {
            fpsService = new FpsService();
        }

        public ActionResult Index()
        {
            if (Session["Username"] == null)
            {
                return Redirect("Login");
            }
            else
            {
                if (Session["fpsSessionKey"] == null)
                {
                    var fpsSessionKey = fpsService.GenerateSessionKey();

                    string url = System.Configuration.ConfigurationManager.AppSettings["apiUrl"];

                    fpsService.InsertTransaction(url, Session["Username"].ToString(), 2, fpsSessionKey);

                    TempData["fpsSessionKey"] = Session["fpsSessionKey"] = fpsSessionKey;
                }

                return View();
            }
        }
    }
}