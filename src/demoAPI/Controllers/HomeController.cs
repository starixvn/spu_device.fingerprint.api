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

        
    }
}