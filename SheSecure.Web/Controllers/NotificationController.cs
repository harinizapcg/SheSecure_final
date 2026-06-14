using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace SheSecure.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public NotificationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient("NotificationService");
            var token = HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            var client = GetClient();
            var userId = HttpContext.Session.GetString("UserId") ?? "1";

            try
            {
                var response = await client.GetStringAsync($"api/Notification/by-employee/{userId}");
                ViewBag.Notifications = JsonDocument.Parse(response).RootElement;
            }
            catch
            {
                ViewBag.Notifications = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "Notifications";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = HttpContext.Session.GetString("UserId") ?? "1";
            var client = GetClient();
            try
            {
                var response = await client.GetStringAsync($"api/Notification/by-employee/{userId}");
                var notifications = JsonDocument.Parse(response).RootElement;
                var unread = notifications.EnumerateArray()
                    .Count(n => !n.GetProperty("isRead").GetBoolean());
                return Json(new { count = unread });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var client = GetClient();
            await client.PutAsync($"api/Notification/read/{id}", null);
            return RedirectToAction("Index");
        }
    }
}