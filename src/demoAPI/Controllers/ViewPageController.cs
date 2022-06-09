using System.Web.Mvc;

namespace demoAPI.Controllers
{
    public class ViewPageController : Controller
    {
        public ActionResult ViewPage1()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        public ActionResult ViewPage2()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        public ActionResult ViewPage3()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        public ActionResult ViewPage4()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        public ActionResult ViewPage5()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }
    }
}