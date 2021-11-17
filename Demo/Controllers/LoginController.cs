using Demo.Models;
using Fps.Device.FingerPrint.Client.Net.Core;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    public class LoginController : Controller
    {
        private readonly IFpsService fpsService;
        
        public LoginController()
        {
            fpsService = new FpsService();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult OnPost(LoginModel loginModel)
        {
            if (loginModel.Username == "test" && loginModel.Password == "1234aa")
            {
                int SubscriberId = 1;
                string url = "http://localhost:5003";
                fpsService.InsertTransaction(HttpContext, url, loginModel.Username, SubscriberId);

                return RedirectToAction("index", "home");
            }
            return View("Index");
        }
    }
}
