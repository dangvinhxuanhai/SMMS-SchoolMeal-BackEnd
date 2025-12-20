using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "school");

            migrationBuilder.EnsureSchema(
                name: "nutrition");

            migrationBuilder.EnsureSchema(
                name: "logs");

            migrationBuilder.EnsureSchema(
                name: "foodmenu");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.EnsureSchema(
                name: "fridge");

            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.EnsureSchema(
                name: "rag");

            migrationBuilder.EnsureSchema(
                name: "purchasing");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "logs",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActionDesc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RecordId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    AttributeNamne = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AuditLog__5E5486484B0B98E3", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "ExternalProviders",
                schema: "auth",
                columns: table => new
                {
                    ProviderId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KeyId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__External__B54C687DB3EC3D2C", x => x.ProviderId);
                });

            migrationBuilder.CreateTable(
                name: "FoodInFridgeIngredient",
                columns: table => new
                {
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    SampleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodInFridgeIngredient", x => new { x.IngredientId, x.SampleId });
                });

            migrationBuilder.CreateTable(
                name: "LoginAttempts",
                schema: "auth",
                columns: table => new
                {
                    AttemptId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AttemptAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Succeeded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LoginAtt__891A68E6084CCE3A", x => x.AttemptId);
                });

            migrationBuilder.CreateTable(
                name: "MenuRecommendSessions",
                schema: "rag",
                columns: table => new
                {
                    SessionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    RequestJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CandidateCount = table.Column<int>(type: "int", nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MenuReco__C9F49290D616D83C", x => x.SessionId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "auth",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoleDesc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__8AFACE1A019B869A", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                schema: "school",
                columns: table => new
                {
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    SchoolName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Hotline = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SchoolAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SettlementAccountNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SettlementBankCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SettlementAccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NeedRebuildAiIndex = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Schools__3DA4675B9933A434", x => x.SchoolId);
                });

            migrationBuilder.CreateTable(
                name: "StagingStudents",
                schema: "school",
                columns: table => new
                {
                    StageId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: true),
                    ClassName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ParentEmail1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ParentRelation1 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ParentEmail2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ParentRelation2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ImportBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RowErrors = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StagingS__03EB7AD8FF589359", x => x.StageId);
                });

            migrationBuilder.CreateTable(
                name: "AcademicYears",
                schema: "school",
                columns: table => new
                {
                    YearId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YearName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BoardingStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BoardingEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Academic__C33A18CDAE55CF4A", x => x.YearId);
                    table.ForeignKey(
                        name: "FK_AcademicYears_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                });

            migrationBuilder.CreateTable(
                name: "SchoolPaymentSettings",
                schema: "billing",
                columns: table => new
                {
                    SettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromMonth = table.Column<byte>(type: "tinyint", nullable: false),
                    ToMonth = table.Column<byte>(type: "tinyint", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MealPricePerDay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SchoolPa__54372B1D80E71C57", x => x.SettingId);
                    table.ForeignKey(
                        name: "FK_SPS_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmergencyPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LanguagePref = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "vi"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdentityNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    LockoutEndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Gender = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CC4CD5D84C8B", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                    table.ForeignKey(
                        name: "FK_Users_UpdBy",
                        column: x => x.UpdatedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__Users__RoleId__4AB81AF0",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "Roles",
                        principalColumn: "RoleId");
                });

            migrationBuilder.CreateTable(
                name: "Allergens",
                schema: "nutrition",
                columns: table => new
                {
                    AllergenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllergenName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AllergenMatter = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    AllergenInfo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Allergen__158B939FC3B0D611", x => x.AllergenId);
                    table.ForeignKey(
                        name: "FK_Allergens_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Allergens_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                });

            migrationBuilder.CreateTable(
                name: "FoodItems",
                schema: "nutrition",
                columns: table => new
                {
                    FoodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FoodName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FoodType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FoodDesc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsMainDish = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FoodItem__856DB3EB611ACDEE", x => x.FoodId);
                    table.ForeignKey(
                        name: "FK_FoodItems_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_FoodItems_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                schema: "nutrition",
                columns: table => new
                {
                    IngredientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IngredientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IngredientType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EnergyKcal = table.Column<decimal>(type: "decimal(7,2)", nullable: true),
                    ProteinG = table.Column<decimal>(type: "decimal(7,2)", nullable: true),
                    FatG = table.Column<decimal>(type: "decimal(7,2)", nullable: true),
                    CarbG = table.Column<decimal>(type: "decimal(7,2)", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ingredie__BEAEB25AF14E132E", x => x.IngredientId);
                    table.ForeignKey(
                        name: "FK_Ingredients_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Ingredients_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                schema: "foodmenu",
                columns: table => new
                {
                    MenuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    WeekNo = table.Column<short>(type: "smallint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    ConfirmedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AskToDelete = table.Column<bool>(type: "bit", nullable: false),
                    YearId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Menus__C99ED230D6EF5C89", x => x.MenuId);
                    table.ForeignKey(
                        name: "FK_Menus_Confirm",
                        column: x => x.ConfirmedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Menus_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                    table.ForeignKey(
                        name: "FK_Menus_Year",
                        column: x => x.YearId,
                        principalSchema: "school",
                        principalTable: "AcademicYears",
                        principalColumn: "YearId");
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "billing",
                columns: table => new
                {
                    NotificationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SendType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScheduleCron = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__notifica__20CF2E12D160E539", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK__notificat__Sende__00DF2177",
                        column: x => x.SenderId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "auth",
                columns: table => new
                {
                    RefreshTokenId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    CreatedByIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ReplacedById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RefreshT__F5845E39CBA3E9EC", x => x.RefreshTokenId);
                    table.ForeignKey(
                        name: "FK__RefreshTo__Repla__5070F446",
                        column: x => x.ReplacedById,
                        principalSchema: "auth",
                        principalTable: "RefreshTokens",
                        principalColumn: "RefreshTokenId");
                    table.ForeignKey(
                        name: "FK__RefreshTo__UserI__4F7CD00D",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ScheduleMeal",
                schema: "foodmenu",
                columns: table => new
                {
                    ScheduleMealId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    WeekEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    WeekNo = table.Column<short>(type: "smallint", nullable: false),
                    YearNo = table.Column<short>(type: "smallint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Schedule__7F2713EEE217869A", x => x.ScheduleMealId);
                    table.ForeignKey(
                        name: "FK_ScheduleMeal_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                    table.ForeignKey(
                        name: "FK_ScheduleMeal_User",
                        column: x => x.CreatedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SchoolPaymentGateways",
                schema: "billing",
                columns: table => new
                {
                    GatewayId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TheProvider = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChecksumKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReturnUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CancelUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SchoolPa__66BCD8A094CD1017", x => x.GatewayId);
                    table.ForeignKey(
                        name: "FK_SPG_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_SPG_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                    table.ForeignKey(
                        name: "FK_SPG_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SchoolRevenues",
                schema: "billing",
                columns: table => new
                {
                    SchoolRevenueId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RevenueDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],getdate()))"),
                    RevenueAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContractCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContractFileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContractNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SchoolRe__19A8292473B2508D", x => x.SchoolRevenueId);
                    table.ForeignKey(
                        name: "FK_SchoolRevenues_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_SchoolRevenues_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                    table.ForeignKey(
                        name: "FK_SchoolRevenues_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                schema: "school",
                columns: table => new
                {
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Gender = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelationName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Students__32C52B9905670856", x => x.StudentId);
                    table.ForeignKey(
                        name: "FK_Students_Parent",
                        column: x => x.ParentId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__Students__School__76969D2E",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                schema: "school",
                columns: table => new
                {
                    TagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tags__657CF9AC5250A64D", x => x.TagId);
                    table.ForeignKey(
                        name: "FK_Tags_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Tags_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                schema: "school",
                columns: table => new
                {
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HiredDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Teachers__EDF25964BF9BB93F", x => x.TeacherId);
                    table.ForeignKey(
                        name: "FK__Teachers__Teache__6754599E",
                        column: x => x.TeacherId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserExternalLogins",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<short>(type: "smallint", nullable: false),
                    ProviderSub = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserExte__923A62779BE7041C", x => new { x.UserId, x.ProviderId, x.ProviderSub });
                    table.ForeignKey(
                        name: "FK__UserExter__Provi__5AEE82B9",
                        column: x => x.ProviderId,
                        principalSchema: "auth",
                        principalTable: "ExternalProviders",
                        principalColumn: "ProviderId");
                    table.ForeignKey(
                        name: "FK__UserExter__UserI__59FA5E80",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "MenuRecommendResults",
                schema: "rag",
                columns: table => new
                {
                    SessionId = table.Column<long>(type: "bigint", nullable: false),
                    FoodId = table.Column<int>(type: "int", nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    RankShown = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    IsChosen = table.Column<bool>(type: "bit", nullable: false),
                    ChosenAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MenuReco__FCCDB5A8468F9A80", x => new { x.SessionId, x.FoodId, x.IsMain });
                    table.ForeignKey(
                        name: "FK__MenuRecom__FoodI__308E3499",
                        column: x => x.FoodId,
                        principalSchema: "nutrition",
                        principalTable: "FoodItems",
                        principalColumn: "FoodId");
                    table.ForeignKey(
                        name: "FK__MenuRecom__Sessi__2F9A1060",
                        column: x => x.SessionId,
                        principalSchema: "rag",
                        principalTable: "MenuRecommendSessions",
                        principalColumn: "SessionId");
                });

            migrationBuilder.CreateTable(
                name: "AllergeticIngredients",
                schema: "nutrition",
                columns: table => new
                {
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    AllergenId = table.Column<int>(type: "int", nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SeverityLevel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReactionNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HandlingNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DiagnosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Allerget__5FF60B63B04CFBA0", x => new { x.IngredientId, x.AllergenId });
                    table.ForeignKey(
                        name: "FK__Allergeti__Aller__245D67DE",
                        column: x => x.AllergenId,
                        principalSchema: "nutrition",
                        principalTable: "Allergens",
                        principalColumn: "AllergenId");
                    table.ForeignKey(
                        name: "FK__Allergeti__Ingre__236943A5",
                        column: x => x.IngredientId,
                        principalSchema: "nutrition",
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId");
                });

            migrationBuilder.CreateTable(
                name: "FoodItemIngredients",
                schema: "nutrition",
                columns: table => new
                {
                    FoodId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    QuantityGram = table.Column<decimal>(type: "decimal(9,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FoodItem__3E8758CE9383034F", x => new { x.FoodId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK__FoodItemI__FoodI__2DE6D218",
                        column: x => x.FoodId,
                        principalSchema: "nutrition",
                        principalTable: "FoodItems",
                        principalColumn: "FoodId");
                    table.ForeignKey(
                        name: "FK__FoodItemI__Ingre__2EDAF651",
                        column: x => x.IngredientId,
                        principalSchema: "nutrition",
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId");
                });

            migrationBuilder.CreateTable(
                name: "IngredientAlternatives",
                schema: "nutrition",
                columns: table => new
                {
                    AltIngredientId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(4,3)", nullable: true),
                    SourceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ingredie__790D4520C037718E", x => new { x.IngredientId, x.AltIngredientId });
                    table.ForeignKey(
                        name: "FK__Ingredien__AltIn__1A9EF37A",
                        column: x => x.AltIngredientId,
                        principalSchema: "nutrition",
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId");
                    table.ForeignKey(
                        name: "FK__Ingredien__Creat__1B9317B3",
                        column: x => x.CreatedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__Ingredien__Ingre__19AACF41",
                        column: x => x.IngredientId,
                        principalSchema: "nutrition",
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId");
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                schema: "inventory",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    QuantityGram = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Inventor__727E838B486011F9", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_InventoryItems_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_InventoryItems_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                    table.ForeignKey(
                        name: "FK__Inventory__Ingre__5E8A0973",
                        column: x => x.IngredientId,
                        principalSchema: "nutrition",
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId");
                });

            migrationBuilder.CreateTable(
                name: "FoodInFridge",
                schema: "fridge",
                columns: table => new
                {
                    SampleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YearId = table.Column<int>(type: "int", nullable: true),
                    FoodId = table.Column<int>(type: "int", nullable: true),
                    MenuId = table.Column<int>(type: "int", nullable: true),
                    StoredBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    TemperatureC = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FoodInFr__8B99EC6A88A99F3A", x => x.SampleId);
                    table.ForeignKey(
                        name: "FK_FIF_DeletedBy",
                        column: x => x.DeletedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_FIF_Food",
                        column: x => x.FoodId,
                        principalSchema: "nutrition",
                        principalTable: "FoodItems",
                        principalColumn: "FoodId");
                    table.ForeignKey(
                        name: "FK_FIF_Menu",
                        column: x => x.MenuId,
                        principalSchema: "foodmenu",
                        principalTable: "Menus",
                        principalColumn: "MenuId");
                    table.ForeignKey(
                        name: "FK_FIF_School",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                    table.ForeignKey(
                        name: "FK_FIF_StoredBy",
                        column: x => x.StoredBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_FIF_Year",
                        column: x => x.YearId,
                        principalSchema: "school",
                        principalTable: "AcademicYears",
                        principalColumn: "YearId");
                });

            migrationBuilder.CreateTable(
                name: "MenuDays",
                schema: "foodmenu",
                columns: table => new
                {
                    MenuDayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<byte>(type: "tinyint", nullable: false),
                    MealType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MenuDays__48283E5406B2E20E", x => x.MenuDayId);
                    table.ForeignKey(
                        name: "FK_MenuDays_Menus",
                        column: x => x.MenuId,
                        principalSchema: "foodmenu",
                        principalTable: "Menus",
                        principalColumn: "MenuId");
                });

            migrationBuilder.CreateTable(
                name: "NotificationRecipients",
                schema: "billing",
                columns: table => new
                {
                    NotificationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__F1B7A2D6B3FF2628", x => new { x.NotificationId, x.UserId });
                    table.ForeignKey(
                        name: "FK__Notificat__Notif__04AFB25B",
                        column: x => x.NotificationId,
                        principalSchema: "billing",
                        principalTable: "notifications",
                        principalColumn: "NotificationId");
                    table.ForeignKey(
                        name: "FK__Notificat__UserI__05A3D694",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "DailyMeals",
                schema: "foodmenu",
                columns: table => new
                {
                    DailyMealId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleMealId = table.Column<long>(type: "bigint", nullable: false),
                    MealDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MealType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DailyMea__4325CAFBE92EE8C8", x => x.DailyMealId);
                    table.ForeignKey(
                        name: "FK_DailyMeals_Schedule",
                        column: x => x.ScheduleMealId,
                        principalSchema: "foodmenu",
                        principalTable: "ScheduleMeal",
                        principalColumn: "ScheduleMealId");
                });

            migrationBuilder.CreateTable(
                name: "PurchasePlans",
                schema: "purchasing",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    PlanStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfirmedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AskToDelete = table.Column<bool>(type: "bit", nullable: false),
                    ScheduleMealId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Purchase__755C22B708449412", x => x.PlanId);
                    table.ForeignKey(
                        name: "FK_PurchasePlans_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_PurchasePlans_ScheduleMeal",
                        column: x => x.ScheduleMealId,
                        principalSchema: "foodmenu",
                        principalTable: "ScheduleMeal",
                        principalColumn: "ScheduleMealId");
                    table.ForeignKey(
                        name: "FK_PurchasePlans_Staff",
                        column: x => x.StaffId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Attendance",
                schema: "school",
                columns: table => new
                {
                    AttendanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbsentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NotifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Attendan__8B69261C251FB6CD", x => x.AttendanceId);
                    table.ForeignKey(
                        name: "FK__Attendanc__Notif__0A688BB1",
                        column: x => x.NotifiedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__Attendanc__Stude__09746778",
                        column: x => x.StudentId,
                        principalSchema: "school",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                schema: "billing",
                columns: table => new
                {
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonthNo = table.Column<short>(type: "smallint", nullable: false),
                    DateFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    DateTo = table.Column<DateOnly>(type: "date", nullable: false),
                    AbsentDay = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceCode = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK__Invoices__Studen__74794A92",
                        column: x => x.StudentId,
                        principalSchema: "school",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "StudentAllergens",
                schema: "nutrition",
                columns: table => new
                {
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllergenId = table.Column<int>(type: "int", nullable: false),
                    DiagnosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReactionNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HandlingNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StudentA__D39D92A0C877B8A0", x => new { x.StudentId, x.AllergenId });
                    table.ForeignKey(
                        name: "FK__StudentAl__Aller__32AB8735",
                        column: x => x.AllergenId,
                        principalSchema: "nutrition",
                        principalTable: "Allergens",
                        principalColumn: "AllergenId");
                    table.ForeignKey(
                        name: "FK__StudentAl__Stude__31B762FC",
                        column: x => x.StudentId,
                        principalSchema: "school",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "StudentHealthRecords",
                schema: "school",
                columns: table => new
                {
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordAt = table.Column<DateOnly>(type: "date", nullable: false),
                    HeightCm = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    YearId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StudentH__FBDF78E96508BFEE", x => x.RecordId);
                    table.ForeignKey(
                        name: "FK_StudentHealthRecords_Year",
                        column: x => x.YearId,
                        principalSchema: "school",
                        principalTable: "AcademicYears",
                        principalColumn: "YearId");
                    table.ForeignKey(
                        name: "FK__StudentHe__Stude__02FC7413",
                        column: x => x.StudentId,
                        principalSchema: "school",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "StudentImages",
                schema: "school",
                columns: table => new
                {
                    ImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TakenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    YearId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StudentI__7516F70C6F0FC553", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_StudentImages_Year",
                        column: x => x.YearId,
                        principalSchema: "school",
                        principalTable: "AcademicYears",
                        principalColumn: "YearId");
                    table.ForeignKey(
                        name: "FK__StudentIm__Stude__08B54D69",
                        column: x => x.StudentId,
                        principalSchema: "school",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                    table.ForeignKey(
                        name: "FK__StudentIm__Uploa__09A971A2",
                        column: x => x.UploadedBy,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                schema: "school",
                columns: table => new
                {
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    ClassName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YearId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Classes__CB1927C03085562A", x => x.ClassId);
                    table.ForeignKey(
                        name: "FK__Classes__SchoolI__6E01572D",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                    table.ForeignKey(
                        name: "FK__Classes__Teacher__6FE99F9F",
                        column: x => x.TeacherId,
                        principalSchema: "school",
                        principalTable: "Teachers",
                        principalColumn: "TeacherId");
                    table.ForeignKey(
                        name: "FK__Classes__YearId__6EF57B66",
                        column: x => x.YearId,
                        principalSchema: "school",
                        principalTable: "AcademicYears",
                        principalColumn: "YearId");
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                schema: "inventory",
                columns: table => new
                {
                    TransId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    TransType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    QuantityGram = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Inventor__9E5DDB3CAD03F0B1", x => x.TransId);
                    table.ForeignKey(
                        name: "FK__Inventory__ItemI__6442E2C9",
                        column: x => x.ItemId,
                        principalSchema: "inventory",
                        principalTable: "InventoryItems",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "IngredientInFridge",
                schema: "fridge",
                columns: table => new
                {
                    SampleId = table.Column<long>(type: "bigint", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ingredie__3073074FAA273DE7", x => new { x.SampleId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_FIFI_Ingredient",
                        column: x => x.IngredientId,
                        principalSchema: "nutrition",
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId");
                    table.ForeignKey(
                        name: "FK_FIFI_Sample",
                        column: x => x.SampleId,
                        principalSchema: "fridge",
                        principalTable: "FoodInFridge",
                        principalColumn: "SampleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuDayFoodItems",
                schema: "foodmenu",
                columns: table => new
                {
                    MenuDayId = table.Column<int>(type: "int", nullable: false),
                    FoodId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuDayFoodItems", x => new { x.MenuDayId, x.FoodId });
                    table.ForeignKey(
                        name: "FK_MenuDayFoodItems_Food",
                        column: x => x.FoodId,
                        principalSchema: "nutrition",
                        principalTable: "FoodItems",
                        principalColumn: "FoodId");
                    table.ForeignKey(
                        name: "FK_MenuDayFoodItems_MenuDay",
                        column: x => x.MenuDayId,
                        principalSchema: "foodmenu",
                        principalTable: "MenuDays",
                        principalColumn: "MenuDayId");
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                schema: "foodmenu",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    DailyMealId = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Feedback__6A4BEDD65FA0D847", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_Feedbacks_DailyMeal",
                        column: x => x.DailyMealId,
                        principalSchema: "foodmenu",
                        principalTable: "DailyMeals",
                        principalColumn: "DailyMealId");
                    table.ForeignKey(
                        name: "FK__Feedbacks__Sende__57DD0BE4",
                        column: x => x.SenderId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "MenuFoodItems",
                schema: "foodmenu",
                columns: table => new
                {
                    DailyMealId = table.Column<int>(type: "int", nullable: false),
                    FoodId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuFoodItems", x => new { x.DailyMealId, x.FoodId });
                    table.ForeignKey(
                        name: "FK_MenuFoodItems_DailyMeals",
                        column: x => x.DailyMealId,
                        principalSchema: "foodmenu",
                        principalTable: "DailyMeals",
                        principalColumn: "DailyMealId");
                    table.ForeignKey(
                        name: "FK_MenuFoodItems_FoodItems",
                        column: x => x.FoodId,
                        principalSchema: "nutrition",
                        principalTable: "FoodItems",
                        principalColumn: "FoodId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                schema: "purchasing",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    PurchaseOrderStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SupplierName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanId = table.Column<int>(type: "int", nullable: true),
                    StaffInCharged = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Purchase__C3905BCFDBD9BAF1", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Plans",
                        column: x => x.PlanId,
                        principalSchema: "purchasing",
                        principalTable: "PurchasePlans",
                        principalColumn: "PlanId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Schools",
                        column: x => x.SchoolId,
                        principalSchema: "school",
                        principalTable: "Schools",
                        principalColumn: "SchoolId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Users",
                        column: x => x.StaffInCharged,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PurchasePlanLines",
                schema: "purchasing",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    RqQuanityGram = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Purchase__CEB6C9922FF780FB", x => new { x.PlanId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK__PurchaseP__Ingre__6FB49575",
                        column: x => x.IngredientId,
                        principalSchema: "nutrition",
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId");
                    table.ForeignKey(
                        name: "FK__PurchaseP__PlanI__6EC0713C",
                        column: x => x.PlanId,
                        principalSchema: "purchasing",
                        principalTable: "PurchasePlans",
                        principalColumn: "PlanId");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "billing",
                columns: table => new
                {
                    PaymentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    ExpectedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 600m),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "pending"),
                    PaymentContent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    Method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentCode = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices",
                        column: x => x.InvoiceId,
                        principalSchema: "billing",
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId");
                });

            migrationBuilder.CreateTable(
                name: "StudentImageTags",
                schema: "school",
                columns: table => new
                {
                    ImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false),
                    TagNotes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsFavourite = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StudentI__A3413896B077628A", x => new { x.ImageId, x.TagId });
                    table.ForeignKey(
                        name: "FK__StudentIm__Image__1332DBDC",
                        column: x => x.ImageId,
                        principalSchema: "school",
                        principalTable: "StudentImages",
                        principalColumn: "ImageId");
                    table.ForeignKey(
                        name: "FK__StudentIm__TagId__14270015",
                        column: x => x.TagId,
                        principalSchema: "school",
                        principalTable: "Tags",
                        principalColumn: "TagId");
                });

            migrationBuilder.CreateTable(
                name: "StudentClasses",
                schema: "school",
                columns: table => new
                {
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoinedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LeftDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RegistStatus = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StudentC__2E74B9E56CCB357A", x => new { x.StudentId, x.ClassId });
                    table.ForeignKey(
                        name: "FK__StudentCl__Class__7F2BE32F",
                        column: x => x.ClassId,
                        principalSchema: "school",
                        principalTable: "Classes",
                        principalColumn: "ClassId");
                    table.ForeignKey(
                        name: "FK__StudentCl__Stude__7E37BEF6",
                        column: x => x.StudentId,
                        principalSchema: "school",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderLines",
                schema: "purchasing",
                columns: table => new
                {
                    LinesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    QuantityGram = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Purchase__728C596D29E416B9", x => x.LinesId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_Ingredients",
                        column: x => x.IngredientId,
                        principalSchema: "nutrition",
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_Orders",
                        column: x => x.OrderId,
                        principalSchema: "purchasing",
                        principalTable: "PurchaseOrders",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_Users",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_SchoolId",
                schema: "school",
                table: "AcademicYears",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Allergens_CreatedBy",
                schema: "nutrition",
                table: "Allergens",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Allergens_SchoolId",
                schema: "nutrition",
                table: "Allergens",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_AllergeticIngredients_AllergenId",
                schema: "nutrition",
                table: "AllergeticIngredients",
                column: "AllergenId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_NotifiedBy",
                schema: "school",
                table: "Attendance",
                column: "NotifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_StudentId",
                schema: "school",
                table: "Attendance",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_SchoolId",
                schema: "school",
                table: "Classes",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_YearId",
                schema: "school",
                table: "Classes",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "UQ__Classes__EDF259652891263B",
                schema: "school",
                table: "Classes",
                column: "TeacherId",
                unique: true,
                filter: "[TeacherId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_DailyMeals",
                schema: "foodmenu",
                table: "DailyMeals",
                columns: new[] { "ScheduleMealId", "MealDate", "MealType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__External__7D057CE505536EA9",
                schema: "auth",
                table: "ExternalProviders",
                column: "ProviderName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_DailyMealId",
                schema: "foodmenu",
                table: "Feedbacks",
                column: "DailyMealId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_SenderId",
                schema: "foodmenu",
                table: "Feedbacks",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodInFridge_DeletedBy",
                schema: "fridge",
                table: "FoodInFridge",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FoodInFridge_FoodId",
                schema: "fridge",
                table: "FoodInFridge",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodInFridge_MenuId",
                schema: "fridge",
                table: "FoodInFridge",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodInFridge_SchoolId",
                schema: "fridge",
                table: "FoodInFridge",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodInFridge_StoredBy",
                schema: "fridge",
                table: "FoodInFridge",
                column: "StoredBy");

            migrationBuilder.CreateIndex(
                name: "IX_FoodInFridge_YearId",
                schema: "fridge",
                table: "FoodInFridge",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItemIngredients_IngredientId",
                schema: "nutrition",
                table: "FoodItemIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_CreatedBy",
                schema: "nutrition",
                table: "FoodItems",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_SchoolId",
                schema: "nutrition",
                table: "FoodItems",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientAlternatives_AltIngredientId",
                schema: "nutrition",
                table: "IngredientAlternatives",
                column: "AltIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientAlternatives_CreatedBy",
                schema: "nutrition",
                table: "IngredientAlternatives",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientInFridge_IngredientId",
                schema: "fridge",
                table: "IngredientInFridge",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_CreatedBy",
                schema: "nutrition",
                table: "Ingredients",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_SchoolId",
                schema: "nutrition",
                table: "Ingredients",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_CreatedBy",
                schema: "inventory",
                table: "InventoryItems",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_IngredientId",
                schema: "inventory",
                table: "InventoryItems",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_SchoolId",
                schema: "inventory",
                table: "InventoryItems",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ItemId",
                schema: "inventory",
                table: "InventoryTransactions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceCode",
                schema: "billing",
                table: "Invoices",
                column: "InvoiceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_StudentId",
                schema: "billing",
                table: "Invoices",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuDayFoodItems_FoodId",
                schema: "foodmenu",
                table: "MenuDayFoodItems",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuDays_Menu",
                schema: "foodmenu",
                table: "MenuDays",
                columns: new[] { "MenuId", "DayOfWeek", "MealType" });

            migrationBuilder.CreateIndex(
                name: "UQ_MenuDays",
                schema: "foodmenu",
                table: "MenuDays",
                columns: new[] { "MenuId", "DayOfWeek", "MealType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuFoodItems_FoodId",
                schema: "foodmenu",
                table: "MenuFoodItems",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRecommendResults_FoodId",
                schema: "rag",
                table: "MenuRecommendResults",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_ConfirmedBy",
                schema: "foodmenu",
                table: "Menus",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_SchoolId",
                schema: "foodmenu",
                table: "Menus",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_YearId",
                schema: "foodmenu",
                table: "Menus",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_UserId",
                schema: "billing",
                table: "NotificationRecipients",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_SenderId",
                schema: "billing",
                table: "notifications",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                schema: "billing",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentCode",
                schema: "billing",
                table: "Payments",
                column: "PaymentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_IngredientId",
                schema: "purchasing",
                table: "PurchaseOrderLines",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_OrderId",
                schema: "purchasing",
                table: "PurchaseOrderLines",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_UserId",
                schema: "purchasing",
                table: "PurchaseOrderLines",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PlanId",
                schema: "purchasing",
                table: "PurchaseOrders",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SchoolId",
                schema: "purchasing",
                table: "PurchaseOrders",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_StaffInCharged",
                schema: "purchasing",
                table: "PurchaseOrders",
                column: "StaffInCharged");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePlanLines_IngredientId",
                schema: "purchasing",
                table: "PurchasePlanLines",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePlans_ConfirmedBy",
                schema: "purchasing",
                table: "PurchasePlans",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePlans_ScheduleMealId",
                schema: "purchasing",
                table: "PurchasePlans",
                column: "ScheduleMealId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePlans_StaffId",
                schema: "purchasing",
                table: "PurchasePlans",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ReplacedById",
                schema: "auth",
                table: "RefreshTokens",
                column: "ReplacedById");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "auth",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__RefreshT__1EB4F817E3CAB654",
                schema: "auth",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Roles__8A2B616026863920",
                schema: "auth",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMeal_CreatedBy",
                schema: "foodmenu",
                table: "ScheduleMeal",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMeal_SchoolWeek",
                schema: "foodmenu",
                table: "ScheduleMeal",
                columns: new[] { "SchoolId", "WeekStart", "WeekEnd" });

            migrationBuilder.CreateIndex(
                name: "UQ_ScheduleMeal_School_WeekNoYear",
                schema: "foodmenu",
                table: "ScheduleMeal",
                columns: new[] { "SchoolId", "WeekNo", "YearNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ScheduleMeal_School_WeekStart",
                schema: "foodmenu",
                table: "ScheduleMeal",
                columns: new[] { "SchoolId", "WeekStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPaymentGateways_CreatedBy",
                schema: "billing",
                table: "SchoolPaymentGateways",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPaymentGateways_SchoolId",
                schema: "billing",
                table: "SchoolPaymentGateways",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPaymentGateways_UpdatedBy",
                schema: "billing",
                table: "SchoolPaymentGateways",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPaymentSettings_SchoolId",
                schema: "billing",
                table: "SchoolPaymentSettings",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRevenues_CreatedBy",
                schema: "billing",
                table: "SchoolRevenues",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRevenues_SchoolDate",
                schema: "billing",
                table: "SchoolRevenues",
                columns: new[] { "SchoolId", "RevenueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRevenues_UpdatedBy",
                schema: "billing",
                table: "SchoolRevenues",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAllergens_AllergenId",
                schema: "nutrition",
                table: "StudentAllergens",
                column: "AllergenId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_ClassId",
                schema: "school",
                table: "StudentClasses",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentHealthRecords_StudentId",
                schema: "school",
                table: "StudentHealthRecords",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentHealthRecords_YearId",
                schema: "school",
                table: "StudentHealthRecords",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentImages_StudentId",
                schema: "school",
                table: "StudentImages",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentImages_UploadedBy",
                schema: "school",
                table: "StudentImages",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StudentImages_YearId",
                schema: "school",
                table: "StudentImages",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentImageTags_TagId",
                schema: "school",
                table: "StudentImageTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_ParentId",
                schema: "school",
                table: "Students",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolId",
                schema: "school",
                table: "Students",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CreatedBy",
                schema: "school",
                table: "Tags",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_SchoolId",
                schema: "school",
                table: "Tags",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "UQ__Tags__BDE0FD1D1C99F83F",
                schema: "school",
                table: "Tags",
                column: "TagName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Teachers__1F6425484B87CBAA",
                schema: "school",
                table: "Teachers",
                column: "EmployeeCode",
                unique: true,
                filter: "[EmployeeCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalLogins_ProviderId",
                schema: "auth",
                table: "UserExternalLogins",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                schema: "auth",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SchoolId",
                schema: "auth",
                table: "Users",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UpdatedBy",
                schema: "auth",
                table: "Users",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ_Users_Phone",
                schema: "auth",
                table: "Users",
                column: "Phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllergeticIngredients",
                schema: "nutrition");

            migrationBuilder.DropTable(
                name: "Attendance",
                schema: "school");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "logs");

            migrationBuilder.DropTable(
                name: "Feedbacks",
                schema: "foodmenu");

            migrationBuilder.DropTable(
                name: "FoodInFridgeIngredient");

            migrationBuilder.DropTable(
                name: "FoodItemIngredients",
                schema: "nutrition");

            migrationBuilder.DropTable(
                name: "IngredientAlternatives",
                schema: "nutrition");

            migrationBuilder.DropTable(
                name: "IngredientInFridge",
                schema: "fridge");

            migrationBuilder.DropTable(
                name: "InventoryTransactions",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "LoginAttempts",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "MenuDayFoodItems",
                schema: "foodmenu");

            migrationBuilder.DropTable(
                name: "MenuFoodItems",
                schema: "foodmenu");

            migrationBuilder.DropTable(
                name: "MenuRecommendResults",
                schema: "rag");

            migrationBuilder.DropTable(
                name: "NotificationRecipients",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "Payments",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "PurchaseOrderLines",
                schema: "purchasing");

            migrationBuilder.DropTable(
                name: "PurchasePlanLines",
                schema: "purchasing");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "SchoolPaymentGateways",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "SchoolPaymentSettings",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "SchoolRevenues",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "StagingStudents",
                schema: "school");

            migrationBuilder.DropTable(
                name: "StudentAllergens",
                schema: "nutrition");

            migrationBuilder.DropTable(
                name: "StudentClasses",
                schema: "school");

            migrationBuilder.DropTable(
                name: "StudentHealthRecords",
                schema: "school");

            migrationBuilder.DropTable(
                name: "StudentImageTags",
                schema: "school");

            migrationBuilder.DropTable(
                name: "UserExternalLogins",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "FoodInFridge",
                schema: "fridge");

            migrationBuilder.DropTable(
                name: "InventoryItems",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "MenuDays",
                schema: "foodmenu");

            migrationBuilder.DropTable(
                name: "DailyMeals",
                schema: "foodmenu");

            migrationBuilder.DropTable(
                name: "MenuRecommendSessions",
                schema: "rag");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "Invoices",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "PurchaseOrders",
                schema: "purchasing");

            migrationBuilder.DropTable(
                name: "Allergens",
                schema: "nutrition");

            migrationBuilder.DropTable(
                name: "Classes",
                schema: "school");

            migrationBuilder.DropTable(
                name: "StudentImages",
                schema: "school");

            migrationBuilder.DropTable(
                name: "Tags",
                schema: "school");

            migrationBuilder.DropTable(
                name: "ExternalProviders",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "FoodItems",
                schema: "nutrition");

            migrationBuilder.DropTable(
                name: "Ingredients",
                schema: "nutrition");

            migrationBuilder.DropTable(
                name: "Menus",
                schema: "foodmenu");

            migrationBuilder.DropTable(
                name: "PurchasePlans",
                schema: "purchasing");

            migrationBuilder.DropTable(
                name: "Teachers",
                schema: "school");

            migrationBuilder.DropTable(
                name: "Students",
                schema: "school");

            migrationBuilder.DropTable(
                name: "AcademicYears",
                schema: "school");

            migrationBuilder.DropTable(
                name: "ScheduleMeal",
                schema: "foodmenu");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Schools",
                schema: "school");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "auth");
        }
    }
}
