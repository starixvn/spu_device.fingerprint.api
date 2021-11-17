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

                    string url = "http://localhost:5003";

                    fpsService.InsertTransaction(url, Session["Username"].ToString(), 2, fpsSessionKey);

                    TempData["fpsSessionKey"] = Session["fpsSessionKey"] = fpsSessionKey;                    
                }

                return View();
            }            
        }
    }
}