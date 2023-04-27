using System.Web.Mvc;

namespace demoAPI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["Username"] == null)
            {
                return Redirect("Login");
            }

            return View();
        }
    }
}