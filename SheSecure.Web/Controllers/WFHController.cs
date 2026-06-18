using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace SheSecure.Web.Controllers
{
    public class WFHController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WFHController(IHttpClientFactory httpClientFactory)
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

        // GET — Employee WFH requests

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            var employeeId = HttpContext.Session.GetString("UserId") ?? "";

            // ✅ Add this temporarily to debug
            ViewBag.DebugEmployeeId = employeeId;

            var client = GetClient();
            try
            {
                var response = await client.GetStringAsync($"api/WellnessRequest/by-employee/{employeeId}");
                ViewBag.Requests = JsonDocument.Parse(response).RootElement;
            }
            catch
            {
                ViewBag.Requests = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "Wellness Requests";
            return View();
        }

        // POST — Create WFH request
        [HttpPost]
        public async Task<IActionResult> Create(
            string requestType, string description, string priority)
        {
            var client = GetClient();
            var employeeId = HttpContext.Session.GetString("UserId") ?? "";

            var payload = JsonSerializer.Serialize(new
            {
                employeeId,
                requestType,
                description,
                priority
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/WellnessRequest/create", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Wellness request submitted successfully!";
            else
                TempData["Error"] = "Failed to submit request. Please try again.";

            return RedirectToAction("Index");
        }

        // GET — Manager approvals view
        public async Task<IActionResult> Approvals()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin")
                return RedirectToAction("Index", "Dashboard");

            var client = GetClient();

            try
            {
                var response = await client.GetStringAsync("api/WellnessRequest/all");
                ViewBag.Requests = JsonDocument.Parse(response).RootElement;
            }
            catch
            {
                ViewBag.Requests = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "Wellness Approvals";
            return View();
        }

        // POST — Update status (approve/reject)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int requestId, string status)
        {
            var client = GetClient();
            var payload = JsonSerializer.Serialize(new { requestId, status });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PutAsync("api/WellnessRequest/status", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = $"Request {status} successfully!";
            else
                TempData["Error"] = "Failed to update status.";

            return RedirectToAction("Approvals");
        }
    }
}