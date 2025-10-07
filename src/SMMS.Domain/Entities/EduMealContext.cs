using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

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

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<FoodItem> FoodItems { get; set; }

    public virtual DbSet<FoodItemIngredient> FoodItemIngredients { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<InventoryItem> InventoryItems { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=XUANHAI;Database=EduMeal;User Id=haidang;Password=123;TrustServerCertificate=True;integrated security=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.HasKey(e => e.YearId).HasName("PK__Academic__C33A18CD2FD44C9F");
        });

        modelBuilder.Entity<Allergen>(entity =>
        {
            entity.HasKey(e => e.AllergenId).HasName("PK__Allergen__158B939F20395610");
        });

        modelBuilder.Entity<AllergeticIngredient>(entity =>
        {
            entity.HasKey(e => new { e.IngredientId, e.AllergenId }).HasName("PK__Allerget__5FF60B634AFE7008");

            entity.HasOne(d => d.Allergen).WithMany(p => p.AllergeticIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Allergeti__Aller__7D439ABD");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.AllergeticIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Allergeti__Ingre__7C4F7684");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69261CE0DDD46A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.NotifiedByNavigation).WithMany(p => p.Attendances).HasConstraintName("FK__Attendanc__Notif__55009F39");

            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Stude__540C7B00");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AuditLog__5E5486489FE798A7");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLogs_User");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classes__CB1927C0E8113822");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.School).WithMany(p => p.Classes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Classes__SchoolI__534D60F1");

            entity.HasOne(d => d.Teacher).WithOne(p => p.Class).HasConstraintName("FK__Classes__Teacher__5535A963");

            entity.HasOne(d => d.Year).WithMany(p => p.Classes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Classes__YearId__5441852A");
        });

        modelBuilder.Entity<DailyMeal>(entity =>
        {
            entity.HasKey(e => e.DailyMealId).HasName("PK__DailyMea__4325CAFB5029486F");

            entity.HasOne(d => d.ScheduleMeal).WithMany(p => p.DailyMeals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DailyMeals_Schedule");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDD60B3F35E0");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.DailyMeal).WithMany(p => p.Feedbacks).HasConstraintName("FK_Feedbacks_DailyMeal");

            entity.HasOne(d => d.Sender).WithMany(p => p.Feedbacks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedbacks__Sende__282DF8C2");
        });

        modelBuilder.Entity<FoodItem>(entity =>
        {
            entity.HasKey(e => e.FoodId).HasName("PK__FoodItem__856DB3EB8BF109D6");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<FoodItemIngredient>(entity =>
        {
            entity.HasKey(e => new { e.FoodId, e.IngredientId }).HasName("PK__FoodItem__3E8758CE9AC87722");

            entity.HasOne(d => d.Food).WithMany(p => p.FoodItemIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FoodItemI__FoodI__02FC7413");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.FoodItemIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FoodItemI__Ingre__03F0984C");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.IngredientId).HasName("PK__Ingredie__BEAEB25AA12A599E");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Inventor__727E838BE67C7C02");

            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.InventoryItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Ingre__2CF2ADDF");

            entity.HasOne(d => d.School).WithMany(p => p.InventoryItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Schoo__2DE6D218");
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.TransId).HasName("PK__Inventor__9E5DDB3CCEC1D795");

            entity.Property(e => e.TransDate).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Item).WithMany(p => p.InventoryTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__ItemI__32AB8735");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAB521483D5F");

            entity.HasOne(d => d.Student).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__Studen__40F9A68C");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menus__C99ED230B0BA627E");

            entity.HasOne(d => d.School).WithMany(p => p.Menus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Menus__SchoolId__0A9D95DB");
        });

        modelBuilder.Entity<MenuDay>(entity =>
        {
            entity.HasKey(e => e.MenuDayId).HasName("PK__MenuDays__48283E540CA91F88");

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
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E1238E02250");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Sender).WithMany(p => p.Notifications).HasConstraintName("FK__Notificat__Sende__4B7734FF");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => new { e.NotificationId, e.UserId }).HasName("PK__Notifica__F1B7A2D67DD7E8E8");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationRecipients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Notif__4F47C5E3");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationRecipients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__503BEA1C");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A38EA385FDA");

            entity.Property(e => e.ExpectedAmount).HasDefaultValue(600m);
            entity.Property(e => e.PaidAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Invoic__46B27FE2");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Purchase__C3905BCF556D740E");

            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Plan).WithMany(p => p.PurchaseOrders).HasConstraintName("FK_PurchaseOrders_Plans");

            entity.HasOne(d => d.School).WithMany(p => p.PurchaseOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrders_Schools");

            entity.HasOne(d => d.User).WithMany(p => p.PurchaseOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrders_Users");
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.HasKey(e => e.PolId).HasName("PK__Purchase__397C02FD6CAA4BB2");

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
            entity.HasKey(e => e.PlanId).HasName("PK__Purchase__755C22B70E787071");

            entity.Property(e => e.GeneratedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Menu).WithMany(p => p.PurchasePlans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchasePlans_Menus");

            entity.HasOne(d => d.Staff).WithMany(p => p.PurchasePlans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchasePlans_Staff");
        });

        modelBuilder.Entity<PurchasePlanLine>(entity =>
        {
            entity.HasKey(e => new { e.PlanId, e.IngredientId }).HasName("PK__Purchase__CEB6C9926C9017A7");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.PurchasePlanLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseP__Ingre__3C34F16F");

            entity.HasOne(d => d.Plan).WithMany(p => p.PurchasePlanLines)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseP__PlanI__3B40CD36");
        });

        modelBuilder.Entity<ScheduleMeal>(entity =>
        {
            entity.HasKey(e => e.ScheduleMealId).HasName("PK__Schedule__7F2713EE422BD63B");

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
            entity.HasKey(e => e.SchoolId).HasName("PK__Schools__3DA4675BF38E0A25");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<StagingStudent>(entity =>
        {
            entity.HasKey(e => e.StageId).HasName("PK__StagingS__03EB7AD89A50E339");

            entity.Property(e => e.Gender).IsFixedLength();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B99FE5131CF");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Gender).IsFixedLength();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Parent).WithMany(p => p.Students).HasConstraintName("FK_Students_Parent");

            entity.HasOne(d => d.School).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Students__School__5AEE82B9");
        });

        modelBuilder.Entity<StudentAllergen>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.AllergenId }).HasName("PK__StudentA__D39D92A009F9D9B6");

            entity.HasOne(d => d.Allergen).WithMany(p => p.StudentAllergens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentAl__Aller__07C12930");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentAllergens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentAl__Stude__06CD04F7");
        });

        modelBuilder.Entity<StudentClass>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.ClassId }).HasName("PK__StudentC__2E74B9E5C00876E9");

            entity.HasOne(d => d.Class).WithMany(p => p.StudentClasses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCl__Class__6383C8BA");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentClasses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCl__Stude__628FA481");
        });

        modelBuilder.Entity<StudentHealthRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__StudentH__FBDF78E9E5E24390");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentHealthRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentHe__Stude__66603565");
        });

        modelBuilder.Entity<StudentImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__StudentI__7516F70CEE990AB8");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentImages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentIm__Stude__6A30C649");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.StudentImages).HasConstraintName("FK__StudentIm__Uploa__6B24EA82");
        });

        modelBuilder.Entity<StudentImageTag>(entity =>
        {
            entity.HasKey(e => new { e.ImageId, e.TagId }).HasName("PK__StudentI__A341389615E7831A");

            entity.HasOne(d => d.Image).WithMany(p => p.StudentImageTags)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentIm__Image__71D1E811");

            entity.HasOne(d => d.Tag).WithMany(p => p.StudentImageTags)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentIm__TagId__72C60C4A");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tags__657CF9AC60DCCD55");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Teachers__EDF259647034B870");

            entity.Property(e => e.TeacherId).ValueGeneratedNever();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.TeacherNavigation).WithOne(p => p.Teacher)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teachers__Teache__4D94879B");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CBCA6A21D");

            entity.HasIndex(e => e.Email, "UX_Users_Email_NotNull")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LanguagePref).HasDefaultValue("vi");
            entity.Property(e => e.RoleCode).IsFixedLength();

            entity.HasOne(d => d.School).WithMany(p => p.Users).HasConstraintName("FK_Users_School");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.InverseUpdatedByNavigation).HasConstraintName("FK_Users_UpdBy");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
