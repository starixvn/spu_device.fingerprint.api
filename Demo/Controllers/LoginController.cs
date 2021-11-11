using Demo.Models;
using Fps.Device.FingerPrint.Client;
using Fps.Device.FingerPrint.Client.Net.Core;
using Fps.Device.FingerPrint.Client.Net.Framework;
using FPS.Device.FingerPrint.Model;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    public class LoginController : Controller
    {
        private readonly IFpsService fpsService;
        private readonly IFpsServiceFramework fpsServiceFramework;
        private readonly IFpsServiceCore fpsServiceCore;
        
        public LoginController()
        {
            fpsService = new FpsService();
            fpsServiceFramework = new FpsServiceFramework();
            fpsServiceCore = new FpsServiceCore();
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
                fpsService.InsertTransaction("http://localhost:5003/api/fp/Transaction", new TransactionModel
                {
                    LoginName = loginModel.Username,
                    SessionKey = HttpContext.Request.Form["__fk"],
                    SubscriberId = 1
                });
                //HttpContext.Items["SubscriberId"] = 1;
                //fpsServiceFramework.InsertTransaction("http://localhost:5003/api/fp/Transaction", HttpContext);

                return RedirectToAction("index", "home");
            }
            return View("Index");
        }
    }
}
