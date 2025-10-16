using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Common.Validators;
using SMMS.Application.Features.foodmenu.Handlers;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Persistence.DbContextSite;
using SMMS.Persistence.Repositories.foodmenu;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SMMS.Application.Features.foodmenu.Queries.GetWeekMenuQuery).Assembly));
// FluentValidation: quét toàn bộ validators trong Application
builder.Services.AddValidatorsFromAssembly(typeof(WeeklyMenuHandler).Assembly);
// Pipeline: chạy validation trước handler
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddScoped<IWeeklyMenuRepository, WeeklyMenuRepository>();
builder.Services.AddDbContext<EduMealContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
