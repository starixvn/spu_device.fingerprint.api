using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            

            _logger = logger;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("username") != null)
            {
                return Redirect("login");
            }
            else
            {
                if (HttpContext.Session.GetString("fpsSessionKey") == null)
                {
                    //var fpsSessionKey = fpsService.GenerateSessionKey();

                    //string url = "http://localhost:5003";

                    //fpsService.InsertTransaction(url, Session["Username"].ToString(), 2, fpsSessionKey);

                    //TempData["fpsSessionKey"] = Session["fpsSessionKey"] = fpsSessionKey;
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
