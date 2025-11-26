using SMMS.Application.Features.auth.Handlers;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.billing.Handlers;
using SMMS.Application.Features.Identity.Interfaces;
using SMMS.Application.Features.school.Handlers;
using SMMS.Infrastructure.Security;
using SMMS.Infrastructure.Services;
using SMMS.Persistence.Service;

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
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(AttendanceCommandHandler).Assembly));
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(NotificationHandler).Assembly));
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<ParentProfileHandler>();
        });
        return services;
    }
}
