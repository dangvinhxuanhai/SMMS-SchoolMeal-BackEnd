using SMMS.Application.Abstractions;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.Inventory.Interfaces;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Meal.Interfaces;
using SMMS.Application.Features.notification.Interfaces;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Application.Features.Plan.Interfaces;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Infrastructure.Repositories;
using SMMS.Infrastructure.Repositories.Implementations;
using SMMS.Infrastructure.Service;
using SMMS.Persistence;
using SMMS.Persistence.Repositories.auth;
using SMMS.Persistence.Repositories.billing;
using SMMS.Persistence.Repositories.foodmenu;
using SMMS.Persistence.Repositories.inventory;
using SMMS.Persistence.Repositories.Manager;
using SMMS.Persistence.Repositories.Meal;
using SMMS.Persistence.Repositories.nutrition;
using SMMS.Persistence.Repositories.purchasing;
using SMMS.Persistence.Repositories.schools;
using SMMS.Persistence.Repositories.Schools;
using SMMS.Persistence.Repositories.Wardens;
using SMMS.WebAPI.Hubs;

namespace SMMS.WebAPI.Configurations;

public static class RepositoryDI
{
    public static IServiceCollection AddPrjRepo(this IServiceCollection services)
    {
        services.AddScoped<IWeeklyMenuRepository, WeeklyMenuRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IStudentHealthRepository, StudentHealthRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IWardensHealthRepository, WardensHealthRepository>();
        services.AddScoped<IMenuRecommendResultRepository, MenuRecommendResultRepository>();
        services.AddScoped<IManagerPaymentSettingRepository, ManagerPaymentSettingRepository>();
        services.AddScoped<ISchoolRevenueRepository, SchoolRevenueRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<INotificationARealtimeService,  NotificationARealtimeService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWardensRepository, WardensRepository>();
        services.AddScoped<IManagerRepository, ManagerRepository>();
        services.AddScoped<IManagerAccountRepository, ManagerAccountRepository>();
        services.AddScoped<IWardensFeedbackRepository, WardensFeedbackRepository>();
        services.AddScoped<IManagerClassRepository, ManagerClassRepository>();
        services.AddScoped<IManagerFinanceRepository, ManagerFinanceRepository>();
        services.AddScoped<ICloudStorageRepository, CloudStorageRepository>();
        services.AddScoped<IManagerNotificationRepository, ManagerNotificationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IClassStudentRepository, ClassStudentRepository>();
        services.AddScoped<IMenuRecommendSessionRepository, MenuRecommendSessionRepository>();
        services.AddScoped<IMenuRecommendResultRepository, MenuRecommendResultRepository>();
        services.AddScoped<IFoodItemRepository, FoodItemRepository>();
        services.AddScoped<IIngredientRepository, IngredientRepository>();
        services.AddScoped<IFoodItemIngredientRepository, FoodItemIngredientRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IKitchenDashboardRepository, KitchenDashboardRepository>();
        services.AddScoped<IPurchasePlanRepository, PurchasePlanRepository>();
        services.AddScoped<IScheduleMealRepository, ScheduleMealRepository>(); 
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<ISchoolPaymentGatewayRepository, SchoolPaymentGatewayRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IManagerAcademicYearRepository, ManagerAcademicYearRepository>();
        services.AddScoped<IStudentImageRepository, StudentImageRepository>();
        services.AddScoped<ISchoolInvoiceRepository, SchoolInvoiceRepository>();
        return services;
    }
}
