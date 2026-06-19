using Microsoft.AspNetCore.Mvc;

namespace SheSecure.Web.Controllers
{
    public class HRController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Index", "Dashboard");

            ViewBag.HideSidebar = true;
            ViewData["Title"] = "Portal Selection";
            
            return View();
        }

        [HttpPost]
        public IActionResult SetMode(string mode)
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return Unauthorized();

            if (mode == "Personal" || mode == "Administration")
            {
                HttpContext.Session.SetString("HRMode", mode);

                if (mode == "Personal")
                    return RedirectToAction("Index", "Dashboard");
                else
                    return RedirectToAction("Administration", "HR");
            }

            return RedirectToAction("Index");
        }

        public IActionResult Administration()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "HR" || HttpContext.Session.GetString("HRMode") != "Administration")
                return RedirectToAction("Index");

            ViewData["Title"] = "HR Administration Portal";
            return View();
        }
    }
}
