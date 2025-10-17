using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Models.auth;
using SMMS.Domain.Models.billing;
using SMMS.Domain.Models.foodmenu;
using SMMS.Domain.Models.fridge;
using SMMS.Domain.Models.inventory;
using SMMS.Domain.Models.logs;
using SMMS.Domain.Models.nutrition;
using SMMS.Domain.Models.purchasing;
using SMMS.Domain.Models.rag;
using SMMS.Domain.Models.school;

namespace SMMS.Persistence.DbContextSite;

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

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }

    public virtual DbSet<PurchasePlan> PurchasePlans { get; set; }

    public virtual DbSet<PurchasePlanLine> PurchasePlanLines { get; set; }

    public virtual DbSet<RagRequestAllergen> RagRequestAllergens { get; set; }

    public virtual DbSet<RagRequestInput> RagRequestInputs { get; set; }

    public virtual DbSet<RagSuggestedFoodItem> RagSuggestedFoodItems { get; set; }

    public virtual DbSet<RagSuggestedFoodItemIngredient> RagSuggestedFoodItemIngredients { get; set; }

    public virtual DbSet<RagSuggestedIngredient> RagSuggestedIngredients { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ScheduleMeal> ScheduleMeals { get; set; }

    public virtual DbSet<School> Schools { get; set; }

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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQL2019;Database=EduMeal;User Id=sa;Password=123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.HasKey(e => e.YearId).HasName("PK__Academic__C33A18CD8A7DC0CF");

            entity.HasOne(d => d.School).WithMany(p => p.AcademicYears).HasConstraintName("FK_AcademicYears_School");
        });

        modelBuilder.Entity<Allergen>(entity =>
        {
            entity.HasKey(e => e.AllergenId).HasName("PK__Allergen__158B939F2972CFA3");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Allergens).HasConstraintName("FK_Allergens_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.Allergens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Allergens_School");
        });

        modelBuilder.Entity<AllergeticIngredient>(entity =>
        {
            entity.HasKey(e => new { e.IngredientId, e.AllergenId }).HasName("PK__Allerget__5FF60B63B4628613");

            entity.HasOne(d => d.Allergen).WithMany(p => p.AllergeticIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Allergeti__Aller__245D67DE");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.AllergeticIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Allergeti__Ingre__236943A5");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69261CBE0C10D3");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.NotifiedByNavigation).WithMany(p => p.Attendances).HasConstraintName("FK__Attendanc__Notif__09746778");

            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Stude__0880433F");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AuditLog__5E548648B43F6EF6");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLogs_User");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classes__CB1927C034A689D6");

            entity.Property(e => e.ClassId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.School).WithMany(p => p.Classes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Classes__SchoolI__6E01572D");

            entity.HasOne(d => d.Teacher).WithOne(p => p.Class).HasConstraintName("FK__Classes__Teacher__6FE99F9F");

            entity.HasOne(d => d.Year).WithMany(p => p.Classes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Classes__YearId__6EF57B66");
        });

        modelBuilder.Entity<DailyMeal>(entity =>
        {
            entity.HasKey(e => e.DailyMealId).HasName("PK__DailyMea__4325CAFB7AE3BBE4");

            entity.HasOne(d => d.ScheduleMeal).WithMany(p => p.DailyMeals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DailyMeals_Schedule");
        });

        modelBuilder.Entity<ExternalProvider>(entity =>
        {
            entity.HasKey(e => e.ProviderId).HasName("PK__External__B54C687D8250975E");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDD61994BEDC");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.DailyMeal).WithMany(p => p.Feedbacks).HasConstraintName("FK_Feedbacks_DailyMeal");

            entity.HasOne(d => d.Sender).WithMany(p => p.Feedbacks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedbacks__Sende__56E8E7AB");
        });

        modelBuilder.Entity<FoodInFridge>(entity =>
        {
            entity.HasKey(e => e.SampleId).HasName("PK__FoodInFr__8B99EC6AD0ED5F80");

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
                        j.HasKey("SampleId", "IngredientId").HasName("PK__Ingredie__3073074F6EECBCE3");
                        j.ToTable("IngredientInFridge", "fridge");
                    });
        });

        modelBuilder.Entity<FoodItem>(entity =>
        {
            entity.HasKey(e => e.FoodId).HasName("PK__FoodItem__856DB3EBABCA9EAE");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.FoodItems).HasConstraintName("FK_FoodItems_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.FoodItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FoodItems_School");
        });

        modelBuilder.Entity<FoodItemIngredient>(entity =>
        {
            entity.HasKey(e => new { e.FoodId, e.IngredientId }).HasName("PK__FoodItem__3E8758CEA7A3A211");

            entity.HasOne(d => d.Food).WithMany(p => p.FoodItemIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FoodItemI__FoodI__2CF2ADDF");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.FoodItemIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FoodItemI__Ingre__2DE6D218");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.IngredientId).HasName("PK__Ingredie__BEAEB25A5B3B520E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Ingredients).HasConstraintName("FK_Ingredients_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.Ingredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ingredients_School");
        });

        modelBuilder.Entity<IngredientAlternative>(entity =>
        {
            entity.HasKey(e => new { e.IngredientId, e.AltIngredientId }).HasName("PK__Ingredie__790D4520603642C1");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.AltIngredient).WithMany(p => p.IngredientAlternativeAltIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ingredien__AltIn__19AACF41");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.IngredientAlternatives).HasConstraintName("FK__Ingredien__Creat__1A9EF37A");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.IngredientAlternativeIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ingredien__Ingre__18B6AB08");
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Inventor__727E838B6F321E2E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryItems).HasConstraintName("FK_InventoryItems_CreatedBy");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.InventoryItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Ingre__5D95E53A");

            entity.HasOne(d => d.School).WithMany(p => p.InventoryItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryItems_School");
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.TransId).HasName("PK__Inventor__9E5DDB3C033AEA9C");

            entity.Property(e => e.TransDate).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Item).WithMany(p => p.InventoryTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__ItemI__634EBE90");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAB577A4937B");

            entity.HasOne(d => d.Student).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__Studen__73852659");
        });

        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId).HasName("PK__LoginAtt__891A68E66B6A265D");

            entity.Property(e => e.AttemptAt).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menus__C99ED2309A5BD57B");

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
            entity.HasKey(e => e.MenuDayId).HasName("PK__MenuDays__48283E54594949DD");

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

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__notifica__20CF2E12C704AB4D");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Sender).WithMany(p => p.Notifications).HasConstraintName("FK__notificat__Sende__7FEAFD3E");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => new { e.NotificationId, e.UserId }).HasName("PK__Notifica__F1B7A2D6F92104C9");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationRecipients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Notif__03BB8E22");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationRecipients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__04AFB25B");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A38893A90F6");

            entity.Property(e => e.ExpectedAmount).HasDefaultValue(600m);
            entity.Property(e => e.PaidAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PaymentStatus).HasDefaultValue("pending");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Invoic__7B264821");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Purchase__C3905BCF4CDDEB94");

            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Plan).WithMany(p => p.PurchaseOrders).HasConstraintName("FK_PurchaseOrders_Plans");

            entity.HasOne(d => d.School).WithMany(p => p.PurchaseOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrders_Schools");

            entity.HasOne(d => d.StaffInChargedNavigation).WithMany(p => p.PurchaseOrders).HasConstraintName("FK_PurchaseOrders_Users");
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.HasKey(e => e.LinesId).HasName("PK__Purchase__728C596DB81E4AB8");

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
            entity.HasKey(e => e.PlanId).HasName("PK__Purchase__755C22B70D5AC075");

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
            entity.HasKey(e => new { e.PlanId, e.IngredientId }).HasName("PK__Purchase__CEB6C99244E74BEE");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.PurchasePlanLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseP__Ingre__6EC0713C");

            entity.HasOne(d => d.Plan).WithMany(p => p.PurchasePlanLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseP__PlanI__6DCC4D03");
        });

        modelBuilder.Entity<RagRequestAllergen>(entity =>
        {
            entity.HasKey(e => new { e.RequestId, e.AllergenId }).HasName("PK__RAG_Requ__D2F0E8431C8979AA");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Allergen).WithMany(p => p.RagRequestAllergens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_RA_Allergen");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RagRequestAllergens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_RA_Created");

            entity.HasOne(d => d.School).WithMany(p => p.RagRequestAllergens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_RA_School");
        });

        modelBuilder.Entity<RagRequestInput>(entity =>
        {
            entity.HasKey(e => e.RagInputId).HasName("PK__RAG_Requ__94C425D207477037");

            entity.HasIndex(e => e.RequestId, "UX_RAG_RI_Request_Header")
                .IsUnique()
                .HasFilter("([IsHeader]=(1))");

            entity.Property(e => e.RagInputId).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RagRequestInputs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_RI_Created");

            entity.HasOne(d => d.School).WithMany(p => p.RagRequestInputs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_RI_School");
        });

        modelBuilder.Entity<RagSuggestedFoodItem>(entity =>
        {
            entity.HasKey(e => e.SuggestedFoodItemId).HasName("PK__RAG_Sugg__5F9F55F66C6D3F56");

            entity.Property(e => e.SuggestedFoodItemId).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RagSuggestedFoodItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_SF_Created");

            entity.HasOne(d => d.School).WithMany(p => p.RagSuggestedFoodItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_SF_School");
        });

        modelBuilder.Entity<RagSuggestedFoodItemIngredient>(entity =>
        {
            entity.HasKey(e => new { e.SuggestedFoodItemId, e.SuggestedIngredientId }).HasName("PK__RAG_Sugg__F988B1ED3052054F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RagSuggestedFoodItemIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_SFII_Created");

            entity.HasOne(d => d.School).WithMany(p => p.RagSuggestedFoodItemIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_SFII_School");

            entity.HasOne(d => d.SuggestedFoodItem).WithMany(p => p.RagSuggestedFoodItemIngredients).HasConstraintName("FK_RAG_SFII_Food");

            entity.HasOne(d => d.SuggestedIngredient).WithMany(p => p.RagSuggestedFoodItemIngredients).HasConstraintName("FK_RAG_SFII_Ingredient");
        });

        modelBuilder.Entity<RagSuggestedIngredient>(entity =>
        {
            entity.HasKey(e => e.SuggestedIngredientId).HasName("PK__RAG_Sugg__617E41BB3807B64A");

            entity.Property(e => e.SuggestedIngredientId).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RagSuggestedIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_SI_Created");

            entity.HasOne(d => d.School).WithMany(p => p.RagSuggestedIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_SI_School");

            entity.HasOne(d => d.SourceIngredient).WithMany(p => p.RagSuggestedIngredients).HasConstraintName("FK_RAG_SI_SourceIngredient");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__RefreshT__F5845E395E830E63");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.ReplacedBy).WithMany(p => p.InverseReplacedBy).HasConstraintName("FK__RefreshTo__Repla__5070F446");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefreshTo__UserI__4F7CD00D");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A6D6BF837");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<ScheduleMeal>(entity =>
        {
            entity.HasKey(e => e.ScheduleMealId).HasName("PK__Schedule__7F2713EEE49CB35F");

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
            entity.HasKey(e => e.SchoolId).HasName("PK__Schools__3DA4675BB61E968C");

            entity.Property(e => e.SchoolId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<StagingStudent>(entity =>
        {
            entity.HasKey(e => e.StageId).HasName("PK__StagingS__03EB7AD8BC629D4F");

            entity.Property(e => e.Gender).IsFixedLength();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B99CC9F2A3A");

            entity.Property(e => e.StudentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Gender).IsFixedLength();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Parent).WithMany(p => p.Students).HasConstraintName("FK_Students_Parent");

            entity.HasOne(d => d.School).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Students__School__76969D2E");
        });

        modelBuilder.Entity<StudentAllergen>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.AllergenId }).HasName("PK__StudentA__D39D92A0CB3637DA");

            entity.HasOne(d => d.Allergen).WithMany(p => p.StudentAllergens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentAl__Aller__31B762FC");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentAllergens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentAl__Stude__30C33EC3");
        });

        modelBuilder.Entity<StudentClass>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.ClassId }).HasName("PK__StudentC__2E74B9E5D4781575");

            entity.HasOne(d => d.Class).WithMany(p => p.StudentClasses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCl__Class__7F2BE32F");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentClasses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCl__Stude__7E37BEF6");
        });

        modelBuilder.Entity<StudentHealthRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__StudentH__FBDF78E9E07B2EA8");

            entity.Property(e => e.RecordId).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentHealthRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentHe__Stude__02FC7413");

            entity.HasOne(d => d.Year).WithMany(p => p.StudentHealthRecords).HasConstraintName("FK_StudentHealthRecords_Year");
        });

        modelBuilder.Entity<StudentImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__StudentI__7516F70C6BB4BD13");

            entity.Property(e => e.ImageId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentImages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentIm__Stude__08B54D69");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.StudentImages).HasConstraintName("FK__StudentIm__Uploa__09A971A2");

            entity.HasOne(d => d.Year).WithMany(p => p.StudentImages).HasConstraintName("FK_StudentImages_Year");
        });

        modelBuilder.Entity<StudentImageTag>(entity =>
        {
            entity.HasKey(e => new { e.ImageId, e.TagId }).HasName("PK__StudentI__A3413896E00650A6");

            entity.HasOne(d => d.Image).WithMany(p => p.StudentImageTags)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentIm__Image__1332DBDC");

            entity.HasOne(d => d.Tag).WithMany(p => p.StudentImageTags)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentIm__TagId__14270015");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tags__657CF9ACBA715E79");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Tags).HasConstraintName("FK_Tags_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.Tags)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tags_School");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Teachers__EDF25964472A055A");

            entity.Property(e => e.TeacherId).ValueGeneratedNever();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.TeacherNavigation).WithOne(p => p.Teacher)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teachers__Teache__6754599E");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CD93F3FEC");

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LanguagePref).HasDefaultValue("vi");
            entity.Property(e => e.LockoutEnabled).HasDefaultValue(true);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__4AB81AF0");

            entity.HasOne(d => d.School).WithMany(p => p.Users).HasConstraintName("FK_Users_School");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.InverseUpdatedByNavigation).HasConstraintName("FK_Users_UpdBy");
        });

        modelBuilder.Entity<UserExternalLogin>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ProviderId, e.ProviderSub }).HasName("PK__UserExte__923A62779FFE4803");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Provider).WithMany(p => p.UserExternalLogins)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserExter__Provi__5AEE82B9");

            entity.HasOne(d => d.User).WithMany(p => p.UserExternalLogins)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserExter__UserI__59FA5E80");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
