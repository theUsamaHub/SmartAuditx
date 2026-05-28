using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAuditX.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDbContextForComandUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Companies_CompanyId1",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CompanyId1",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyId1",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "OnboardingStatus",
                table: "Companies",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "CompanyInfoSaved",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CountryCode",
                table: "Companies",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_OnboardingStatus",
                table: "Companies",
                column: "OnboardingStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_CountryCode",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_OnboardingStatus",
                table: "Companies");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId1",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OnboardingStatus",
                table: "Companies",
                type: "nvarchar(30)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "CompanyInfoSaved");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId1",
                table: "Users",
                column: "CompanyId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Companies_CompanyId1",
                table: "Users",
                column: "CompanyId1",
                principalTable: "Companies",
                principalColumn: "CompanyId");
        }
    }
}
