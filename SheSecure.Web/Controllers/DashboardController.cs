using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace SheSecure.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("Role");
            if (role == "Security")
                return RedirectToAction("All", "Emergency");
            if (role == "Admin")
                return RedirectToAction("All", "Complaint");
            if (role == "Manager" && HttpContext.Session.GetString("ManagerMode") != "Personal")
                return RedirectToAction("Index", "Manager");
            if (role == "HR" && HttpContext.Session.GetString("HRMode") != "Personal")
                return RedirectToAction("Index", "HR");

            var client = _httpClientFactory.CreateClient("DashboardService");
            var token = HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Fetch all 4 endpoints in parallel
            var statsTask = client.GetStringAsync("api/Dashboard/stats");
            var complaintsTask = client.GetStringAsync("api/Dashboard/complaints");
            var wellnessTask = client.GetStringAsync("api/Dashboard/wellness");
            var emergencyTask = client.GetStringAsync("api/Dashboard/emergency");

            await Task.WhenAll(statsTask, complaintsTask, wellnessTask, emergencyTask);

            ViewBag.Stats = JsonDocument.Parse(statsTask.Result).RootElement;
            ViewBag.Complaints = JsonDocument.Parse(complaintsTask.Result).RootElement;
            ViewBag.Wellness = JsonDocument.Parse(wellnessTask.Result).RootElement;
            ViewBag.Emergency = JsonDocument.Parse(emergencyTask.Result).RootElement;

            ViewData["Title"] = "Dashboard";
            return View();
        }

        public IActionResult LearningHub()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");
            
            ViewData["Title"] = "Learning & Awareness Hub";
            return View();
        }
    }
}