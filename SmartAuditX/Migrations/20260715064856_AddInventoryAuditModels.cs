using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAuditX.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryAuditModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditBarcodeScans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    AuditResponseId = table.Column<int>(type: "int", nullable: true),
                    BarcodeValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemNameSnapshot = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LocationSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SKUSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpectedQuantity = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    ActualQuantity = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DiscrepancyQuantity = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ScanCount = table.Column<int>(type: "int", nullable: false),
                    FirstScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditBarcodeScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditBarcodeScans_AuditResponses_AuditResponseId",
                        column: x => x.AuditResponseId,
                        principalTable: "AuditResponses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditBarcodeScans_Audits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "Audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditTemplateInventoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditTemplateId = table.Column<int>(type: "int", nullable: false),
                    BarcodeValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SKU = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpectedQuantity = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTemplateInventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditTemplateInventoryItems_AuditTemplates_AuditTemplateId",
                        column: x => x.AuditTemplateId,
                        principalTable: "AuditTemplates",
                        principalColumn: "AuditTemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditBarcodeScans_AuditId",
                table: "AuditBarcodeScans",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditBarcodeScans_AuditId_BarcodeValue",
                table: "AuditBarcodeScans",
                columns: new[] { "AuditId", "BarcodeValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditBarcodeScans_AuditResponseId",
                table: "AuditBarcodeScans",
                column: "AuditResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTemplateInventoryItems_AuditTemplateId",
                table: "AuditTemplateInventoryItems",
                column: "AuditTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTemplateInventoryItems_BarcodeValue",
                table: "AuditTemplateInventoryItems",
                column: "BarcodeValue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditBarcodeScans");

            migrationBuilder.DropTable(
                name: "AuditTemplateInventoryItems");
        }
    }
}
