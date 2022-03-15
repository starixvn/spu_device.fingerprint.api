using FPS.Device.FingerPrint.Net.Framework.Client;
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

            FpsIntegration(Session["Username"].ToString());

            return View();
        }

        private void FpsIntegration(string loginName)
        {
            if (Session["IsPushTrans"] == null)
            {
                var subscriberName = ConfigurationManager.AppSettings["subscriberName"].ToString();
                var apiUrl = ConfigurationManager.AppSettings["apiUrl"];
                var fpsService = new TransactionServices(subscriberName, apiUrl);

                ViewBag.fpsScript = fpsService.GetScriptUrl();

                fpsService.Insert("Login", loginName, DateTime.Now);

                Session["IsPushTrans"] = "Ok";
            }
        }
    }
}