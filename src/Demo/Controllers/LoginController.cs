using Demo.Models;
using Fps.Device.FingerPrint.Client.Net.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
        public IActionResult OnPost(LoginModel loginModel, int SubscriberId = 2)
        {
            if (loginModel.Username == "test" && loginModel.Password == "1234aa")
            {
                HttpContext.Session.SetString("username", loginModel.Username);

                return RedirectToAction("index", "home");
            }
            return View("Index");
        }
    }
}
