using SmartAuditX.Data;
using SmartAuditX.Models.AuditModule;
using SmartAuditX.Models.AuditModule.AuditEnums;
using SmartAuditX.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SmartAuditX.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateAuditReportAsync(int auditId, int companyId)
        {
            var audit = await _context.Audits
                .Include(a => a.AuditTemplate)
                    .ThenInclude(t => t.Sections)
                        .ThenInclude(s => s.Fields)
                            .ThenInclude(f => f.Options)
                .Include(a => a.Branch)
                .Include(a => a.AssignedToUser)
                .Include(a => a.ReviewedByUser)
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            if (audit == null)
                throw new ArgumentException("Audit not found.");

            var responses = await _context.AuditResponses
                .Where(r => r.AuditId == auditId)
                .ToListAsync();

            var scans = await _context.AuditBarcodeScans
                .Where(s => s.AuditId == auditId)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(40);
                    page.MarginVertical(30);

                    page.Header().Text("SmartAuditX - Audit Report").FontSize(20).Bold();

                    page.Content().PaddingTop(20).Column(column =>
                    {
                        // Audit Info
                        column.Item().Text($"Title: {audit.Title}").FontSize(14).Bold();
                        column.Item().Text($"Template: {audit.AuditTemplate?.Title ?? "N/A"}").FontSize(11);
                        column.Item().Text($"Branch: {audit.Branch?.BranchName ?? "All Branches"}").FontSize(11);
                        column.Item().Text($"Auditor: {audit.AssignedToUser?.UserName ?? "N/A"}").FontSize(11);
                        column.Item().Text($"Status: {audit.Status}").FontSize(11);
                        column.Item().Text($"Score: {audit.FinalScore?.ToString("F1") ?? "N/A"}%").FontSize(11).Bold();
                        column.Item().Text($"Date: {audit.CreatedAt:dd MMM yyyy}").FontSize(11);
                        column.Item().PaddingBottom(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                        // Sections & Responses
                        if (audit.AuditTemplate?.Sections != null)
                        {
                            foreach (var section in audit.AuditTemplate.Sections.OrderBy(s => s.SortOrder))
                            {
                                column.Item().PaddingBottom(5).PaddingTop(10).Text(section.Title).FontSize(14).Bold();

                                if (section.Fields != null)
                                {
                                    foreach (var field in section.Fields.OrderBy(f => f.SortOrder))
                                    {
                                        var response = responses.FirstOrDefault(r => r.AuditTemplateFieldId == field.Id);
                                        var fieldValue = GetFieldValue(response, field);

                                        column.Item().PaddingBottom(3).Row(row =>
                                        {
                                            row.ConstantItem(250).Text(field.QuestionText).FontSize(10);
                                            row.ConstantItem(200).Text(fieldValue).FontSize(10);
                                        });
                                    }
                                }

                                column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                            }
                        }

                        // Inventory Comparison
                        if (scans.Any())
                        {
                            column.Item().PaddingTop(15).Text("Inventory Comparison").FontSize(14).Bold();

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Item").FontSize(9).Bold();
                                    header.Cell().Text("Barcode").FontSize(9).Bold();
                                    header.Cell().Text("Expected").FontSize(9).Bold();
                                    header.Cell().Text("Actual").FontSize(9).Bold();
                                    header.Cell().Text("Status").FontSize(9).Bold();
                                });

                                foreach (var scan in scans)
                                {
                                    table.Cell().Text(scan.ItemNameSnapshot ?? "Unknown").FontSize(9);
                                    table.Cell().Text(scan.BarcodeValue).FontSize(9);
                                    table.Cell().Text(scan.ExpectedQuantity?.ToString() ?? "N/A").FontSize(9);
                                    table.Cell().Text(scan.ActualQuantity.ToString()).FontSize(9);
                                    table.Cell().Text(scan.Status.ToString()).FontSize(9);
                                }
                            });
                        }

                        // Review Notes
                        if (!string.IsNullOrEmpty(audit.ReviewNotes))
                        {
                            column.Item().PaddingTop(15).Text("Review Notes").FontSize(12).Bold();
                            column.Item().Text(audit.ReviewNotes).FontSize(10);
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("SmartAuditX Audit Report | Page ").FontSize(9);
                        text.CurrentPageNumber().FontSize(9);
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateInventoryReportAsync(int auditId, int companyId)
        {
            var audit = await _context.Audits
                .Include(a => a.AuditTemplate)
                .Include(a => a.Branch)
                .Include(a => a.AssignedToUser)
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            if (audit == null)
                throw new ArgumentException("Audit not found.");

            var scans = await _context.AuditBarcodeScans
                .Where(s => s.AuditId == auditId)
                .ToListAsync();

            var inventoryItems = await _context.AuditTemplateInventoryItems
                .Where(i => i.AuditTemplateId == audit.AuditTemplateId)
                .ToListAsync();

            var matched = scans.Count(s => s.Status == BarcodeScanStatus.Matched);
            var surplus = scans.Count(s => s.Status == BarcodeScanStatus.Surplus);
            var shortage = scans.Count(s => s.Status == BarcodeScanStatus.Shortage);
            var missing = inventoryItems.Count(i => !scans.Any(s => s.BarcodeValue == i.BarcodeValue));

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.MarginHorizontal(30);
                    page.MarginVertical(25);

                    page.Header().Text("SmartAuditX - Inventory Report").FontSize(20).Bold();

                    page.Content().PaddingTop(20).Column(column =>
                    {
                        // Audit Info
                        column.Item().Text($"Audit: {audit.Title}").FontSize(12).Bold();
                        column.Item().Text($"Auditor: {audit.AssignedToUser?.UserName ?? "N/A"}").FontSize(10);
                        column.Item().Text($"Date: {DateTime.Now:dd MMM yyyy}").FontSize(10);
                        column.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                        // Summary Stats
                        column.Item().PaddingBottom(10).Row(row =>
                        {
                            row.ConstantItem(120).Text($"Total: {inventoryItems.Count}").FontSize(10).Bold();
                            row.ConstantItem(120).Text($"Scanned: {scans.Count}").FontSize(10).Bold();
                            row.ConstantItem(100).Text($"Matched: {matched}").FontSize(10);
                            row.ConstantItem(100).Text($"Surplus: {surplus}").FontSize(10);
                            row.ConstantItem(100).Text($"Shortage: {shortage}").FontSize(10);
                            row.ConstantItem(100).Text($"Missing: {missing}").FontSize(10);
                        });

                        // Comparison Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Item").FontSize(8).Bold();
                                header.Cell().Text("Barcode").FontSize(8).Bold();
                                header.Cell().Text("Location").FontSize(8).Bold();
                                header.Cell().Text("Expected").FontSize(8).Bold();
                                header.Cell().Text("Actual").FontSize(8).Bold();
                                header.Cell().Text("Diff").FontSize(8).Bold();
                                header.Cell().Text("Status").FontSize(8).Bold();
                                header.Cell().Text("Unit").FontSize(8).Bold();
                            });

                            foreach (var scan in scans)
                            {
                                table.Cell().Text(scan.ItemNameSnapshot ?? "Unknown").FontSize(8);
                                table.Cell().Text(scan.BarcodeValue).FontSize(8);
                                table.Cell().Text(scan.LocationSnapshot ?? "-").FontSize(8);
                                table.Cell().Text(scan.ExpectedQuantity?.ToString() ?? "N/A").FontSize(8);
                                table.Cell().Text(scan.ActualQuantity.ToString()).FontSize(8);
                                table.Cell().Text((scan.DiscrepancyQuantity ?? 0).ToString()).FontSize(8);
                                table.Cell().Text(scan.Status.ToString()).FontSize(8);
                                table.Cell().Text(scan.Unit ?? "-").FontSize(8);
                            }

                            foreach (var item in inventoryItems.Where(i => !scans.Any(s => s.BarcodeValue == i.BarcodeValue)))
                            {
                                table.Cell().Text(item.ItemName).FontSize(8);
                                table.Cell().Text(item.BarcodeValue).FontSize(8);
                                table.Cell().Text(item.Location ?? "-").FontSize(8);
                                table.Cell().Text(item.ExpectedQuantity.ToString()).FontSize(8);
                                table.Cell().Text("0").FontSize(8);
                                table.Cell().Text($"-{item.ExpectedQuantity}").FontSize(8);
                                table.Cell().Text("Missing").FontSize(8);
                                table.Cell().Text(item.Unit ?? "-").FontSize(8);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("SmartAuditX Inventory Report | Page ").FontSize(9);
                        text.CurrentPageNumber().FontSize(9);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private string GetFieldValue(AuditResponse? response, AuditTemplateField field)
        {
            if (response == null) return "Not answered";

            return field.ItemType switch
            {
                TemplateItemType.Boolean => response.ResponseBoolean == true ? "Yes" : "No",
                TemplateItemType.Text => response.ResponseText ?? "Not answered",
                TemplateItemType.Number => response.ResponseNumber?.ToString() ?? "Not answered",
                TemplateItemType.Rating => response.ResponseNumber?.ToString() ?? "Not answered",
                TemplateItemType.Date => response.ResponseDate?.ToString("dd MMM yyyy") ?? "Not answered",
                TemplateItemType.Selection => response.SelectedOptionId?.ToString() ?? "Not answered",
                TemplateItemType.Photo => "Photo uploaded",
                TemplateItemType.Signature => "Signature captured",
                TemplateItemType.Barcode => response.ResponseText ?? "Not answered",
                _ => response.ResponseText ?? "Not answered"
            };
        }
    }
}
