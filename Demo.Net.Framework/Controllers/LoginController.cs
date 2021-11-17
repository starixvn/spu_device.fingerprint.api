using Demo.Net.Framework.Models;
using Fps.Device.FingerPrint.Client.Net.Framework;
using System.Web.Mvc;

namespace Demo.Net.Framework.Controllers
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
        public ActionResult OnPost(LoginModel loginModel)
        {
            if (loginModel.Username == "test" && loginModel.Password == "1234aa")
            {                
                string url = "http://localhost:5003";
                fpsService.InsertTransaction(url, loginModel.Username, 2);

                return RedirectToAction("index", "home");
            }
            return View("Index");
        }
    }
}