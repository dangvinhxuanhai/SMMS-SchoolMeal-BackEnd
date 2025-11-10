using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Persistence.Repositories.Wardens;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Dbcontext;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Handlers;
using SMMS.Persistence.Repositories.Manager;
using SMMS.Application.Features.Wardens.Handlers;

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
});
// Register Application Services
builder.Services.AddScoped<IWardensRepository, WardensRepository>();
builder.Services.AddScoped<IManagerRepository, ManagerRepository>();
builder.Services.AddScoped<IManagerAccountRepository, ManagerAccountRepository>();
builder.Services.AddScoped<IWardensFeedbackRepository, WardensFeedbackRepository>();
builder.Services.AddScoped<IManagerClassRepository, ManagerClassRepository>();
builder.Services.AddScoped<IManagerFinanceRepository, ManagerFinanceRepository>();
builder.Services.AddScoped<ICloudStorageRepository, CloudStorageRepository>();
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
