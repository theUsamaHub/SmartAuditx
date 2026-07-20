using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAuditX.Migrations
{
    /// <inheritdoc />
    public partial class AuditModuleEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditResponses_AuditTemplateItems_AuditTemplateItemId",
                table: "AuditResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditResponses_Users_AuditorId",
                table: "AuditResponses");

            migrationBuilder.DropIndex(
                name: "IX_AuditResponses_AuditorId",
                table: "AuditResponses");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "AuditResponses");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "AuditResponses");

            migrationBuilder.RenameColumn(
                name: "IsPassed",
                table: "AuditResponses",
                newName: "ResponseBoolean");

            migrationBuilder.RenameColumn(
                name: "Comments",
                table: "AuditResponses",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "AuditorId",
                table: "AuditResponses",
                newName: "FieldTypeSnapshot");

            migrationBuilder.RenameColumn(
                name: "AuditTemplateItemId",
                table: "AuditResponses",
                newName: "AuditTemplateFieldId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditResponses_AuditTemplateItemId",
                table: "AuditResponses",
                newName: "IX_AuditResponses_AuditTemplateFieldId");

            migrationBuilder.AddColumn<bool>(
                name: "AllowNotes",
                table: "AuditTemplateFields",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AuditTemplateId",
                table: "AuditTemplateFields",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HelpText",
                table: "AuditTemplateFields",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxValue",
                table: "AuditTemplateFields",
                type: "decimal(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinPhotoCount",
                table: "AuditTemplateFields",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinValue",
                table: "AuditTemplateFields",
                type: "decimal(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedToUserId",
                table: "Audits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Audits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Audits",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "Audits",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                table: "Audits",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByUserId",
                table: "Audits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemplateVersionSnapshot",
                table: "Audits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FieldLabelSnapshot",
                table: "AuditResponses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSkipped",
                table: "AuditResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResponseDate",
                table: "AuditResponses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ResponseNumber",
                table: "AuditResponses",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseText",
                table: "AuditResponses",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Score",
                table: "AuditResponses",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SelectedOptionId",
                table: "AuditResponses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Audits_AssignedToUserId",
                table: "Audits",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_ReviewedByUserId",
                table: "Audits",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditResponses_SelectedOptionId",
                table: "AuditResponses",
                column: "SelectedOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditResponses_AuditTemplateFieldOptions_SelectedOptionId",
                table: "AuditResponses",
                column: "SelectedOptionId",
                principalTable: "AuditTemplateFieldOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditResponses_AuditTemplateFields_AuditTemplateFieldId",
                table: "AuditResponses",
                column: "AuditTemplateFieldId",
                principalTable: "AuditTemplateFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Audits_Users_AssignedToUserId",
                table: "Audits",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Audits_Users_ReviewedByUserId",
                table: "Audits",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditResponses_AuditTemplateFieldOptions_SelectedOptionId",
                table: "AuditResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditResponses_AuditTemplateFields_AuditTemplateFieldId",
                table: "AuditResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_Audits_Users_AssignedToUserId",
                table: "Audits");

            migrationBuilder.DropForeignKey(
                name: "FK_Audits_Users_ReviewedByUserId",
                table: "Audits");

            migrationBuilder.DropIndex(
                name: "IX_Audits_AssignedToUserId",
                table: "Audits");

            migrationBuilder.DropIndex(
                name: "IX_Audits_ReviewedByUserId",
                table: "Audits");

            migrationBuilder.DropIndex(
                name: "IX_AuditResponses_SelectedOptionId",
                table: "AuditResponses");

            migrationBuilder.DropColumn(
                name: "AllowNotes",
                table: "AuditTemplateFields");

            migrationBuilder.DropColumn(
                name: "AuditTemplateId",
                table: "AuditTemplateFields");

            migrationBuilder.DropColumn(
                name: "HelpText",
                table: "AuditTemplateFields");

            migrationBuilder.DropColumn(
                name: "MaxValue",
                table: "AuditTemplateFields");

            migrationBuilder.DropColumn(
                name: "MinPhotoCount",
                table: "AuditTemplateFields");

            migrationBuilder.DropColumn(
                name: "MinValue",
                table: "AuditTemplateFields");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "TemplateVersionSnapshot",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "FieldLabelSnapshot",
                table: "AuditResponses");

            migrationBuilder.DropColumn(
                name: "IsSkipped",
                table: "AuditResponses");

            migrationBuilder.DropColumn(
                name: "ResponseDate",
                table: "AuditResponses");

            migrationBuilder.DropColumn(
                name: "ResponseNumber",
                table: "AuditResponses");

            migrationBuilder.DropColumn(
                name: "ResponseText",
                table: "AuditResponses");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "AuditResponses");

            migrationBuilder.DropColumn(
                name: "SelectedOptionId",
                table: "AuditResponses");

            migrationBuilder.RenameColumn(
                name: "ResponseBoolean",
                table: "AuditResponses",
                newName: "IsPassed");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "AuditResponses",
                newName: "Comments");

            migrationBuilder.RenameColumn(
                name: "FieldTypeSnapshot",
                table: "AuditResponses",
                newName: "AuditorId");

            migrationBuilder.RenameColumn(
                name: "AuditTemplateFieldId",
                table: "AuditResponses",
                newName: "AuditTemplateItemId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditResponses_AuditTemplateFieldId",
                table: "AuditResponses",
                newName: "IX_AuditResponses_AuditTemplateItemId");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubmittedAt",
                table: "AuditResponses",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "AuditResponses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditResponses_AuditorId",
                table: "AuditResponses",
                column: "AuditorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditResponses_AuditTemplateItems_AuditTemplateItemId",
                table: "AuditResponses",
                column: "AuditTemplateItemId",
                principalTable: "AuditTemplateItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditResponses_Users_AuditorId",
                table: "AuditResponses",
                column: "AuditorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
