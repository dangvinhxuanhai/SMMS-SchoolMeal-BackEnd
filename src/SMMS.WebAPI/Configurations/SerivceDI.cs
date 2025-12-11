using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.SignalR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Common.Validators;
using SMMS.Application.Features.auth.Handlers;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.billing.Handlers;
using SMMS.Application.Features.foodmenu.Handlers;
using SMMS.Application.Features.Identity.Interfaces;
using SMMS.Application.Features.Manager.Handlers;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.school.Handlers;
using SMMS.Application.Features.Wardens.Handlers;
using SMMS.Infrastructure.Security;
using SMMS.Infrastructure.Service;
using SMMS.Infrastructure.Services;
using SMMS.Persistence;
using SMMS.Persistence.Service;
using SMMS.WebAPI.Hubs;

namespace SMMS.WebAPI.Configurations;

public static class SerivceDI
{
    public static IServiceCollection AddPrjService(this IServiceCollection services)
    {
        // resolve swagger namespace
        services.AddSwaggerGen(c =>
        {
            c.CustomSchemaIds(type => $"{type.Namespace}.{type.Name}");
        });

        // continute DI
        services.AddScoped<IJwtService, JwtTokenService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddHttpClient<IPayOsIntegrationService, PayOsIntegrationService>();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(AttendanceCommandHandler).Assembly));
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(NotificationHandler).Assembly));
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<ParentProfileHandler>();
        });

        //  mediatr
        services.AddMediatR(cfg =>
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
            cfg.RegisterServicesFromAssemblyContaining<ManagerNotificationHandler>();
            cfg.RegisterServicesFromAssemblyContaining<WardensHealthHandler>();
            cfg.RegisterServicesFromAssemblyContaining<ManagerAcademicYearHandler>();
        });

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(SMMS.Application.Features.foodmenu.Queries.GetWeekMenuQuery).Assembly));

        services.AddValidatorsFromAssembly(typeof(WeeklyMenuHandler).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        //  cloud
        services.AddScoped<CloudinaryService>();
        services.AddScoped<INotificationRealtimeService, NotificationRealtimeService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        //  Odata
        services.AddControllers()
            .AddOData(opt => opt
                        .Select()
                        .Filter()
                        .OrderBy()
                        .Expand()
                        .Count()
                        .SetMaxTop(100)
                        .AddRouteComponents("odata",
                                            ODataConfig.GetEdmModel())
        );

        return services;
    }
}
