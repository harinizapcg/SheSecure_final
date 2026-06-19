using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace SheSecure.Web.Controllers
{
    public class SafeReachController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SafeReachController> _logger;

        public SafeReachController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<SafeReachController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        // GET — show my check-ins + form
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            var client = GetClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var userId = HttpContext.Session.GetString("UserId") ?? "1";
            
            try
            {
                var response = await client.GetAsync($"{baseUrl}api/SafeReach/by-employee/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var contentString = await response.Content.ReadAsStringAsync();
                    var records = JsonDocument.Parse(contentString).RootElement;
                    ViewBag.Records = records;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                    ViewBag.Records = JsonDocument.Parse("[]").RootElement;
                    TempData["Error"] = "Unable to fetch records at this time.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Network error while fetching SafeReach records.");
                ViewBag.Records = JsonDocument.Parse("[]").RootElement;
                TempData["Error"] = "A network error occurred. Please try again later.";
            }

            ViewData["Title"] = "My Safe Check-In";
            return View();
        }

        // GET — HR/Admin: all safe check-ins
        public async Task<IActionResult> All()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("Role");
            if (role != "HR" && role != "Admin" && role != "Manager")
                return RedirectToAction("Index", "Dashboard");

            var client = GetClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            
            try
            {
                var response = await client.GetAsync($"{baseUrl}api/SafeReach/all");
                if (response.IsSuccessStatusCode)
                {
                    var contentString = await response.Content.ReadAsStringAsync();
                    var records = JsonDocument.Parse(contentString).RootElement;
                    ViewBag.Records = records;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                    ViewBag.Records = JsonDocument.Parse("[]").RootElement;
                    TempData["Error"] = "Unable to fetch records at this time.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Network error while fetching SafeReach records.");
                ViewBag.Records = JsonDocument.Parse("[]").RootElement;
                TempData["Error"] = "A network error occurred. Please try again later.";
            }

            ViewData["Title"] = "All Safe Check-Ins";
            return View();
        }

        // POST — create a new check-in
        [HttpPost]
        public async Task<IActionResult> Create(string expectedArrivalTime)
        {
            var client = GetClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var employeeId = HttpContext.Session.GetString("UserId") ?? "";

            var parsedDate = DateTime.Parse(expectedArrivalTime);
            var utcDate = parsedDate.Kind == DateTimeKind.Utc ? parsedDate : parsedDate.ToUniversalTime();

            var payload = JsonSerializer.Serialize(new
            {
                employeeId,
                expectedArrivalTime = utcDate.ToString("o")
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{baseUrl}api/SafeReach/create", content);

                if (response.IsSuccessStatusCode)
                    TempData["Success"] = "Safe check-in created successfully!";
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                    TempData["Error"] = "Failed to create check-in. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Network error during create.");
                TempData["Error"] = "Network error. Please try again later.";
            }

            return RedirectToAction("Index");
        }

        // POST — acknowledge arrival
        [HttpPost]
        public async Task<IActionResult> Acknowledge(int safeReachId)
        {
            var client = GetClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var payload = JsonSerializer.Serialize(new { safeReachId });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PutAsync($"{baseUrl}api/SafeReach/acknowledge", content);

                if (response.IsSuccessStatusCode)
                    TempData["Success"] = "Arrival acknowledged successfully!";
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                    TempData["Error"] = "Failed to acknowledge. Already acknowledged?";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Network error during acknowledge.");
                TempData["Error"] = "Network error. Please try again later.";
            }

            return RedirectToAction("Index");
        }

        // POST — escalate
        [HttpPost]
        public async Task<IActionResult> Escalate(int id)
        {
            var client = GetClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            try
            {
                var response = await client.PutAsync($"{baseUrl}api/SafeReach/escalate/{id}", null);

                if (response.IsSuccessStatusCode)
                    TempData["Success"] = "Escalated successfully!";
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                    TempData["Error"] = "Cannot escalate — employee may have already acknowledged.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Network error during escalate.");
                TempData["Error"] = "Network error. Please try again later.";
            }

            return RedirectToAction("Index");
        }
    }
}