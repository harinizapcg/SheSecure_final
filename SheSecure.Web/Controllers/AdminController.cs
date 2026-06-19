using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace SheSecure.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient("AuthService");
            var token = HttpContext.Session.GetString("Token");
            if (token != null)
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        private async Task LogAuditAsync(string action, string entity, string details)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SafetyService");
                var token = HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var log = new
                {
                    userId = HttpContext.Session.GetString("UserName") ?? "Admin",
                    action = action,
                    entity = entity,
                    details = details
                };

                var payload = JsonSerializer.Serialize(log);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                await client.PostAsync("api/Audit", content);
            }
            catch
            {
                // Ignore audit log failures to not break the main flow
            }
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Dashboard");

            var client = GetClient();
            try
            {
                var response = await client.GetStringAsync("api/Auth/users");
                ViewBag.Users = JsonDocument.Parse(response).RootElement;
            }
            catch
            {
                ViewBag.Users = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "User Management";
            return View();
        }

        // POST: /Admin/ChangeRole
        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            var client = GetClient();
            var payload = JsonSerializer.Serialize(new { userId, newRole });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Auth/change-role", content);

            if (response.IsSuccessStatusCode)
            {
                await LogAuditAsync("Change Role", "User", $"Changed role of user {userId} to {newRole}");
                TempData["Success"] = "User role updated successfully.";
            }
            else
                TempData["Error"] = "Failed to update user role.";

            return RedirectToAction("Users");
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            var client = GetClient();
            var response = await client.DeleteAsync($"api/Auth/delete/{userId}");

            if (response.IsSuccessStatusCode)
            {
                await LogAuditAsync("Delete User", "User", $"Deleted user {userId}");
                TempData["Success"] = "User deleted successfully.";
            }
            else
                TempData["Error"] = "Failed to delete user.";

            return RedirectToAction("Users");
        }

        // GET: /Admin/Settings
        public async Task<IActionResult> Settings()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Dashboard");

            var client = _httpClientFactory.CreateClient("SafetyService");
            var token = HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.GetStringAsync("api/SystemSettings");
                ViewBag.Settings = JsonDocument.Parse(response).RootElement;
            }
            catch
            {
                ViewBag.Settings = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "System Settings";
            return View();
        }

        // POST: /Admin/UpdateSetting
        [HttpPost]
        public async Task<IActionResult> UpdateSetting(string key, string value)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            var client = _httpClientFactory.CreateClient("SafetyService");
            var token = HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = JsonSerializer.Serialize(value);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/SystemSettings/{key}", content);

            if (response.IsSuccessStatusCode)
            {
                await LogAuditAsync("Update Setting", "SystemSetting", $"Updated {key} to {value}");
                TempData["Success"] = "Setting updated successfully.";
            }
            else
                TempData["Error"] = "Failed to update setting.";

            return RedirectToAction("Settings");
        }

        // GET: /Admin/AuditLogs
        public async Task<IActionResult> AuditLogs()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Dashboard");

            var client = _httpClientFactory.CreateClient("SafetyService");
            var token = HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.GetStringAsync("api/Audit");
                ViewBag.AuditLogs = JsonDocument.Parse(response).RootElement;
            }
            catch
            {
                ViewBag.AuditLogs = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "Audit Logs";
            return View();
        }
    }
}
