using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Persistence.Repositories.Wardens;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Configurations;
using SMMS.Persistence.Dbcontext;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Handlers;
using SMMS.Persistence.Repositories.Manager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<EduMealContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
// Register Application Services
builder.Services.AddScoped<IWardensService, WardensService>();
builder.Services.AddScoped<ICloudStorageService, CloudStorageService>();
builder.Services.AddScoped<IWardensFeedbackService, WardensFeedbackService>();
builder.Services.AddScoped<IManagerService, ManagerService>();
builder.Services.AddScoped<IManagerRepository, ManagerRepository>();
builder.Services.AddScoped<IManagerAccountRepository, ManagerAccountRepository>();
builder.Services.AddScoped<IManagerAccountService, ManagerAccountService>();
builder.Services.AddScoped<IManagerParentService, ManagerParentService>();
builder.Services.AddScoped<IManagerClassRepository, ManagerClassRepository>();
builder.Services.AddScoped<IManagerClassService, ManagerClassService>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
