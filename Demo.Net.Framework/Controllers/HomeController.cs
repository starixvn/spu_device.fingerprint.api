using System.Web.Mvc;

namespace Demo.Net.Framework.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}