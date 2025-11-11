using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Domain.Entities.fridge;
using SMMS.Domain.Entities.inventory;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.purchasing;
using SMMS.Domain.Entities.rag;
using SMMS.Domain.Entities.school;
using SMMS.Domain.Entities.Logs;
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
        => optionsBuilder.UseSqlServer("server =MSI\\MSSQLSERVER1; database=EduMeal;Trusted_Connection=True; TrustServerCertificate=True;Connection Timeout=120;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.HasKey(e => e.YearId).HasName("PK__Academic__C33A18CD86F82F7B");

            entity.ToTable("AcademicYears", "school");

            entity.HasIndex(e => e.YearName, "UQ__Academic__294C4DA9CB04D9F8").IsUnique();

            entity.Property(e => e.YearName).HasMaxLength(20);

            entity.HasOne(d => d.School).WithMany(p => p.AcademicYears)
                .HasForeignKey(d => d.SchoolId)
                .HasConstraintName("FK_AcademicYears_School");
        });

        modelBuilder.Entity<Allergen>(entity =>
        {
            entity.HasKey(e => e.AllergenId).HasName("PK__Allergen__158B939F52D1CF1F");

            entity.ToTable("Allergens", "nutrition");

            entity.HasIndex(e => e.AllergenName, "UQ__Allergen__7D98861991B82E2B").IsUnique();

            entity.Property(e => e.AllergenInfo).HasMaxLength(300);
            entity.Property(e => e.AllergenMatter).HasMaxLength(500);
            entity.Property(e => e.AllergenName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Allergens)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Allergens_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.Allergens)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Allergens_School");
        });

        modelBuilder.Entity<AllergeticIngredient>(entity =>
        {
            entity.HasKey(e => new { e.IngredientId, e.AllergenId }).HasName("PK__Allerget__5FF60B6329475222");

            entity.ToTable("AllergeticIngredients", "nutrition");

            entity.Property(e => e.HandlingNotes).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ReactionNotes).HasMaxLength(500);
            entity.Property(e => e.SeverityLevel).HasMaxLength(10);

            entity.HasOne(d => d.Allergen).WithMany(p => p.AllergeticIngredients)
                .HasForeignKey(d => d.AllergenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Allergeti__Aller__114A936A");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.AllergeticIngredients)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Allergeti__Ingre__10566F31");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69261CE0F4376A");

            entity.ToTable("Attendance", "school");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Reason).HasMaxLength(300);

            entity.HasOne(d => d.NotifiedByNavigation).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.NotifiedBy)
                .HasConstraintName("FK__Attendanc__Notif__76619304");

            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Stude__756D6ECB");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AuditLog__5E5486485A15F79D");

            entity.ToTable("AuditLogs", "logs");

            entity.Property(e => e.ActionDesc).HasMaxLength(100);
            entity.Property(e => e.ActionType).HasMaxLength(100);
            entity.Property(e => e.AttributeNamne).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.RecordId).HasMaxLength(64);
            entity.Property(e => e.TableName).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLogs_User");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classes__CB1927C017263F51");

            entity.ToTable("Classes", "school");

            entity.HasIndex(e => e.TeacherId, "UQ__Classes__EDF2596541A6ADCD").IsUnique();

            entity.Property(e => e.ClassId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ClassName).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.School).WithMany(p => p.Classes)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Classes__SchoolI__5AEE82B9");

            entity.HasOne(d => d.Teacher).WithOne(p => p.Class)
                .HasForeignKey<Class>(d => d.TeacherId)
                .HasConstraintName("FK__Classes__Teacher__5CD6CB2B");

            entity.HasOne(d => d.Year).WithMany(p => p.Classes)
                .HasForeignKey(d => d.YearId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Classes__YearId__5BE2A6F2");
        });

        modelBuilder.Entity<DailyMeal>(entity =>
        {
            entity.HasKey(e => e.DailyMealId).HasName("PK__DailyMea__4325CAFBA4B6DD51");

            entity.ToTable("DailyMeals", "foodmenu");

            entity.HasIndex(e => new { e.ScheduleMealId, e.MealDate, e.MealType }, "UX_DailyMeals").IsUnique();

            entity.Property(e => e.MealType).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(300);

            entity.HasOne(d => d.ScheduleMeal).WithMany(p => p.DailyMeals)
                .HasForeignKey(d => d.ScheduleMealId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DailyMeals_Schedule");
        });

        modelBuilder.Entity<ExternalProvider>(entity =>
        {
            entity.HasKey(e => e.ProviderId).HasName("PK__External__B54C687DD554857D");

            entity.ToTable("ExternalProviders", "auth");

            entity.HasIndex(e => e.ProviderName, "UQ__External__7D057CE5E96A1F34").IsUnique();

            entity.Property(e => e.ProviderName).HasMaxLength(50);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDD6A7DE8A0B");

            entity.ToTable("Feedbacks", "foodmenu");

            entity.Property(e => e.Content).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TargetRef).HasMaxLength(50);
            entity.Property(e => e.TargetType).HasMaxLength(20);

            entity.HasOne(d => d.DailyMeal).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.DailyMealId)
                .HasConstraintName("FK_Feedbacks_DailyMeal");

            entity.HasOne(d => d.Sender).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedbacks__Sende__43D61337");
        });

        modelBuilder.Entity<FoodInFridge>(entity =>
        {
            entity.HasKey(e => e.SampleId).HasName("PK__FoodInFr__8B99EC6AEF66D569");

            entity.ToTable("FoodInFridge", "fridge");

            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.StoredAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TemperatureC).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.FoodInFridgeDeletedByNavigations)
                .HasForeignKey(d => d.DeletedBy)
                .HasConstraintName("FK_FIF_DeletedBy");

            entity.HasOne(d => d.Food).WithMany(p => p.FoodInFridges)
                .HasForeignKey(d => d.FoodId)
                .HasConstraintName("FK_FIF_Food");

            entity.HasOne(d => d.Menu).WithMany(p => p.FoodInFridges)
                .HasForeignKey(d => d.MenuId)
                .HasConstraintName("FK_FIF_Menu");

            entity.HasOne(d => d.School).WithMany(p => p.FoodInFridges)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FIF_School");

            entity.HasOne(d => d.StoredByNavigation).WithMany(p => p.FoodInFridgeStoredByNavigations)
                .HasForeignKey(d => d.StoredBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FIF_StoredBy");

            entity.HasOne(d => d.Year).WithMany(p => p.FoodInFridges)
                .HasForeignKey(d => d.YearId)
                .HasConstraintName("FK_FIF_Year");

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
                        j.HasKey("SampleId", "IngredientId").HasName("PK__Ingredie__3073074F273297B9");
                        j.ToTable("IngredientInFridge", "fridge");
                    });
        });

        modelBuilder.Entity<FoodItem>(entity =>
        {
            entity.HasKey(e => e.FoodId).HasName("PK__FoodItem__856DB3EB6DE096E8");

            entity.ToTable("FoodItems", "nutrition");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FoodDesc).HasMaxLength(500);
            entity.Property(e => e.FoodName).HasMaxLength(150);
            entity.Property(e => e.FoodType).HasMaxLength(150);
            entity.Property(e => e.ImageUrl).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.FoodItems)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_FoodItems_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.FoodItems)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FoodItems_School");
        });

        modelBuilder.Entity<FoodItemIngredient>(entity =>
        {
            entity.HasKey(e => new { e.FoodId, e.IngredientId }).HasName("PK__FoodItem__3E8758CEBF71308D");

            entity.ToTable("FoodItemIngredients", "nutrition");

            entity.Property(e => e.QuantityGram).HasColumnType("decimal(9, 2)");

            entity.HasOne(d => d.Food).WithMany(p => p.FoodItemIngredients)
                .HasForeignKey(d => d.FoodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FoodItemI__FoodI__19DFD96B");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.FoodItemIngredients)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FoodItemI__Ingre__1AD3FDA4");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.IngredientId).HasName("PK__Ingredie__BEAEB25A38602579");

            entity.ToTable("Ingredients", "nutrition");

            entity.Property(e => e.CarbG).HasColumnType("decimal(7, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.EnergyKcal).HasColumnType("decimal(7, 2)");
            entity.Property(e => e.FatG).HasColumnType("decimal(7, 2)");
            entity.Property(e => e.IngredientName).HasMaxLength(100);
            entity.Property(e => e.IngredientType).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ProteinG).HasColumnType("decimal(7, 2)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Ingredients)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Ingredients_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.Ingredients)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ingredients_School");
        });

        modelBuilder.Entity<IngredientAlternative>(entity =>
        {
            entity.HasKey(e => new { e.IngredientId, e.AltIngredientId }).HasName("PK__Ingredie__790D4520E69C41E6");

            entity.ToTable("IngredientAlternatives", "nutrition");

            entity.Property(e => e.ConfidenceScore).HasColumnType("decimal(4, 3)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ReasonCode).HasMaxLength(30);
            entity.Property(e => e.SourceType).HasMaxLength(20);

            entity.HasOne(d => d.AltIngredient).WithMany(p => p.IngredientAlternativeAltIngredients)
                .HasForeignKey(d => d.AltIngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ingredien__AltIn__0697FACD");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.IngredientAlternatives)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Ingredien__Creat__078C1F06");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.IngredientAlternativeIngredients)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ingredien__Ingre__05A3D694");
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Inventor__727E838B94F95DE1");

            entity.ToTable("InventoryItems", "inventory");

            entity.Property(e => e.BatchNo).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ItemName).HasMaxLength(150);
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Origin).HasMaxLength(255);
            entity.Property(e => e.QuantityGram).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryItems)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_InventoryItems_CreatedBy");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.InventoryItems)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Ingre__4A8310C6");

            entity.HasOne(d => d.School).WithMany(p => p.InventoryItems)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryItems_School");
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.TransId).HasName("PK__Inventor__9E5DDB3C2218A76C");

            entity.ToTable("InventoryTransactions", "inventory");

            entity.Property(e => e.QuantityGram).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.TransDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TransType).HasMaxLength(10);

            entity.HasOne(d => d.Item).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__ItemI__503BEA1C");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAB5D70DDAAF");

            entity.ToTable("Invoices", "billing");

            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Student).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__Studen__607251E5");
        });

        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId).HasName("PK__LoginAtt__891A68E601D6D872");

            entity.ToTable("LoginAttempts", "auth");

            entity.Property(e => e.AttemptAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserName).HasMaxLength(255);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menus__C99ED230234C123F");

            entity.ToTable("Menus", "foodmenu");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsVisible).HasDefaultValue(true);

            entity.HasOne(d => d.ConfirmedByNavigation).WithMany(p => p.Menus)
                .HasForeignKey(d => d.ConfirmedBy)
                .HasConstraintName("FK_Menus_Confirm");

            entity.HasOne(d => d.School).WithMany(p => p.Menus)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Menus_School");

            entity.HasOne(d => d.Year).WithMany(p => p.Menus)
                .HasForeignKey(d => d.YearId)
                .HasConstraintName("FK_Menus_Year");
        });

        modelBuilder.Entity<MenuDay>(entity =>
        {
            entity.HasKey(e => e.MenuDayId).HasName("PK__MenuDays__48283E547729CE1C");

            entity.ToTable("MenuDays", "foodmenu");

            entity.HasIndex(e => new { e.MenuId, e.DayOfWeek, e.MealType }, "IX_MenuDays_Menu");

            entity.HasIndex(e => new { e.MenuId, e.DayOfWeek, e.MealType }, "UQ_MenuDays").IsUnique();

            entity.Property(e => e.MealType).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(300);

            entity.HasOne(d => d.Menu).WithMany(p => p.MenuDays)
                .HasForeignKey(d => d.MenuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuDays_Menus");
        });

        modelBuilder.Entity<MenuDayFoodItem>(entity =>
        {
            entity.HasKey(e => new { e.MenuDayId, e.FoodId });

            entity.ToTable("MenuDayFoodItems", "foodmenu");

            entity.HasOne(d => d.Food).WithMany(p => p.MenuDayFoodItems)
                .HasForeignKey(d => d.FoodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuDayFoodItems_Food");

            entity.HasOne(d => d.MenuDay).WithMany(p => p.MenuDayFoodItems)
                .HasForeignKey(d => d.MenuDayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuDayFoodItems_MenuDay");
        });

        modelBuilder.Entity<MenuFoodItem>(entity =>
        {
            entity.HasKey(e => new { e.DailyMealId, e.FoodId });

            entity.ToTable("MenuFoodItems", "foodmenu");

            entity.HasOne(d => d.DailyMeal).WithMany(p => p.MenuFoodItems)
                .HasForeignKey(d => d.DailyMealId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuFoodItems_DailyMeals");

            entity.HasOne(d => d.Food).WithMany(p => p.MenuFoodItems)
                .HasForeignKey(d => d.FoodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuFoodItems_FoodItems");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__notifica__20CF2E121ABA843B");

            entity.ToTable("notifications", "billing");

            entity.Property(e => e.AttachmentUrl).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ScheduleCron).HasMaxLength(100);
            entity.Property(e => e.SendType).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Sender).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK__notificat__Sende__6CD828CA");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => new { e.NotificationId, e.UserId }).HasName("PK__Notifica__F1B7A2D6B6AF74FA");

            entity.ToTable("NotificationRecipients", "billing");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Notif__70A8B9AE");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__719CDDE7");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A3806C0B5B4");

            entity.ToTable("Payments", "billing");

            entity.Property(e => e.ExpectedAmount)
                .HasDefaultValue(600m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Method).HasMaxLength(20);
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaidAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PaymentContent).HasMaxLength(500);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(10)
                .HasDefaultValue("pending");
            entity.Property(e => e.ReferenceNo).HasMaxLength(100);

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Invoic__681373AD");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Purchase__C3905BCF41CC78A6");

            entity.ToTable("PurchaseOrders", "purchasing");

            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PurchaseOrderStatus).HasMaxLength(50);
            entity.Property(e => e.SupplierName).HasMaxLength(255);

            entity.HasOne(d => d.Plan).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK_PurchaseOrders_Plans");

            entity.HasOne(d => d.School).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrders_Schools");

            entity.HasOne(d => d.StaffInChargedNavigation).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.StaffInCharged)
                .HasConstraintName("FK_PurchaseOrders_Users");
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.HasKey(e => e.LinesId).HasName("PK__Purchase__728C596DD705B1AD");

            entity.ToTable("PurchaseOrderLines", "purchasing");

            entity.Property(e => e.BatchNo).HasMaxLength(100);
            entity.Property(e => e.Origin).HasMaxLength(255);
            entity.Property(e => e.QuantityGram).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.PurchaseOrderLines)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrderLines_Ingredients");

            entity.HasOne(d => d.Order).WithMany(p => p.PurchaseOrderLines)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrderLines_Orders");

            entity.HasOne(d => d.User).WithMany(p => p.PurchaseOrderLines)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrderLines_Users");
        });

        modelBuilder.Entity<PurchasePlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Purchase__755C22B7F194E585");

            entity.ToTable("PurchasePlans", "purchasing");

            entity.Property(e => e.GeneratedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PlanStatus).HasMaxLength(20);

            entity.HasOne(d => d.ConfirmedByNavigation).WithMany(p => p.PurchasePlanConfirmedByNavigations)
                .HasForeignKey(d => d.ConfirmedBy)
                .HasConstraintName("FK_PurchasePlans_ConfirmedBy");

            entity.HasOne(d => d.Menu).WithMany(p => p.PurchasePlans)
                .HasForeignKey(d => d.MenuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchasePlans_Menus");

            entity.HasOne(d => d.Staff).WithMany(p => p.PurchasePlanStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchasePlans_Staff");
        });

        modelBuilder.Entity<PurchasePlanLine>(entity =>
        {
            entity.HasKey(e => new { e.PlanId, e.IngredientId }).HasName("PK__Purchase__CEB6C992484F194C");

            entity.ToTable("PurchasePlanLines", "purchasing");

            entity.Property(e => e.RqQuanityGram).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.PurchasePlanLines)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseP__Ingre__5BAD9CC8");

            entity.HasOne(d => d.Plan).WithMany(p => p.PurchasePlanLines)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseP__PlanI__5AB9788F");
        });

        modelBuilder.Entity<RagRequestAllergen>(entity =>
        {
            entity.HasKey(e => new { e.RequestId, e.AllergenId }).HasName("PK__RAG_Requ__D2F0E8438B8625BF");

            entity.ToTable("RAG_RequestAllergens", "rag");

            entity.HasIndex(e => e.RequestId, "IX_RAG_RA_Request");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Allergen).WithMany(p => p.RagRequestAllergens)
                .HasForeignKey(d => d.AllergenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_RA_Allergen");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RagRequestAllergens)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_RA_Created");

            entity.HasOne(d => d.School).WithMany(p => p.RagRequestAllergens)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_RA_School");
        });

        modelBuilder.Entity<RagRequestInput>(entity =>
        {
            entity.HasKey(e => e.RagInputId).HasName("PK__RAG_Requ__94C425D2F9B34680");

            entity.ToTable("RAG_RequestInput", "rag");

            entity.HasIndex(e => e.RequestId, "IX_RAG_RI_Request");

            entity.HasIndex(e => new { e.SchoolId, e.CreatedAt }, "IX_RAG_RI_School");

            entity.HasIndex(e => e.RequestId, "UX_RAG_RI_Request_Header")
                .IsUnique()
                .HasFilter("([IsHeader]=(1))");

            entity.Property(e => e.RagInputId).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IngredientName).HasMaxLength(150);
            entity.Property(e => e.Notes).HasMaxLength(300);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RagRequestInputs)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_RI_Created");

            entity.HasOne(d => d.School).WithMany(p => p.RagRequestInputs)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_RI_School");
        });

        modelBuilder.Entity<RagSuggestedFoodItem>(entity =>
        {
            entity.HasKey(e => e.SuggestedFoodItemId).HasName("PK__RAG_Sugg__5F9F55F6532B1205");

            entity.ToTable("RAG_SuggestedFoodItems", "rag");

            entity.HasIndex(e => e.RequestId, "IX_RAG_SF_Request");

            entity.HasIndex(e => new { e.SchoolId, e.CreatedAt }, "IX_RAG_SF_School");

            entity.Property(e => e.SuggestedFoodItemId).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.ConfidenceScore).HasColumnType("decimal(4, 3)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FoodName).HasMaxLength(200);
            entity.Property(e => e.ImageUrl).HasMaxLength(300);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RagSuggestedFoodItems)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_SF_Created");

            entity.HasOne(d => d.School).WithMany(p => p.RagSuggestedFoodItems)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_SF_School");
        });

        modelBuilder.Entity<RagSuggestedFoodItemIngredient>(entity =>
        {
            entity.HasKey(e => new { e.SuggestedFoodItemId, e.SuggestedIngredientId }).HasName("PK__RAG_Sugg__F988B1ED132698AE");

            entity.ToTable("RAG_SuggestedFoodItemIngredients", "rag");

            entity.HasIndex(e => e.RequestId, "IX_RAG_SFII_Request");

            entity.HasIndex(e => new { e.SchoolId, e.CreatedAt }, "IX_RAG_SFII_School");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.QuantityGram).HasColumnType("decimal(9, 2)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RagSuggestedFoodItemIngredients)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_SFII_Created");

            entity.HasOne(d => d.School).WithMany(p => p.RagSuggestedFoodItemIngredients)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_SFII_School");

            entity.HasOne(d => d.SuggestedFoodItem).WithMany(p => p.RagSuggestedFoodItemIngredients)
                .HasForeignKey(d => d.SuggestedFoodItemId)
                .HasConstraintName("FK_RAG_SFII_Food");

            entity.HasOne(d => d.SuggestedIngredient).WithMany(p => p.RagSuggestedFoodItemIngredients)
                .HasForeignKey(d => d.SuggestedIngredientId)
                .HasConstraintName("FK_RAG_SFII_Ingredient");
        });

        modelBuilder.Entity<RagSuggestedIngredient>(entity =>
        {
            entity.HasKey(e => e.SuggestedIngredientId).HasName("PK__RAG_Sugg__617E41BB41D3346A");

            entity.ToTable("RAG_SuggestedIngredients", "rag");

            entity.HasIndex(e => e.RequestId, "IX_RAG_SI_Request");

            entity.HasIndex(e => new { e.SchoolId, e.CreatedAt }, "IX_RAG_SI_School");

            entity.Property(e => e.SuggestedIngredientId).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IngredientName).HasMaxLength(150);
            entity.Property(e => e.Notes).HasMaxLength(300);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RagSuggestedIngredients)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_SI_Created");

            entity.HasOne(d => d.School).WithMany(p => p.RagSuggestedIngredients)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RAG_SI_School");

            entity.HasOne(d => d.SourceIngredient).WithMany(p => p.RagSuggestedIngredients)
                .HasForeignKey(d => d.SourceIngredientId)
                .HasConstraintName("FK_RAG_SI_SourceIngredient");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__RefreshT__F5845E39376A48EF");

            entity.ToTable("RefreshTokens", "auth");

            entity.HasIndex(e => e.Token, "UQ__RefreshT__1EB4F817F3E0675F").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CreatedByIp).HasMaxLength(45);
            entity.Property(e => e.RevokedByIp).HasMaxLength(45);
            entity.Property(e => e.Token).HasMaxLength(250);

            entity.HasOne(d => d.ReplacedBy).WithMany(p => p.InverseReplacedBy)
                .HasForeignKey(d => d.ReplacedById)
                .HasConstraintName("FK__RefreshTo__Repla__3D5E1FD2");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefreshTo__UserI__3C69FB99");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AB88957A7");

            entity.ToTable("Roles", "auth");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160FA2068F6").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleDesc).HasMaxLength(200);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<ScheduleMeal>(entity =>
        {
            entity.HasKey(e => e.ScheduleMealId).HasName("PK__Schedule__7F2713EE0FFBA06A");

            entity.ToTable("ScheduleMeal", "foodmenu");

            entity.HasIndex(e => e.MenuId, "IX_ScheduleMeal_Menu");

            entity.HasIndex(e => new { e.SchoolId, e.WeekStart, e.WeekEnd }, "IX_ScheduleMeal_SchoolWeek");

            entity.HasIndex(e => new { e.SchoolId, e.WeekNo, e.YearNo }, "UQ_ScheduleMeal_School_WeekNoYear").IsUnique();

            entity.HasIndex(e => new { e.SchoolId, e.WeekStart }, "UQ_ScheduleMeal_School_WeekStart").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Notes).HasMaxLength(300);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ScheduleMeals)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_ScheduleMeal_User");

            entity.HasOne(d => d.Menu).WithMany(p => p.ScheduleMeals)
                .HasForeignKey(d => d.MenuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ScheduleMeal_Menu");

            entity.HasOne(d => d.School).WithMany(p => p.ScheduleMeals)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ScheduleMeal_School");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.SchoolId).HasName("PK__Schools__3DA4675B534F8518");

            entity.ToTable("Schools", "school");

            entity.Property(e => e.SchoolId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ContactEmail).HasMaxLength(150);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Hotline).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SchoolAddress).HasMaxLength(200);
            entity.Property(e => e.SchoolName).HasMaxLength(150);
        });

        modelBuilder.Entity<StagingStudent>(entity =>
        {
            entity.HasKey(e => e.StageId).HasName("PK__StagingS__03EB7AD81A4790B1");

            entity.ToTable("StagingStudents", "school");

            entity.Property(e => e.ClassName).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ParentEmail1).HasMaxLength(255);
            entity.Property(e => e.ParentEmail2).HasMaxLength(255);
            entity.Property(e => e.ParentRelation1).HasMaxLength(20);
            entity.Property(e => e.ParentRelation2).HasMaxLength(20);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.RowErrors).HasMaxLength(1000);
            entity.Property(e => e.RowStatus).HasMaxLength(20);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B997D446F8E");

            entity.ToTable("Students", "school");

            entity.Property(e => e.StudentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AvatarUrl).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RelationName).HasMaxLength(50);

            entity.HasOne(d => d.Parent).WithMany(p => p.Students)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_Students_Parent");

            entity.HasOne(d => d.School).WithMany(p => p.Students)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Students__School__6383C8BA");
        });

        modelBuilder.Entity<StudentAllergen>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.AllergenId }).HasName("PK__StudentA__D39D92A0636FC91D");

            entity.ToTable("StudentAllergens", "nutrition");

            entity.Property(e => e.HandlingNotes).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ReactionNotes).HasMaxLength(500);

            entity.HasOne(d => d.Allergen).WithMany(p => p.StudentAllergens)
                .HasForeignKey(d => d.AllergenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentAl__Aller__1EA48E88");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentAllergens)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentAl__Stude__1DB06A4F");
        });

        modelBuilder.Entity<StudentClass>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.ClassId }).HasName("PK__StudentC__2E74B9E5BEBB23BE");

            entity.ToTable("StudentClasses", "school");

            entity.HasOne(d => d.Class).WithMany(p => p.StudentClasses)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCl__Class__6C190EBB");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentClasses)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCl__Stude__6B24EA82");
        });

        modelBuilder.Entity<StudentHealthRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__StudentH__FBDF78E9C56BA393");

            entity.ToTable("StudentHealthRecords", "school");

            entity.Property(e => e.RecordId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.HeightCm).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.WeightKg).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentHealthRecords)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentHe__Stude__6FE99F9F");

            entity.HasOne(d => d.Year).WithMany(p => p.StudentHealthRecords)
                .HasForeignKey(d => d.YearId)
                .HasConstraintName("FK_StudentHealthRecords_Year");
        });

        modelBuilder.Entity<StudentImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__StudentI__7516F70C6994F875");

            entity.ToTable("StudentImages", "school");

            entity.Property(e => e.ImageId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Caption).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ImageUrl).HasMaxLength(300);

            entity.HasOne(d => d.Student).WithMany(p => p.StudentImages)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentIm__Stude__75A278F5");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.StudentImages)
                .HasForeignKey(d => d.UploadedBy)
                .HasConstraintName("FK__StudentIm__Uploa__76969D2E");

            entity.HasOne(d => d.Year).WithMany(p => p.StudentImages)
                .HasForeignKey(d => d.YearId)
                .HasConstraintName("FK_StudentImages_Year");
        });

        modelBuilder.Entity<StudentImageTag>(entity =>
        {
            entity.HasKey(e => new { e.ImageId, e.TagId }).HasName("PK__StudentI__A34138962773EE7D");

            entity.ToTable("StudentImageTags", "school");

            entity.Property(e => e.TagNotes).HasMaxLength(255);

            entity.HasOne(d => d.Image).WithMany(p => p.StudentImageTags)
                .HasForeignKey(d => d.ImageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentIm__Image__00200768");

            entity.HasOne(d => d.Tag).WithMany(p => p.StudentImageTags)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentIm__TagId__01142BA1");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tags__657CF9AC25082328");

            entity.ToTable("Tags", "school");

            entity.HasIndex(e => e.TagName, "UQ__Tags__BDE0FD1D2461BFC2").IsUnique();

            entity.Property(e => e.TagName).HasMaxLength(50);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Tags)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Tags_CreatedBy");

            entity.HasOne(d => d.School).WithMany(p => p.Tags)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tags_School");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Teachers__EDF2596404061A69");

            entity.ToTable("Teachers", "school");

            entity.HasIndex(e => e.EmployeeCode, "UQ__Teachers__1F642548E06ECF24").IsUnique();

            entity.Property(e => e.TeacherId).ValueGeneratedNever();
            entity.Property(e => e.EmployeeCode).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.TeacherNavigation).WithOne(p => p.Teacher)
                .HasForeignKey<Teacher>(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teachers__Teache__5441852A");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CCADFF753");

            entity.ToTable("Users", "auth");

            entity.HasIndex(e => e.Phone, "UQ_Users_Phone").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534F158BD91").IsUnique();

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IdentityNo).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LanguagePref)
                .HasMaxLength(10)
                .HasDefaultValue("vi");
            entity.Property(e => e.LockoutEnabled).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__37A5467C");

            entity.HasOne(d => d.School).WithMany(p => p.Users)
                .HasForeignKey(d => d.SchoolId)
                .HasConstraintName("FK_Users_School");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.InverseUpdatedByNavigation)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Users_UpdBy");
        });

        modelBuilder.Entity<UserExternalLogin>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ProviderId, e.ProviderSub }).HasName("PK__UserExte__923A6277033FFA70");

            entity.ToTable("UserExternalLogins", "auth");

            entity.Property(e => e.ProviderSub).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);

            entity.HasOne(d => d.Provider).WithMany(p => p.UserExternalLogins)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserExter__Provi__47DBAE45");

            entity.HasOne(d => d.User).WithMany(p => p.UserExternalLogins)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserExter__UserI__46E78A0C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
