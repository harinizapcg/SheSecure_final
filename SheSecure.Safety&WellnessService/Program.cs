using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SheSecure.Safety_WellnessService.Data;
using SheSecure.Safety_WellnessService.Interfaces;
using SheSecure.Safety_WellnessService.Jobs;
using SheSecure.Safety_WellnessService.Repositories;
using SheSecure.Safety_WellnessService.Services;
using SheSecure.WellnessSafetyService.Interfaces;
using SheSecure.WellnessSafetyService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<WellnessDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

builder.Services.AddHangfireServer();

builder.Services.AddHttpClient("NotificationService", c =>
    c.BaseAddress = new Uri("https://localhost:7179/"));

builder.Services.AddScoped<ISafeReachRepository, SafeReachRepository>();
builder.Services.AddScoped<ISafeReachService, SafeReachService>();
builder.Services.AddScoped<IWellnessRequestRepository, WellnessRequestRepository>();
builder.Services.AddScoped<IWellnessRequestService, WellnessRequestService>();
builder.Services.AddScoped<IEmergencyAlertRepository, EmergencyAlertRepository>();
builder.Services.AddScoped<IEmergencyAlertService, EmergencyAlertService>();
builder.Services.AddScoped<IMoodLogRepository, MoodLogRepository>();
builder.Services.AddScoped<IMoodLogService, MoodLogService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<SafeReachReminderJob>();

builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["NotificationService:BaseUrl"]!);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = builder.Configuration["Jwt:Issuer"],

                ValidAudience = builder.Configuration["Jwt:Audience"],

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]))
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire");
app.MapControllers();
app.Run();