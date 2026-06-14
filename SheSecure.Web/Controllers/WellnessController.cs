using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace SheSecure.Web.Controllers
{
    public class WellnessController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WellnessController(IHttpClientFactory httpClientFactory)
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

        private string GetEmployeeId()
        {
            // Use UserName (email) as employeeId since Mood API uses string IDs
            return HttpContext.Session.GetString("UserName") ?? "EMP001";
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            var client = GetClient();
            var employeeId = GetEmployeeId();

            try
            {
                var recordsTask = client.GetStringAsync($"api/Mood/employee/{employeeId}");
                var burnoutTask = client.GetStringAsync($"api/Mood/burnout-risk/{employeeId}");
                var trendTask = client.GetStringAsync($"api/Mood/trend/{employeeId}");
                var recommendationTask = client.GetStringAsync($"api/Mood/recommendation/{employeeId}");

                await Task.WhenAll(recordsTask, burnoutTask, trendTask, recommendationTask);

                ViewBag.Records = JsonDocument.Parse(recordsTask.Result).RootElement;
                ViewBag.Burnout = JsonDocument.Parse(burnoutTask.Result).RootElement;
                ViewBag.Trend = JsonDocument.Parse(trendTask.Result).RootElement;
                ViewBag.Recommendation = JsonDocument.Parse(recommendationTask.Result).RootElement;
                ViewBag.EmployeeId = employeeId;
            }
            catch
            {
                ViewBag.Records = JsonDocument.Parse("[]").RootElement;
                ViewBag.Burnout = JsonDocument.Parse("{}").RootElement;
                ViewBag.Trend = JsonDocument.Parse("[]").RootElement;
                ViewBag.Recommendation = JsonDocument.Parse("{}").RootElement;
                ViewBag.EmployeeId = employeeId;
            }

            ViewData["Title"] = "Mood & Wellness";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int moodLevel, int stressLevel, string remarks)
        {
            var client = GetClient();
            var employeeId = GetEmployeeId();

            var payload = JsonSerializer.Serialize(new
            {
                employeeId,
                moodLevel,
                stressLevel,
                remarks
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Mood/create", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Mood logged successfully!";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to log mood: {error}";
            }

            return RedirectToAction("Index");
        }
    }
}