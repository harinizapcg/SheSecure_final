using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace SheSecure.Web.Controllers
{
    public class EmergencyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EmergencyController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient("SafetyService");
            var token = HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // GET — Employee SOS view
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            var client = GetClient();
            try
            {
                var response = await client.GetStringAsync("api/EmergencyAlert/all");
                ViewBag.Alerts = JsonDocument.Parse(response).RootElement;
            }
            catch
            {
                ViewBag.Alerts = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "SOS / Emergency";
            return View();
        }

        // GET — Security: all alerts
        public async Task<IActionResult> All()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("Role");
            if (role != "Security" && role != "Admin" && role != "Manager")
                return RedirectToAction("Index", "Dashboard");

            var client = GetClient();
            try
            {
                var response = await client.GetStringAsync("api/EmergencyAlert/all");
                ViewBag.Alerts = JsonDocument.Parse(response).RootElement;
            }
            catch
            {
                ViewBag.Alerts = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "All Emergency Alerts";
            return View();
        }

        // POST — Trigger SOS
        [HttpPost]
        public async Task<IActionResult> SOS(
            string location, string description, string severity)
        {
            var client = GetClient();
            var employeeId = HttpContext.Session.GetString("UserId") ?? "";

            var payload = JsonSerializer.Serialize(new
            {
                employeeId,
                location,
                description,
                severity
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/EmergencyAlert/create", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "🚨 SOS Alert triggered! Security has been notified.";
            else
                TempData["Error"] = "Failed to trigger SOS. Please call security directly.";

            return RedirectToAction("Index");
        }

        // POST — Resolve alert (Security)
        [HttpPost]
        public async Task<IActionResult> Resolve(int alertId)
        {
            var client = GetClient();
            var payload = JsonSerializer.Serialize(new { alertId });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PutAsync("api/EmergencyAlert/resolve", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Alert resolved successfully.";
            else
                TempData["Error"] = "Failed to resolve alert.";

            return RedirectToAction("All");
        }
    }
}