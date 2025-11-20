using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Domain.Entities.fridge;
using SMMS.Domain.Entities.inventory;
using SMMS.Domain.Entities.logs;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.purchasing;
using SMMS.Domain.Entities.rag;
using SMMS.Domain.Entities.school;

namespace SMMS.Persistence.Data;

public partial class EduMealContext : DbContext
{
    public EduMealContext()
    {
    }

    public EduMealContext(DbContextOptions<EduMealContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcademicYear> AcademicYears { get; set; }

    public virtual DbSet<Allergen> Allergens { get; set; }

    public virtual DbSet<AllergeticIngredient> AllergeticIngredients { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<DailyMeal> DailyMeals { get; set; }

    public virtual DbSet<ExternalProvider> ExternalProviders { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<FoodInFridge> FoodInFridges { get; set; }

    public virtual DbSet<FoodItem> FoodItems { get; set; }

    public virtual DbSet<FoodItemIngredient> FoodItemIngredients { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<IngredientAlternative> IngredientAlternatives { get; set; }

    public virtual DbSet<InventoryItem> InventoryItems { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<LoginAttempt> LoginAttempts { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MenuDay> MenuDays { get; set; }

    public virtual DbSet<MenuDayFoodItem> MenuDayFoodItems { get; set; }

    public virtual DbSet<MenuFoodItem> MenuFoodItems { get; set; }

    public virtual DbSet<MenuRecommendResult> MenuRecommendResults { get; set; }

    public virtual DbSet<MenuRecommendSession> MenuRecommendSessions { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }

    public virtual DbSet<PurchasePlan> PurchasePlans { get; set; }

    public virtual DbSet<PurchasePlanLine> PurchasePlanLines { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ScheduleMeal> ScheduleMeals { get; set; }

    public virtual DbSet<School> Schools { get; set; }

    public virtual DbSet<SchoolPaymentSetting> SchoolPaymentSettings { get; set; }

    public virtual DbSet<SchoolRevenue> SchoolRevenues { get; set; }

    public virtual DbSet<StagingStudent> StagingStudents { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentAllergen> StudentAllergens { get; set; }

    public virtual DbSet<StudentClass> StudentClasses { get; set; }

    public virtual DbSet<StudentHealthRecord> StudentHealthRecords { get; set; }

    public virtual DbSet<StudentImage> StudentImages { get; set; }

    public virtual DbSet<StudentImageTag> StudentImageTags { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserExternalLogin> UserExternalLogins { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=localhost,1433;Database=EduMeal;User Id=sa;Password=Hung300403;TrustServerCertificate=True;integrated security=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.HasKey(e => e.YearId).HasName("PK__Academic__C33A18CD23C73AC2");

            entity.HasOne(d => d.School).WithMany(p => p.AcademicYears).HasConstraintName("FK_AcademicYears_School");
        });

        modelBuilder.Entity<Allergen>(entity =>
        {
            entity.HasKey(e => e.AllergenId).HasName("PK__Allergen__158B939FFE1B4BAD");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Allergens).HasConstraintName("FK_Allergens_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.Allergens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Allergens_School");
        });

        modelBuilder.Entity<AllergeticIngredient>(entity =>
        {
            entity.HasKey(e => new { e.IngredientId, e.AllergenId }).HasName("PK__Allerget__5FF60B6360259829");

            entity.HasOne(d => d.Allergen).WithMany(p => p.AllergeticIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Allergeti__Aller__114A936A");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.AllergeticIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Allergeti__Ingre__10566F31");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69261C0637B330");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.NotifiedByNavigation).WithMany(p => p.Attendances).HasConstraintName("FK__Attendanc__Notif__7755B73D");

            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Stude__76619304");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AuditLog__5E5486485AD98C49");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLogs_User");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classes__CB1927C02E74610B");

            entity.Property(e => e.ClassId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.School).WithMany(p => p.Classes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Classes__SchoolI__5AEE82B9");

            entity.HasOne(d => d.Teacher).WithOne(p => p.Class).HasConstraintName("FK__Classes__Teacher__5CD6CB2B");

            entity.HasOne(d => d.Year).WithMany(p => p.Classes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Classes__YearId__5BE2A6F2");
        });

        modelBuilder.Entity<DailyMeal>(entity =>
        {
            entity.HasKey(e => e.DailyMealId).HasName("PK__DailyMea__4325CAFBEC36A978");

            entity.HasOne(d => d.ScheduleMeal).WithMany(p => p.DailyMeals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DailyMeals_Schedule");
        });

        modelBuilder.Entity<ExternalProvider>(entity =>
        {
            entity.HasKey(e => e.ProviderId).HasName("PK__External__B54C687DEC992D71");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDD66483E6DB");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.DailyMeal).WithMany(p => p.Feedbacks).HasConstraintName("FK_Feedbacks_DailyMeal");

            entity.HasOne(d => d.Sender).WithMany(p => p.Feedbacks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedbacks__Sende__44CA3770");
        });

        modelBuilder.Entity<FoodInFridge>(entity =>
        {
            entity.HasKey(e => e.SampleId).HasName("PK__FoodInFr__8B99EC6A504C2E9D");

            entity.Property(e => e.StoredAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.FoodInFridgeDeletedByNavigations).HasConstraintName("FK_FIF_DeletedBy");

            entity.HasOne(d => d.Food).WithMany(p => p.FoodInFridges).HasConstraintName("FK_FIF_Food");

            entity.HasOne(d => d.Menu).WithMany(p => p.FoodInFridges).HasConstraintName("FK_FIF_Menu");

            entity.HasOne(d => d.School).WithMany(p => p.FoodInFridges)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FIF_School");

            entity.HasOne(d => d.StoredByNavigation).WithMany(p => p.FoodInFridgeStoredByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FIF_StoredBy");

            entity.HasOne(d => d.Year).WithMany(p => p.FoodInFridges).HasConstraintName("FK_FIF_Year");

            entity.HasMany(d => d.Ingredients).WithMany(p => p.Samples)
                .UsingEntity<Dictionary<string, object>>(
                    "IngredientInFridge",
                    r => r.HasOne<Ingredient>().WithMany()
                        .HasForeignKey("IngredientId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_FIFI_Ingredient"),
                    l => l.HasOne<FoodInFridge>().WithMany()
                        .HasForeignKey("SampleId")
                        .HasConstraintName("FK_FIFI_Sample"),
                    j =>
                    {
                        j.HasKey("SampleId", "IngredientId").HasName("PK__Ingredie__3073074F80259387");
                        j.ToTable("IngredientInFridge", "fridge");
                    });
        });

        modelBuilder.Entity<FoodItem>(entity =>
        {
            entity.HasKey(e => e.FoodId).HasName("PK__FoodItem__856DB3EB194CC084");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsMainDish).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.FoodItems).HasConstraintName("FK_FoodItems_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.FoodItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FoodItems_School");
        });

        modelBuilder.Entity<FoodItemIngredient>(entity =>
        {
            entity.HasKey(e => new { e.FoodId, e.IngredientId }).HasName("PK__FoodItem__3E8758CEE51FD640");

            entity.HasOne(d => d.Food).WithMany(p => p.FoodItemIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FoodItemI__FoodI__1AD3FDA4");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.FoodItemIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FoodItemI__Ingre__1BC821DD");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.IngredientId).HasName("PK__Ingredie__BEAEB25AADAD4292");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Ingredients).HasConstraintName("FK_Ingredients_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.Ingredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ingredients_School");
        });

        modelBuilder.Entity<IngredientAlternative>(entity =>
        {
            entity.HasKey(e => new { e.IngredientId, e.AltIngredientId }).HasName("PK__Ingredie__790D452065C766FB");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.AltIngredient).WithMany(p => p.IngredientAlternativeAltIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ingredien__AltIn__078C1F06");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.IngredientAlternatives).HasConstraintName("FK__Ingredien__Creat__0880433F");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.IngredientAlternativeIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ingredien__Ingre__0697FACD");
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Inventor__727E838B44428B69");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryItems).HasConstraintName("FK_InventoryItems_CreatedBy");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.InventoryItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Ingre__4B7734FF");

            entity.HasOne(d => d.School).WithMany(p => p.InventoryItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryItems_School");
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.TransId).HasName("PK__Inventor__9E5DDB3CCD51497F");

            entity.Property(e => e.TransDate).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Item).WithMany(p => p.InventoryTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__ItemI__51300E55");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAB5A054AF47");

            entity.HasOne(d => d.Student).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__Studen__6166761E");
        });

        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId).HasName("PK__LoginAtt__891A68E6A2D65AA4");

            entity.Property(e => e.AttemptAt).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menus__C99ED230326FDE1E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsVisible).HasDefaultValue(true);

            entity.HasOne(d => d.ConfirmedByNavigation).WithMany(p => p.Menus).HasConstraintName("FK_Menus_Confirm");

            entity.HasOne(d => d.School).WithMany(p => p.Menus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Menus_School");

            entity.HasOne(d => d.Year).WithMany(p => p.Menus).HasConstraintName("FK_Menus_Year");
        });

