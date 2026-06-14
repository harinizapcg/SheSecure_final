using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SheSecure.ComplaintService.Data;
using SheSecure.ComplaintService.Interfaces;
using SheSecure.ComplaintService.Repositories;
using SheSecure.ComplaintService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ComplaintDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();

builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<
    IComplaintFileRepository,
    ComplaintFileRepository>();

builder.Services.AddScoped<
    IComplaintFileService,
    ComplaintFileService>();
builder.Services.AddScoped<
    IComplaintCommentRepository,
    ComplaintCommentRepository>();

builder.Services.AddScoped<
    IComplaintCommentService,
    ComplaintCommentService>();
builder.Services.AddScoped<
    IComplaintStatusHistoryRepository,
    ComplaintStatusHistoryRepository>();

builder.Services.AddScoped<
    IComplaintStatusHistoryService,
    ComplaintStatusHistoryService>();
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

builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["NotificationService:BaseUrl"]!);
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();