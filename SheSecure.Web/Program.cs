using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

var apiSettings = builder.Configuration.GetSection("ApiSettings");

builder.Services.AddHttpClient();

builder.Services.AddHttpClient("AuthService", c =>
    c.BaseAddress = new Uri(apiSettings["AuthServiceUrl"]!));
builder.Services.AddHttpClient("SafetyService", c =>
    c.BaseAddress = new Uri(apiSettings["SafetyServiceUrl"]!));
builder.Services.AddHttpClient("ComplaintService", c =>
    c.BaseAddress = new Uri(apiSettings["ComplaintServiceUrl"]!));
builder.Services.AddHttpClient("NotificationService", c =>
    c.BaseAddress = new Uri(apiSettings["NotificationServiceUrl"]!));
builder.Services.AddHttpClient("DashboardService", c =>
    c.BaseAddress = new Uri(apiSettings["DashboardServiceUrl"]!));

// Note: Ensure project portability by reading DB Connection String from appsettings.json.
// Uncomment and replace 'MyDbContext' when Entity Framework is configured for this project.
// builder.Services.AddDbContext<MyDbContext>(options => 
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();