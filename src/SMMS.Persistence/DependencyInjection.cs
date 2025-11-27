using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Infrastructure.Repositories.Implementations;
using SMMS.Infrastructure.Repositories;
using SMMS.Persistence.Repositories.auth;
using SMMS.Persistence.Repositories.foodmenu;
using SMMS.Persistence.Repositories.schools;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence;
/// <summary>
/// * Điểm mạnh:
///     1. với những repo mà ko có cái gì mới hay là ko cần override thì auto ko cần impl lại
///     2. ngắn gọn hạn chế quá tràn lan ở program.cs
///     3. đó e sẽ refactor lại code sau tạm thời thì cứ để chạy được cho kịp tiến độ
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IReadRepository<,>), typeof(EFReadRepository<,>));
        services.AddScoped(typeof(IWriteRepository<,>), typeof(EFWriteRepository<,>));

        return services;
    }

    /*public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        //services.AddScoped<IMealRepository, MealRepository>();

        return services;
    }*/

}
