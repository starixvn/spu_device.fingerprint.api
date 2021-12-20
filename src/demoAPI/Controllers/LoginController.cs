using demoAPI.Models;
using System.Web.Mvc;

namespace demoAPI.Controllers
{
    public class LoginController : Controller
    {       
        // GET: Login
        public ActionResult Index()
        {
            if (Session["Username"] == null)
            {
                return View();
            }
            return Redirect("Home");
        }

        [HttpPost]
        public ActionResult OnPost(LoginModel loginModel)
        {
            if ((loginModel.Username == "test" || loginModel.Username == "test1" || loginModel.Username == "test2") && loginModel.Password == "1234aa")
            {
                
                Session["Username"] = loginModel.Username;               

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