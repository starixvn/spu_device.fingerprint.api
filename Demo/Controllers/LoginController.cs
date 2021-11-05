using Demo.Models;
using FPS.Device.FingerPrint.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Demo.Controllers
{
    public class LoginController : Controller
    {
        private readonly IFpsActivator fpsService;

        public LoginController()
        {
            fpsService = new FpsActivator(1);
        }

        public IActionResult Index()
        {
            var fpsKey = fpsService.GenerateKey();

            TempData["fpsKey"] = fpsKey;

            return View(new LoginModel
            {
                FpsKey = fpsKey
            });
        }

        [HttpPost]
        public IActionResult OnPost(LoginModel loginModel)
        {
            if (loginModel.Username == "test" && loginModel.Password == "1234aa")
            {
                var fpsKey = TempData["fpsKey"];

                fpsService.PushTransaction("http://localhost:5000/api/fp/Transaction", loginModel.Username, fpsKey.ToString());

                return RedirectToAction("index", "home");
            }
            else
            {
                return View("Index");
            }
        }
    }
}
