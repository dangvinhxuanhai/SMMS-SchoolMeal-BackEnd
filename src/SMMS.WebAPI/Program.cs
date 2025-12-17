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
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Persistence;
using SMMS.Persistence.Data;
using SMMS.Infrastructure.ExternalService.AiMenu;
using SMMS.WebAPI.Hubs;
using SMMS.Application.Common.Options;
using SMMS.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EduMealContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));

builder.Services.AddPrjRepo();
builder.Services.AddPrjService();
builder.Services.AddPersistenceServices();

builder.Services.Configure<AiMenuOptions>(builder.Configuration.GetSection(AiMenuOptions.SectionName));
builder.Services.AddHttpClient<IAiMenuClient, AiMenuClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<AiMenuOptions>>().Value;
    if (!string.IsNullOrWhiteSpace(options.BaseUrl)) client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<IAiMenuAdminClient, AiMenuAdminClient>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<AiMenuOptions>>().Value;
    if (!string.IsNullOrWhiteSpace(opts.BaseUrl)) client.BaseAddress = new Uri(opts.BaseUrl);
});
builder.Services.Configure<PayOsOptions>(builder.Configuration.GetSection(PayOsOptions.SectionName));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type =>
    {
        var ns = type.Namespace ?? "Global";
        ns = ns.Replace(".", "_");
        if (type.IsGenericType)
        {
            var genericTypeName = type.Name[..type.Name.IndexOf('`')];
            var genericArgs = string.Join("_", type.GetGenericArguments().Select(t => t.Name));
            return $"{ns}_{genericTypeName}_{genericArgs}";
        }
        return $"{ns}_{type.Name}";
    });

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
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "EduMeal API", Version = "v1" });
});

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            NameClaimType = "UserId",
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["accessToken"];

                if (string.IsNullOrEmpty(accessToken) && context.Request.Cookies.ContainsKey("accessToken"))
                {
                    accessToken = context.Request.Cookies["accessToken"];
                }

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                   (path.StartsWithSegments("/hubs/notifications") || !string.IsNullOrEmpty(accessToken)))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddAuthorization();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "https://edumeal.id.vn",
                "https://admin.edumeal.id.vn",
                "http://localhost:3000",
                "https://smms-school-meal-admin.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

var uploadFolderPath = Path.Combine(builder.Environment.ContentRootPath, "edu-meal");
if (!Directory.Exists(uploadFolderPath)) Directory.CreateDirectory(uploadFolderPath);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
    FileProvider = new PhysicalFileProvider(uploadFolderPath), RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
