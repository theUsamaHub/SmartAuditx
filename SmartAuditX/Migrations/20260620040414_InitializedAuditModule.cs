using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAuditX.Migrations
{
    /// <inheritdoc />
    public partial class InitializedAuditModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditTemplates",
                columns: table => new
                {
                    AuditTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsScoringEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTemplates", x => x.AuditTemplateId);
                    table.ForeignKey(
                        name: "FK_AuditTemplates_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Audits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditTemplateId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ScheduledStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ScheduledEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ActualStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ActualEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FinalScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Audits_AuditTemplates_AuditTemplateId",
                        column: x => x.AuditTemplateId,
                        principalTable: "AuditTemplates",
                        principalColumn: "AuditTemplateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Audits_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Audits_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditTemplateItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditTemplateId = table.Column<int>(type: "int", nullable: false),
                    SectionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Weightage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditTemplateItems_AuditTemplates_AuditTemplateId",
                        column: x => x.AuditTemplateId,
                        principalTable: "AuditTemplates",
                        principalColumn: "AuditTemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditTemplateSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditTemplateId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTemplateSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditTemplateSections_AuditTemplates_AuditTemplateId",
                        column: x => x.AuditTemplateId,
                        principalTable: "AuditTemplates",
                        principalColumn: "AuditTemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    AuditorId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditAssignments_Audits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "Audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditAssignments_Users_AuditorId",
                        column: x => x.AuditorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    AuditTemplateItemId = table.Column<int>(type: "int", nullable: false),
                    AuditorId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPassed = table.Column<bool>(type: "bit", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditResponses_AuditTemplateItems_AuditTemplateItemId",
                        column: x => x.AuditTemplateItemId,
                        principalTable: "AuditTemplateItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditResponses_Audits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "Audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditResponses_Users_AuditorId",
                        column: x => x.AuditorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditTemplateFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditTemplateSectionId = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Weightage = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTemplateFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditTemplateFields_AuditTemplateSections_AuditTemplateSectionId",
                        column: x => x.AuditTemplateSectionId,
                        principalTable: "AuditTemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditEvidence",
                columns: table => new
                {
                    AuditResponseId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvidence", x => x.AuditResponseId);
                    table.ForeignKey(
                        name: "FK_AuditEvidence_AuditResponses_AuditResponseId",
                        column: x => x.AuditResponseId,
                        principalTable: "AuditResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditTemplateFieldOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditTemplateFieldId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTemplateFieldOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditTemplateFieldOptions_AuditTemplateFields_AuditTemplateFieldId",
                        column: x => x.AuditTemplateFieldId,
                        principalTable: "AuditTemplateFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditAssignments_AuditId",
                table: "AuditAssignments",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditAssignments_AuditorId",
                table: "AuditAssignments",
                column: "AuditorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditResponses_AuditId",
                table: "AuditResponses",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditResponses_AuditorId",
                table: "AuditResponses",
                column: "AuditorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditResponses_AuditTemplateItemId",
                table: "AuditResponses",
                column: "AuditTemplateItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_AuditTemplateId",
                table: "Audits",
                column: "AuditTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_BranchId",
                table: "Audits",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_CompanyId",
                table: "Audits",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTemplateFieldOptions_AuditTemplateFieldId",
                table: "AuditTemplateFieldOptions",
                column: "AuditTemplateFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTemplateFields_AuditTemplateSectionId",
                table: "AuditTemplateFields",
                column: "AuditTemplateSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTemplateItems_AuditTemplateId",
                table: "AuditTemplateItems",
                column: "AuditTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTemplates_CompanyId",
                table: "AuditTemplates",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTemplateSections_AuditTemplateId",
                table: "AuditTemplateSections",
                column: "AuditTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditAssignments");

            migrationBuilder.DropTable(
                name: "AuditEvidence");

            migrationBuilder.DropTable(
                name: "AuditTemplateFieldOptions");

            migrationBuilder.DropTable(
                name: "AuditResponses");

            migrationBuilder.DropTable(
                name: "AuditTemplateFields");

            migrationBuilder.DropTable(
                name: "AuditTemplateItems");

            migrationBuilder.DropTable(
                name: "Audits");

            migrationBuilder.DropTable(
                name: "AuditTemplateSections");

            migrationBuilder.DropTable(
                name: "AuditTemplates");
        }
    }
}