        modelBuilder.Entity<MenuDay>(entity =>
        {
            entity.HasKey(e => e.MenuDayId).HasName("PK__MenuDays__48283E54D1074AC4");

            entity.HasOne(d => d.Menu).WithMany(p => p.MenuDays)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuDays_Menus");
        });

        modelBuilder.Entity<MenuDayFoodItem>(entity =>
        {
            entity.HasOne(d => d.Food).WithMany(p => p.MenuDayFoodItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuDayFoodItems_Food");

            entity.HasOne(d => d.MenuDay).WithMany(p => p.MenuDayFoodItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuDayFoodItems_MenuDay");
        });

        modelBuilder.Entity<MenuFoodItem>(entity =>
        {
            entity.HasOne(d => d.DailyMeal).WithMany(p => p.MenuFoodItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuFoodItems_DailyMeals");

            entity.HasOne(d => d.Food).WithMany(p => p.MenuFoodItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuFoodItems_FoodItems");
        });

        modelBuilder.Entity<MenuRecommendResult>(entity =>
        {
            entity.HasKey(e => new { e.SessionId, e.FoodId, e.IsMain }).HasName("PK__MenuReco__FCCDB5A8D3F5EF9D");

            entity.HasOne(d => d.Food).WithMany(p => p.MenuRecommendResults)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MenuRecom__FoodI__1D7B6025");

            entity.HasOne(d => d.Session).WithMany(p => p.MenuRecommendResults)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MenuRecom__Sessi__1C873BEC");
        });

        modelBuilder.Entity<MenuRecommendSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__MenuReco__C9F492904918AD41");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__notifica__20CF2E12FB188302");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Sender).WithMany(p => p.Notifications).HasConstraintName("FK__notificat__Sende__6DCC4D03");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => new { e.NotificationId, e.UserId }).HasName("PK__Notifica__F1B7A2D6F45F8C6E");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationRecipients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Notif__719CDDE7");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationRecipients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__72910220");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A387B785F8F");

