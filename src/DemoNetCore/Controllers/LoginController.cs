using DemoNetCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DemoNetCore.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return View(new LoginModel());
            }
            return Redirect("Home");
        }

        [HttpPost]
        public ActionResult OnPost(LoginModel loginModel)
        {
            if ((loginModel.Username == "test" || loginModel.Username == "test1" || loginModel.Username == "test2") && loginModel.Password == "1234aa")
            {

                HttpContext.Session.SetString("Username", loginModel.Username);

                return RedirectToAction("index", "home");
            }

            return View("Index");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("");
        }
    }
}
