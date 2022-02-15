using Fps.Device.FingerPrint.Net.Framework.Client;
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

            //FpsIntegration(Session["Username"].ToString());

            return View();
        }

        private void FpsIntegration(string loginName)
        {
            if (Session["IsPushTrans"] == null)
            {
                var subscriberId = Convert.ToInt32(ConfigurationManager.AppSettings["subscriberId"]);
                var apiUrl = ConfigurationManager.AppSettings["apiUrl"];
                var fpsService = new FpsService(subscriberId, apiUrl);

                ViewBag.fpsScript = fpsService.GetScriptUrl();                                

                fpsService.InsertTransaction("Login", loginName);

                Session["IsPushTrans"] = "Ok";
            }
        }
    }
}