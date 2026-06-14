using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace SheSecure.Web.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ComplaintController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient("ComplaintService");
            var token = HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // GET — Employee: raise + my complaints
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            var client = GetClient();
            try
            {
                var response = await client.GetStringAsync("api/Complaint/all");
                ViewBag.Complaints = JsonDocument.Parse(response).RootElement;
            }
            catch
            {
                ViewBag.Complaints = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "My Complaints";
            return View();
        }

        // GET — HR/Admin: all complaints
        public async Task<IActionResult> All()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("Role");
            if (role != "HR" && role != "Admin")
                return RedirectToAction("Index", "Dashboard");

            var client = GetClient();
            try
            {
                var response = await client.GetStringAsync("api/Complaint/all");
                ViewBag.Complaints = JsonDocument.Parse(response).RootElement;
            }
            catch
            {
                ViewBag.Complaints = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "All Complaints";
            return View();
        }

        // GET — Detail view with comments + history
        public async Task<IActionResult> Detail(int id)
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Auth");

            var client = GetClient();
            try
            {
                var complaintTask = client.GetStringAsync($"api/Complaint/{id}");
                var commentsTask = client.GetStringAsync($"api/ComplaintComments/{id}");
                var historyTask = client.GetStringAsync($"api/ComplaintStatusHistory/{id}");

                await Task.WhenAll(complaintTask, commentsTask, historyTask);

                ViewBag.Complaint = JsonDocument.Parse(complaintTask.Result).RootElement;
                ViewBag.Comments = JsonDocument.Parse(commentsTask.Result).RootElement;
                ViewBag.History = JsonDocument.Parse(historyTask.Result).RootElement;
            }
            catch
            {
                ViewBag.Complaint = JsonDocument.Parse("{}").RootElement;
                ViewBag.Comments = JsonDocument.Parse("[]").RootElement;
                ViewBag.History = JsonDocument.Parse("[]").RootElement;
            }

            ViewData["Title"] = "Complaint Detail";
            return View();
        }

        // POST — Create complaint
        [HttpPost]
        public async Task<IActionResult> Create(
            string category, string subject,
            string description, string priority, bool isAnonymous)
        {
            var client = GetClient();
            var payload = JsonSerializer.Serialize(new
            {
                category,
                subject,
                description,
                priority,
                isAnonymous
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Complaint/create", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(result);
                var cmpNum = doc.RootElement.GetProperty("complaintNumber").GetString();
                TempData["Success"] = $"Complaint submitted! Your reference: {cmpNum}";
            }
            else
                TempData["Error"] = "Failed to submit complaint. Please try again.";

            return RedirectToAction("Index");
        }

        // POST — Update status (HR)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(
            int complaintId, string status, string resolutionNotes)
        {
            var client = GetClient();
            var payload = JsonSerializer.Serialize(new
            {
                complaintId,
                status,
                resolutionNotes
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PutAsync("api/Complaint/status", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Status updated successfully!";
            else
                TempData["Error"] = "Failed to update status.";

            return RedirectToAction("Detail", new { id = complaintId });
        }

        // POST — Add comment
        [HttpPost]
        public async Task<IActionResult> AddComment(
            int complaintId, string comment, bool isInternal)
        {
            var client = GetClient();
            var userId = HttpContext.Session.GetString("UserId") ?? "1";
            var payload = JsonSerializer.Serialize(new
            {
                complaintId,
                userId = int.TryParse(userId, out var uid) ? uid : 1,
                comment,
                isInternal
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            await client.PostAsync("api/ComplaintComments/add", content);

            return RedirectToAction("Detail", new { id = complaintId });
        }

        // POST — Upload evidence
        [HttpPost]
        public async Task<IActionResult> UploadFile(int complaintId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction("Detail", new { id = complaintId });
            }

            var client = GetClient();
            using var form = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            form.Add(fileContent, "file", file.FileName);

            var response = await client.PostAsync(
                $"api/ComplaintFiles/upload?complaintId={complaintId}", form);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Evidence uploaded successfully!";
            else
                TempData["Error"] = "Failed to upload file.";

            return RedirectToAction("Detail", new { id = complaintId });
        }
    }
}