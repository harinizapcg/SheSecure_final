using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SheSecure.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ── LOGIN GET ──
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Token") != null)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        // ── LOGIN POST ──
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var client = _httpClientFactory.CreateClient("AuthService");
            var payload = JsonSerializer.Serialize(new { email, password });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString()!;

            // Decode JWT to extract claims
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var userName = jwt.Claims.FirstOrDefault(c =>
                               c.Type == "unique_name" ||
                               c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                           ?.Value ?? email;

            var role = jwt.Claims.FirstOrDefault(c =>
                           c.Type == "role" ||
                           c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                       ?.Value ?? "Employee";

            var userId = jwt.Claims.FirstOrDefault(c =>
                             c.Type == "sub" ||
                             c.Type == "nameid" ||
                             c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                         ?.Value ?? "";

            HttpContext.Session.SetString("Token", token);
            HttpContext.Session.SetString("Role", role);
            HttpContext.Session.SetString("UserName", userName);
            HttpContext.Session.SetString("UserId", userId);

            return RedirectToAction("Index", "Dashboard");
        }

        // ── REGISTER GET ──
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("Token") != null)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        // ── REGISTER POST ──
        [HttpPost]
        public async Task<IActionResult> Register(
            string fullName, string email, string password, string role)
        {
            var client = _httpClientFactory.CreateClient("AuthService");
            var payload = JsonSerializer.Serialize(new { fullName, email, password, role });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ViewBag.Error = error.Contains("already exists")
                    ? "An account with this email already exists."
                    : "Registration failed. Please check your details.";
                return View();
            }

            TempData["Success"] = "Account created successfully! Please sign in.";
            return RedirectToAction("Login");
        }

        // ── LOGOUT ──
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}