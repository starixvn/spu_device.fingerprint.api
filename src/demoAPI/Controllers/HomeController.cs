using FPS.Device.FingerPrint.Net.Framework.Client;
using System;
using System.Configuration;
using System.Web.Mvc;

namespace demoAPI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["Username"] == null)
            {
                return Redirect("Login");
            }
            //try
            //{
            //    FpsIntegration(Session["Username"].ToString());
            //}
            //catch(Exception ex)
            //{
            //    return View(ex.Message);
            //}

            return View();
        }

        private void FpsIntegration(string loginName)
        {
            if (Session["IsPushTrans"] == null)
            {
                try
                {
                    var subscriberName = ConfigurationManager.AppSettings["subscriberName"].ToString();
                    var externalAPIUrl = ConfigurationManager.AppSettings["externalAPIUrl"].ToString();
                    var internalAPIUrl = ConfigurationManager.AppSettings["internalAPIUrl"].ToString();

                    var fpsService = new TransactionServices(subscriberName, externalAPIUrl, internalAPIUrl);

                    ViewBag.fpsScript = fpsService.Insert("Login", loginName, DateTime.Now);

                    Session["IsPushTrans"] = "Ok";
                }
                catch( Exception ex)
                {
                    ViewBag.fpsScript = ex.Message;
                }                
            }
        }
    }
}