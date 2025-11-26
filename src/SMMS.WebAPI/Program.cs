using System.Security.Claims;
using System.Text;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SMMS.Application.Common.Validators;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.billing.Handlers;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Application.Features.foodmenu.Handlers;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.Identity.Interfaces;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Infrastructure.Security;
using SMMS.Infrastructure.Repositories;
using SMMS.Persistence.Repositories.schools;
using SMMS.WebAPI.Configurations;
using SMMS.Application.Features.notification.Interfaces;
using SMMS.Infrastructure.Repositories.Implementations;
using SMMS.Persistence.Repositories.foodmenu;
using SMMS.Persistence.Repositories.Schools;
using SMMS.Persistence.Repositories.auth;
using SMMS.Application.Features.school.Handlers;
using SMMS.Application.Features.billing.Handlers;

using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Persistence.Repositories.Wardens;
using SMMS.Persistence;
using SMMS.Persistence.Data;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Handlers;
using SMMS.Application.Features.Wardens.Handlers;
using SMMS.Infrastructure.ExternalService.AiMenu;
using SMMS.Infrastructure.Service;
using SMMS.Infrastructure.Services;
using SMMS.Persistence.Repositories.Manager;
using SMMS.Persistence.Service;
using SMMS.Application.Features.auth.Handlers;
using SMMS.WebAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// =========================
// 1️⃣ Add Controllers
// =========================
builder.Services.AddControllers();

// =========================
// 2️⃣ MediatR + Validation
// =========================
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SMMS.Application.Features.foodmenu.Queries.GetWeekMenuQuery).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(WeeklyMenuHandler).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddPersistenceServices();

// =========================
// 3️⃣ Database Context
// =========================
builder.Services.AddDbContext<EduMealContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));
// ✅ Add OData with advanced query options
builder.Services.AddControllers()
    .AddOData(opt => opt
        .Select()
        .Filter()
        .OrderBy()
        .Expand()
        .Count()
        .SetMaxTop(100)
        .AddRouteComponents("odata", ODataConfig.GetEdmModel())
    );
// =========================
// 4️⃣ Dependency Injection (Services)
// =========================
builder.Services.AddScoped<IWeeklyMenuRepository, WeeklyMenuRepository>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IStudentHealthRepository, StudentHealthRepository>();
builder.Services.AddScoped<IJwtService, JwtTokenService>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IMenuRecommendResultRepository, MenuRecommendResultRepository>();
builder.Services.AddScoped<IManagerPaymentSettingRepository, ManagerPaymentSettingRepository>();
builder.Services.AddScoped<ISchoolRevenueRepository, SchoolRevenueRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AttendanceCommandHandler).Assembly));
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(NotificationHandler).Assembly));

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
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<ParentProfileHandler>();
});
// =========================
// 5️⃣ Swagger
// =========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // ✅ Thêm cấu hình để Swagger nhập JWT token
    /*options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập token JWT vào đây (ví dụ: Bearer abcdef12345)",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });*/

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// =========================
// 6️⃣ JWT Authentication
// =========================
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
        NameClaimType = "UserId", // ✅ ánh xạ claim "UserId"
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<ManagerAccountHandler>();
    cfg.RegisterServicesFromAssemblyContaining<ManagerClassHandler>();
    cfg.RegisterServicesFromAssemblyContaining<ManagerFinanceHandler>();
    cfg.RegisterServicesFromAssemblyContaining<ManagerParentHandler>();
    cfg.RegisterServicesFromAssemblyContaining<ManagerHandler>();
    cfg.RegisterServicesFromAssemblyContaining<WardensFeedbackHandler>();
    cfg.RegisterServicesFromAssemblyContaining<WardensHandler>();
    cfg.RegisterServicesFromAssemblyContaining<CloudStorageHandler>();
    cfg.RegisterServicesFromAssemblyContaining<ManagerPaymentSettingHandler>();
});
// Register Application Services
builder.Services.AddScoped<IWardensRepository, WardensRepository>();
builder.Services.AddScoped<IManagerRepository, ManagerRepository>();
builder.Services.AddScoped<IManagerAccountRepository, ManagerAccountRepository>();
builder.Services.AddScoped<IWardensFeedbackRepository, WardensFeedbackRepository>();
builder.Services.AddScoped<IManagerClassRepository, ManagerClassRepository>();
builder.Services.AddScoped<IManagerFinanceRepository, ManagerFinanceRepository>();
builder.Services.AddScoped<ICloudStorageRepository, CloudStorageRepository>();
builder.Services.AddScoped<IManagerNotificationRepository, ManagerNotificationRepository>();
builder.Services.AddScoped<INotificationRealtimeService, NotificationRealtimeService>();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapHub<NotificationHub>("/hubs/notifications");
app.UseHttpsRedirection();
//var password = "@1";
//var hashed = PasswordHasher.HashPassword(password);

//Console.ForegroundColor = ConsoleColor.Green;
//Console.WriteLine("=====================================");
//Console.WriteLine($"🔐 Hashed password for \"{password}\" is:");
//Console.WriteLine(hashed);
//Console.WriteLine("=====================================");
//Console.ResetColor();

// ✅ Thứ tự rất quan trọng:
app.UseAuthentication();
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();

