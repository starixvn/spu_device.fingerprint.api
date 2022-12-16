using demoAPI.Models;
using FPS.Device.FingerPrint.Net.Framework.Client;
using System;
using System.Configuration;
using System.Web.Mvc;

namespace demoAPI.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            if (Session["Username"] == null)
            {
                return View();
            }
            return Redirect("Home");
        }

        [HttpPost]
        public ActionResult OnPost(LoginModel loginModel)
        {
            if ((loginModel.Username == "test" || loginModel.Username == "test1" || loginModel.Username == "test2") && loginModel.Password == "1234aa")
            {
                Session["Username"] = loginModel.Username;

                ViewBag.UrlContext = Request.Url;
                ViewBag.UrlReferrerContext = Request.UrlReferrer;

                FpsIntegration(loginModel.Username);
                return View("Index");

                // return RedirectToAction("index", "home");
            }

            return View("Index");
        }

        private void FpsIntegration(string loginName)
        {
            try
            {

                var subscriberName = ConfigurationManager.AppSettings["subscriberName"].ToString();
                var internalAPIUrl = ConfigurationManager.AppSettings["internalAPIUrl"].ToString();

                var fpsService = new TransactionServices(subscriberName, internalAPIUrl);

                fpsService.Insert("Login", loginName, 0, 0, DateTime.Now);
            }
            catch (Exception ex)
            {
                ViewBag.fpsScript = ex.Message;
            }
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("");
        }
    }
}