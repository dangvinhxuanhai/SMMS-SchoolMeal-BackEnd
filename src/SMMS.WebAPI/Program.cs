using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.WebAPI.Configurations;
using Microsoft.Extensions.FileProviders;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Application.Abstractions;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Persistence;
using SMMS.Persistence.Data;
using SMMS.Infrastructure.ExternalService.AiMenu;
using SMMS.Infrastructure.Repositories;
using SMMS.Infrastructure.Repositories.Implementations;
using SMMS.Infrastructure.Security;
using SMMS.Infrastructure.Service;
using SMMS.Infrastructure.Services;
using SMMS.Persistence.Data;
using SMMS.Persistence.Repositories.auth;
using SMMS.Persistence.Repositories.foodmenu;
using SMMS.Persistence.Repositories.Manager;
using SMMS.Persistence.Repositories.schools;
using SMMS.Persistence.Repositories.Schools;
using SMMS.Persistence.Repositories.Wardens;
using SMMS.Persistence;
using SMMS.Persistence.Repositories.nutrition;
using SMMS.WebAPI.Configurations;
using SMMS.WebAPI.Hubs;
using SMMS.Application.Features.school.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<EduMealContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));

// DI
builder.Services.AddPrjRepo();
builder.Services.AddPrjService();
builder.Services.AddPersistenceServices();

builder.Services.Configure<AiMenuOptions>(
    builder.Configuration.GetSection(AiMenuOptions.SectionName));

builder.Services.AddHttpClient<IAiMenuClient, AiMenuClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<AiMenuOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<IAiMenuAdminClient, AiMenuAdminClient>((sp, http) =>
{
    var opts = sp.GetRequiredService<IOptions<AiMenuOptions>>().Value;
    http.BaseAddress = new Uri(opts.BaseUrl);
});

//  swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // ✅ Thêm cấu hình để Swagger nhập JWT token
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập token JWT vào đây (ví dụ: Bearer abcdef12345)",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// JWT Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        ),
        NameClaimType = "UserId",
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("accessToken"))
            {
                context.Token = context.Request.Cookies["accessToken"];
            }
            return Task.CompletedTask;
        }
    };

    // Đó test lại r em mở comment cái dòng dưới hình như cần để phía be allow nhận token từ cookie
    /*options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["jwt"];
            return Task.CompletedTask;
        }
    };*/
    });

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


builder.Services.AddAuthorization();
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
var app = builder.Build();
var uploadFolderPath = Path.Combine(builder.Environment.ContentRootPath, "edu-meal");
if (!Directory.Exists(uploadFolderPath))
{
    Directory.CreateDirectory(uploadFolderPath);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapHub<NotificationHub>("/hubs/notifications");
app.UseHttpsRedirection();

// var hasher = new PasswordHasher();
// var password = "@1";
// var hashed = hasher.HashPassword(password);
//
// Console.ForegroundColor = ConsoleColor.Green;
// Console.WriteLine("=====================================");
// Console.WriteLine($"🔐 Hashed password for \"{password}\" is:");
// Console.WriteLine(hashed);
// Console.WriteLine("=====================================");
// Console.ResetColor();

app.UseCors("AllowFrontend");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadFolderPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
