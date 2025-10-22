using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SMMS.Application.Common.Validators;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.foodmenu.Handlers;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.Identity.Interfaces;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Infrastructure.Security;
using SMMS.Infrastructure.Service;
using SMMS.Infrastructure.Services;
using SMMS.Persistence.DbContextSite;
using SMMS.Persistence.Repositories.foodmenu;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Infrastructure.Repositories;
using SMMS.Persistence.Repositories.school;
using Microsoft.AspNetCore.OData;
using SMMS.WebAPI.Configurations;

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
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStudentHealthService, StudentHealthService>();
builder.Services.AddScoped<IJwtService, JwtTokenService>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
// =========================
// 5️⃣ Swagger
// =========================
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

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
app.UseAuthorization();

app.MapControllers();
app.Run();

