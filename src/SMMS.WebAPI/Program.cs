using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.WebAPI.Configurations;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Persistence;
using SMMS.Persistence.Data;
using SMMS.Infrastructure.ExternalService.AiMenu;
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

app.UseAuthentication();
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();

