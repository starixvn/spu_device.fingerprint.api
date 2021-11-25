using Fps.Device.FingerPrint.Client.Net.Framework;
using System;
using System.Configuration;
using System.Web.Mvc;

namespace demoAPI.Controllers
{
    public class HomeController : Controller
    {
        
        public HomeController()
        {
            
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
                    var subscriperId = int.Parse(ConfigurationManager.AppSettings["subscriberId"]);

                    var apiUrl = ConfigurationManager.AppSettings["apiUrl"];

                    var fpsService = new FpsService(subscriperId, apiUrl);

                    ViewBag.fpsScript = fpsService.GetScriptUrl();

                    var fpsSessionKey = fpsService.GenerateSessionKey();

                    fpsService.InsertTransaction(Session["Username"].ToString(), fpsSessionKey);

                    TempData["fpsSessionKey"] = Session["fpsSessionKey"] = fpsSessionKey;
                }

                return View();
            }
        }
    }
}