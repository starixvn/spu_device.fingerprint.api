using System.Web.Mvc;

namespace demoAPI.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return Redirect("Home");

        }
    }
}