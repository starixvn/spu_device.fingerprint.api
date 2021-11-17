using demoAPI.Models;
using Fps.Device.FingerPrint.Client.Net.Framework;
using System.Web.Mvc;

namespace demoAPI.Controllers
{
    public class LoginController : Controller
    {
        private readonly IFpsService fpsService;

        public LoginController()
        {
            fpsService = new FpsService();
        }
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult OnPost(LoginModel loginModel, int SubscriberId = 2)
        {
            if (loginModel.Username == "test" && loginModel.Password == "1234aa")
            {
                Session["Username"] = loginModel.Username;
                string url = "http://localhost:5003/api/fp/Transaction";
                fpsService.InsertTransaction(url, loginModel.Username, SubscriberId);

                return RedirectToAction("index", "home");
            }
            return View("Index");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("");

        }
    }
}