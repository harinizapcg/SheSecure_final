using Microsoft.AspNetCore.Mvc;

using System.Text.Json;

namespace SheSecure.Web.Controllers
{
    public class ManagerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ManagerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "Manager")
                return RedirectToAction("Index", "Dashboard");

            ViewBag.HideSidebar = true;
            ViewData["Title"] = "Portal Selection";
            
            return View();
        }

        [HttpPost]
        public IActionResult SetMode(string mode)
        {
            if (HttpContext.Session.GetString("Role") != "Manager")
                return Unauthorized();

            if (mode == "Personal" || mode == "Leadership")
            {
                HttpContext.Session.SetString("ManagerMode", mode);

                if (mode == "Personal")
                    return RedirectToAction("Index", "Dashboard");
                else
                    return RedirectToAction("Leadership", "Manager");
            }

            return RedirectToAction("Index");
        }

        public IActionResult Leadership()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "Manager" || HttpContext.Session.GetString("ManagerMode") != "Leadership")
                return RedirectToAction("Index");

            ViewData["Title"] = "Team Leadership Portal";
            return View();
        }

        public async Task<IActionResult> SafeChecks()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "Manager" || HttpContext.Session.GetString("ManagerMode") != "Leadership")
                return RedirectToAction("Index");

            var client = _httpClientFactory.CreateClient("SafetyService");
            var token = HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.GetStringAsync("api/SafeReach/all");
                ViewBag.SafeChecks = JsonDocument.Parse(response).RootElement;
            }
            catch
            {
                ViewBag.SafeChecks = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "Team Safe Checks";
            return View();
        }

        public async Task<IActionResult> Directory()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "Manager" || HttpContext.Session.GetString("ManagerMode") != "Leadership")
                return RedirectToAction("Index");

            var client = _httpClientFactory.CreateClient("AuthService");
            var token = HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.GetStringAsync("api/Auth/users");
                ViewBag.Users = JsonDocument.Parse(response).RootElement;
            }
            catch
            {
                ViewBag.Users = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "Team Directory";
            return View();
        }
    }
}
