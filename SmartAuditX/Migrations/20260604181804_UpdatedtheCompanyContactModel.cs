using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAuditX.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedtheCompanyContactModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "CompanyContacts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "FaxNumber",
                table: "CompanyContacts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactType",
                table: "CompanyContacts",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "HeadOffice");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CompanyContacts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneDialCode",
                table: "CompanyContacts",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyContacts_ContactType",
                table: "CompanyContacts",
                column: "ContactType");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyContacts_IsDeleted",
                table: "CompanyContacts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyContacts_IsPrimary",
                table: "CompanyContacts",
                column: "IsPrimary");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyContacts_ContactType",
                table: "CompanyContacts");

            migrationBuilder.DropIndex(
                name: "IX_CompanyContacts_IsDeleted",
                table: "CompanyContacts");

            migrationBuilder.DropIndex(
                name: "IX_CompanyContacts_IsPrimary",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "ContactType",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "PhoneDialCode",
                table: "CompanyContacts");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "CompanyContacts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "FaxNumber",
                table: "CompanyContacts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
