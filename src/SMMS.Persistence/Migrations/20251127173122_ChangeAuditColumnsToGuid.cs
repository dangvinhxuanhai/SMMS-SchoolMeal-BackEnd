using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAuditColumnsToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Xóa các ràng buộc mặc định (Default Constraint) nếu có
            // Đoạn script SQL này giúp tìm và xóa constraint Default=0 của cột cũ để tránh lỗi khi Drop Column
            migrationBuilder.Sql(@"
        DECLARE @ConstraintName nvarchar(200)
        SELECT @ConstraintName = Name FROM SYS.DEFAULT_CONSTRAINTS
        WHERE PARENT_OBJECT_ID = OBJECT_ID(N'[school].[Schools]')
        AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns WHERE NAME = N'CreatedBy' AND object_id = OBJECT_ID(N'[school].[Schools]'))
        IF @ConstraintName IS NOT NULL
        EXEC('ALTER TABLE [school].[Schools] DROP CONSTRAINT ' + @ConstraintName)
    ");

            migrationBuilder.Sql(@"
        DECLARE @ConstraintName2 nvarchar(200)
        SELECT @ConstraintName2 = Name FROM SYS.DEFAULT_CONSTRAINTS
        WHERE PARENT_OBJECT_ID = OBJECT_ID(N'[school].[Schools]')
        AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns WHERE NAME = N'UpdatedBy' AND object_id = OBJECT_ID(N'[school].[Schools]'))
        IF @ConstraintName2 IS NOT NULL
        EXEC('ALTER TABLE [school].[Schools] DROP CONSTRAINT ' + @ConstraintName2)
    ");

            // 2. Xóa hẳn cột cũ (Kiểu int) đi
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "school",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "school",
                table: "Schools");

            // 3. Tạo lại cột mới (Kiểu Guid)
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "school",
                table: "Schools",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "school",
                table: "Schools",
                type: "uniqueidentifier",
                nullable: true);
        }        /// <inheritdoc />
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
                name: "ScheduleMeal",
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
                name: "Menus",
                schema: "foodmenu");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AcademicYears",
                schema: "school");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Schools",
                schema: "school");
        }
    }
}
