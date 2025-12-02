using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SMMS.Application.Abstractions;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Infrastructure.ExternalService.AiMenu;
using SMMS.Infrastructure.Repositories;
using SMMS.Infrastructure.Repositories.Implementations;
using SMMS.Infrastructure.Security;
using SMMS.Infrastructure.Service;
using SMMS.Infrastructure.Services;
using SMMS.Persistence;
using SMMS.Persistence;
using SMMS.Persistence.Data;
using SMMS.Persistence.Data;
using SMMS.Persistence.Repositories.auth;
using SMMS.Persistence.Repositories.foodmenu;
using SMMS.Persistence.Repositories.Manager;
using SMMS.Persistence.Repositories.nutrition;
using SMMS.Persistence.Repositories.schools;
using SMMS.Persistence.Repositories.Schools;
using SMMS.Persistence.Repositories.Wardens;
using SMMS.WebAPI.Configurations;
using SMMS.WebAPI.Configurations;
using SMMS.WebAPI.Hubs;

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

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SMMS.WebAPI",
        Version = "v1"
    });

    // ĐẢM BẢO schemaId là duy nhất cho cả generic và non-generic
    options.CustomSchemaIds(type =>
    {
        var ns = type.Namespace ?? "Global";
        ns = ns.Replace(".", "_");

        if (type.IsGenericType)
        {
            // Ví dụ: SMMS_Application_Features_foodmenu_DTOs_PagedResult_WeeklyScheduleDto
            var genericTypeName = type.Name[..type.Name.IndexOf('`')]; // bỏ `1
            var genericArgs = string.Join("_",
                type.GetGenericArguments().Select(t => t.Name));
            return $"{ns}_{genericTypeName}_{genericArgs}";
        }

        // Non-generic: SMMS_Application_Features_school_DTOs_CreateSchoolDto
        return $"{ns}_{type.Name}";
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

app.UseAuthentication();
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