            entity.Property(e => e.ExpectedAmount).HasDefaultValue(600m);
            entity.Property(e => e.PaidAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PaymentStatus).HasDefaultValue("pending");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Invoic__690797E6");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Purchase__C3905BCF78BBEDFE");

            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Plan).WithMany(p => p.PurchaseOrders).HasConstraintName("FK_PurchaseOrders_Plans");

            entity.HasOne(d => d.School).WithMany(p => p.PurchaseOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrders_Schools");

            entity.HasOne(d => d.StaffInChargedNavigation).WithMany(p => p.PurchaseOrders).HasConstraintName("FK_PurchaseOrders_Users");
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.HasKey(e => e.LinesId).HasName("PK__Purchase__728C596D3897E235");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.PurchaseOrderLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrderLines_Ingredients");

            entity.HasOne(d => d.Order).WithMany(p => p.PurchaseOrderLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrderLines_Orders");

            entity.HasOne(d => d.User).WithMany(p => p.PurchaseOrderLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrderLines_Users");
        });

        modelBuilder.Entity<PurchasePlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Purchase__755C22B73025FA07");

            entity.Property(e => e.GeneratedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.ConfirmedByNavigation).WithMany(p => p.PurchasePlanConfirmedByNavigations).HasConstraintName("FK_PurchasePlans_ConfirmedBy");

            entity.HasOne(d => d.Menu).WithMany(p => p.PurchasePlans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchasePlans_Menus");

            entity.HasOne(d => d.Staff).WithMany(p => p.PurchasePlanStaffs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchasePlans_Staff");
        });

        modelBuilder.Entity<PurchasePlanLine>(entity =>
        {
            entity.HasKey(e => new { e.PlanId, e.IngredientId }).HasName("PK__Purchase__CEB6C992B65E5699");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.PurchasePlanLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseP__Ingre__5CA1C101");

            entity.HasOne(d => d.Plan).WithMany(p => p.PurchasePlanLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseP__PlanI__5BAD9CC8");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__RefreshT__F5845E391126442E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.ReplacedBy).WithMany(p => p.InverseReplacedBy).HasConstraintName("FK__RefreshTo__Repla__3D5E1FD2");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefreshTo__UserI__3C69FB99");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A2A5C1E7A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<ScheduleMeal>(entity =>
        {
            entity.HasKey(e => e.ScheduleMealId).HasName("PK__Schedule__7F2713EE01339074");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("Draft");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ScheduleMeals).HasConstraintName("FK_ScheduleMeal_User");

            entity.HasOne(d => d.Menu).WithMany(p => p.ScheduleMeals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ScheduleMeal_Menu");

            entity.HasOne(d => d.School).WithMany(p => p.ScheduleMeals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ScheduleMeal_School");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.SchoolId).HasName("PK__Schools__3DA4675B9A5B6663");

            entity.Property(e => e.SchoolId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<SchoolPaymentSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PK__SchoolPa__54372B1D841A88FF");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.School).WithMany(p => p.SchoolPaymentSettings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SPS_School");
        });

        modelBuilder.Entity<SchoolRevenue>(entity =>
        {
            entity.HasKey(e => e.SchoolRevenueId).HasName("PK__SchoolRe__19A82924AE3F6033");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RevenueDate).HasDefaultValueSql("(CONVERT([date],getdate()))");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.SchoolRevenueCreatedByNavigations).HasConstraintName("FK_SchoolRevenues_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.SchoolRevenues)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SchoolRevenues_School");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.SchoolRevenueUpdatedByNavigations).HasConstraintName("FK_SchoolRevenues_UpdatedBy");
        });

        modelBuilder.Entity<StagingStudent>(entity =>
        {
            entity.HasKey(e => e.StageId).HasName("PK__StagingS__03EB7AD8288A25AA");

            entity.Property(e => e.Gender).IsFixedLength();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B99F256A618");

            entity.Property(e => e.StudentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Gender).IsFixedLength();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Parent).WithMany(p => p.Students).HasConstraintName("FK_Students_Parent");

            entity.HasOne(d => d.School).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Students__School__6383C8BA");
        });

        modelBuilder.Entity<StudentAllergen>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.AllergenId }).HasName("PK__StudentA__D39D92A05909C58F");

            entity.HasOne(d => d.Allergen).WithMany(p => p.StudentAllergens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentAl__Aller__1F98B2C1");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentAllergens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentAl__Stude__1EA48E88");
        });

        modelBuilder.Entity<StudentClass>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.ClassId }).HasName("PK__StudentC__2E74B9E59884B876");

            entity.HasOne(d => d.Class).WithMany(p => p.StudentClasses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCl__Class__6C190EBB");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentClasses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCl__Stude__6B24EA82");
        });

        modelBuilder.Entity<StudentHealthRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__StudentH__FBDF78E9A5C21287");

            entity.Property(e => e.RecordId).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentHealthRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentHe__Stude__6FE99F9F");

            entity.HasOne(d => d.Year).WithMany(p => p.StudentHealthRecords).HasConstraintName("FK_StudentHealthRecords_Year");
        });

        modelBuilder.Entity<StudentImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__StudentI__7516F70CD7FB7A8F");

            entity.Property(e => e.ImageId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentImages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentIm__Stude__75A278F5");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.StudentImages).HasConstraintName("FK__StudentIm__Uploa__76969D2E");

            entity.HasOne(d => d.Year).WithMany(p => p.StudentImages).HasConstraintName("FK_StudentImages_Year");
        });

        modelBuilder.Entity<StudentImageTag>(entity =>
        {
            entity.HasKey(e => new { e.ImageId, e.TagId }).HasName("PK__StudentI__A3413896B2432C27");

            entity.HasOne(d => d.Image).WithMany(p => p.StudentImageTags)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentIm__Image__00200768");

            entity.HasOne(d => d.Tag).WithMany(p => p.StudentImageTags)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentIm__TagId__01142BA1");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tags__657CF9AC5A9CDFE5");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Tags).HasConstraintName("FK_Tags_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.Tags)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tags_School");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Teachers__EDF259646FBD4AEF");

            entity.Property(e => e.TeacherId).ValueGeneratedNever();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.TeacherNavigation).WithOne(p => p.Teacher)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teachers__Teache__5441852A");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CE3567F4F");

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Gender).HasDefaultValue(true);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LanguagePref).HasDefaultValue("vi");
            entity.Property(e => e.LockoutEnabled).HasDefaultValue(true);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__37A5467C");

            entity.HasOne(d => d.School).WithMany(p => p.Users).HasConstraintName("FK_Users_School");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.InverseUpdatedByNavigation).HasConstraintName("FK_Users_UpdBy");
        });

        modelBuilder.Entity<UserExternalLogin>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ProviderId, e.ProviderSub }).HasName("PK__UserExte__923A62771D0645F4");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Provider).WithMany(p => p.UserExternalLogins)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserExter__Provi__47DBAE45");

            entity.HasOne(d => d.User).WithMany(p => p.UserExternalLogins)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserExter__UserI__46E78A0C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
