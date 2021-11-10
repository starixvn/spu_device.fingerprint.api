using Demo.Models;
using Fps.Device.FingerPrint.Client;
using FPS.Device.FingerPrint.Model;
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
                fpsService.InsertTransaction("http://localhost:5003/api/fp/Transaction", new TransactionModel { 
                    LoginName = loginModel.Username,
                    SessionKey = HttpContext.Request.Form["__fk"],
                    SubscriberId = 1
                });

                return RedirectToAction("index", "home");
            }
            else
            {
                return View("Index");
            }
        }
    }
}
