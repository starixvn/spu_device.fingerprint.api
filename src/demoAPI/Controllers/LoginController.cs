using demoAPI.Models;
using FPS.Device.FingerPrint.LogTrail.Net.Standard.Client;
using FPS.Device.FingerPrint.Net.Framework.Client;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace demoAPI.Controllers
{
    public class LoginController : Controller
    {
        private string GetHeaderIP(string param)
        {
            var headerVal = Request.Headers[param]?.ToString();
            if (!string.IsNullOrEmpty(headerVal))
            {
                return headerVal;
            }

            return string.Empty;
        }

        private string GetXForWarded()
        {
            var proxyIP = GetHeaderIP("X-Forwarded-For");

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Originating-IP");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Remote-IP");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Remote-Addr");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = Request.UserHostAddress.ToString();
            }

            return proxyIP;
        }

        private string GetRealIp()
        {
            var proxyIP = GetHeaderIP("X-Actual-IP");

            if (string.IsNullOrEmpty(proxyIP))
            {
                var clientIps = GetXForWarded();

                if (!string.IsNullOrEmpty(clientIps))
                {
                    var ips = clientIps.Split(new char[] { ',' });

                    if (ips.Length > 1)
                    {
                        return ips[1];
                    }

                    return ips[0];
                }
                else
                {
                    return Request.UserHostAddress.ToString();
                }
            }
            else
            {
                return proxyIP.Split(new char[] { ',' })[0];
            }
        }

        private string GetFakeIp()
        {
            var proxyIP = GetXForWarded();

            if (!string.IsNullOrEmpty(proxyIP))
            {
                var ips = proxyIP.Split(new char[] { ',' });

                if (ips[0] != GetRealIp())
                {
                    return ips[0];
                }
            }

            return string.Empty;
        }

        // GET: Login
        public ActionResult Index()
        {
            if (Session["Username"] == null)
            {
                var model = new LoginModel {
                    RealIP = GetRealIp(),
                    FakeIP= GetFakeIp(),
                    XForwarded= GetXForWarded(),
                    XActualIP = GetHeaderIP("X-Actual-IP"),
                    Username = "test",
                    Password = "1234aa",
                };
                return View(model);
            }
            return Redirect("Home");
        }

        [HttpPost]
        public async Task<ActionResult> OnPostAsync(LoginModel loginModel)
        {
            if ((loginModel.Username == "test" || loginModel.Username == "test1" || loginModel.Username == "test2") && loginModel.Password == "1234aa")
            {
                Session["Username"] = loginModel.Username;

                FpsIntegration(loginModel.Username);

                var logEvent = new LogEventService("localhost:5008");
                var logEventTrail = new LogEventInfo
                {
                    EventDate = DateTime.Now
                };

                await logEvent.InsertLogEventAsync(logEventTrail);

                return RedirectToAction("index", "home");
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

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("");
        }

        public class LogEventInfo
        {
            public DateTime EventDate { get; set;}
        }
    }
}
