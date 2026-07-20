using SmartAuditX.Data;
using SmartAuditX.Models.AuditModule;
using SmartAuditX.Models.AuditModule.AuditEnums;
using SmartAuditX.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace SmartAuditX.Services.Implementations
{
    public class AuditInventoryService : IAuditInventoryService
    {
        private readonly ApplicationDbContext _context;

        public AuditInventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryItemDto>> GetInventoryItemsAsync(int templateId, int companyId)
        {
            var template = await _context.AuditTemplates
                .FirstOrDefaultAsync(t => t.AuditTemplateId == templateId && t.CompanyId == companyId);

            if (template == null) return new List<InventoryItemDto>();

            return await _context.AuditTemplateInventoryItems
                .Where(i => i.AuditTemplateId == templateId)
                .OrderBy(i => i.BarcodeValue)
                .Select(i => new InventoryItemDto
                {
                    Id = i.Id,
                    BarcodeValue = i.BarcodeValue,
                    ItemName = i.ItemName,
                    Location = i.Location,
                    SKU = i.SKU,
                    Category = i.Category,
                    ExpectedQuantity = i.ExpectedQuantity,
                    Unit = i.Unit
                })
                .ToListAsync();
        }

        public async Task<int> UploadInventoryFromExcelAsync(int templateId, int companyId, Stream fileStream, string fileName)
        {
            var template = await _context.AuditTemplates
                .FirstOrDefaultAsync(t => t.AuditTemplateId == templateId && t.CompanyId == companyId);

            if (template == null) return 0;

            // Clear existing inventory for this template
            var existingItems = await _context.AuditTemplateInventoryItems
                .Where(i => i.AuditTemplateId == templateId)
                .ToListAsync();
            _context.AuditTemplateInventoryItems.RemoveRange(existingItems);

            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null) return 0;

            int rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            if (rowCount < 2) return 0; // No data rows

            int importedCount = 0;

            // Find column indices by header names
            int barcodeCol = -1, nameCol = -1, qtyCol = -1, locationCol = -1, skuCol = -1, categoryCol = -1, unitCol = -1;

            var headerRow = worksheet.Row(1);
            int lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
            for (int col = 1; col <= lastCol; col++)
            {
                var header = headerRow.Cell(col).GetString().Trim().ToLower();
                if (header == "barcode" || header == "barcodevalue") barcodeCol = col;
                else if (header == "itemname" || header == "name" || header == "product") nameCol = col;
                else if (header == "quantity" || header == "expectedquantity" || header == "qty") qtyCol = col;
                else if (header == "location" || header == "shelf") locationCol = col;
                else if (header == "sku") skuCol = col;
                else if (header == "category") categoryCol = col;
                else if (header == "unit") unitCol = col;
            }

            if (barcodeCol == -1 || nameCol == -1 || qtyCol == -1)
                throw new InvalidOperationException("Excel must have columns: Barcode, ItemName, Quantity");

            for (int row = 2; row <= rowCount; row++)
            {
                var barcode = worksheet.Cell(row, barcodeCol).GetString().Trim();
                var itemName = worksheet.Cell(row, nameCol).GetString().Trim();

                if (string.IsNullOrWhiteSpace(barcode) || string.IsNullOrWhiteSpace(itemName))
                    continue;

                var item = new AuditTemplateInventoryItem
                {
                    AuditTemplateId = templateId,
                    BarcodeValue = barcode,
                    ItemName = itemName,
                    Location = locationCol > 0 ? worksheet.Cell(row, locationCol).GetString().Trim() : null,
                    SKU = skuCol > 0 ? worksheet.Cell(row, skuCol).GetString().Trim() : null,
                    Category = categoryCol > 0 ? worksheet.Cell(row, categoryCol).GetString().Trim() : null,
                    ExpectedQuantity = decimal.TryParse(worksheet.Cell(row, qtyCol).GetString(), out var qty) ? qty : 0,
                    Unit = unitCol > 0 ? worksheet.Cell(row, unitCol).GetString().Trim() : null,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuditTemplateInventoryItems.Add(item);
                importedCount++;
            }

            await _context.SaveChangesAsync();
            return importedCount;
        }

        public async Task<bool> DeleteInventoryItemAsync(int itemId, int templateId, int companyId)
        {
            var template = await _context.AuditTemplates
                .FirstOrDefaultAsync(t => t.AuditTemplateId == templateId && t.CompanyId == companyId);
            if (template == null) return false;

            var item = await _context.AuditTemplateInventoryItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.AuditTemplateId == templateId);

            if (item == null) return false;

            _context.AuditTemplateInventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearInventoryAsync(int templateId, int companyId)
        {
            var template = await _context.AuditTemplates
                .FirstOrDefaultAsync(t => t.AuditTemplateId == templateId && t.CompanyId == companyId);
            if (template == null) return false;

            var items = await _context.AuditTemplateInventoryItems
                .Where(i => i.AuditTemplateId == templateId)
                .ToListAsync();

            _context.AuditTemplateInventoryItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BarcodeScanResultDto> ScanBarcodeAsync(int auditId, string barcodeValue, decimal actualQuantity, int companyId)
        {
            var audit = await _context.Audits
                .Include(a => a.AuditTemplate)
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            if (audit == null)
                return new BarcodeScanResultDto { Success = false, Message = "Audit not found." };

            if (audit.Status != AuditStatus.InProgress)
                return new BarcodeScanResultDto { Success = false, Message = "Audit is not in progress." };

            // Look up barcode in inventory
            var inventoryItem = await _context.AuditTemplateInventoryItems
                .FirstOrDefaultAsync(i => i.AuditTemplateId == audit.AuditTemplateId && i.BarcodeValue == barcodeValue);

            // Check if same barcode already scanned in this audit
            var existingScan = await _context.AuditBarcodeScans
                .FirstOrDefaultAsync(s => s.AuditId == auditId && s.BarcodeValue == barcodeValue);

            BarcodeScanStatus status;
            decimal expectedQty = 0;
            string? itemName = null;
            string? location = null;
            string? sku = null;
            string? unit = null;

            if (inventoryItem != null)
            {
                expectedQty = inventoryItem.ExpectedQuantity;
                itemName = inventoryItem.ItemName;
                location = inventoryItem.Location;
                sku = inventoryItem.SKU;
                unit = inventoryItem.Unit;

                decimal discrepancy = actualQuantity - expectedQty;
                if (discrepancy == 0) status = BarcodeScanStatus.Matched;
                else if (discrepancy > 0) status = BarcodeScanStatus.Surplus;
                else status = BarcodeScanStatus.Shortage;
            }
            else
            {
                status = BarcodeScanStatus.Unrecognized;
            }

            if (existingScan != null)
            {
                // Update existing scan (accumulate quantity)
                existingScan.ActualQuantity = actualQuantity;
                existingScan.DiscrepancyQuantity = inventoryItem != null ? actualQuantity - expectedQty : null;
                existingScan.Status = status;
                existingScan.ScanCount++;
                existingScan.LastScannedAt = DateTime.UtcNow;
                existingScan.Notes = null;
            }
            else
            {
                // Create new scan record
                var scan = new AuditBarcodeScan
                {
                    AuditId = auditId,
                    BarcodeValue = barcodeValue,
                    ItemNameSnapshot = itemName,
                    LocationSnapshot = location,
                    SKUSnapshot = sku,
                    ExpectedQuantity = inventoryItem != null ? expectedQty : null,
                    ActualQuantity = actualQuantity,
                    Unit = unit,
                    DiscrepancyQuantity = inventoryItem != null ? actualQuantity - expectedQty : null,
                    Status = status,
                    ScanCount = 1,
                    FirstScannedAt = DateTime.UtcNow,
                    LastScannedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AuditBarcodeScans.Add(scan);
            }

            await _context.SaveChangesAsync();

            return new BarcodeScanResultDto
            {
                Success = true,
                Message = existingScan != null ? "Scan updated." : "Scan recorded.",
                InventoryItem = inventoryItem != null ? new InventoryItemDto
                {
                    Id = inventoryItem.Id,
                    BarcodeValue = inventoryItem.BarcodeValue,
                    ItemName = inventoryItem.ItemName,
                    Location = inventoryItem.Location,
                    SKU = inventoryItem.SKU,
                    ExpectedQuantity = inventoryItem.ExpectedQuantity,
                    Unit = inventoryItem.Unit
                } : null,
                IsNewScan = existingScan == null,
                ActualQuantity = actualQuantity,
                Status = status
            };
        }

        public async Task<List<InventoryComparisonDto>> GetComparisonReportAsync(int auditId, int companyId)
        {
            var audit = await _context.Audits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            if (audit == null) return new List<InventoryComparisonDto>();

            var scans = await _context.AuditBarcodeScans
                .Where(s => s.AuditId == auditId)
                .ToListAsync();

            var inventoryItems = await _context.AuditTemplateInventoryItems
                .Where(i => i.AuditTemplateId == audit.AuditTemplateId)
                .ToListAsync();

            var result = new List<InventoryComparisonDto>();

            // Items that were scanned
            foreach (var scan in scans)
            {
                result.Add(new InventoryComparisonDto
                {
                    ItemName = scan.ItemNameSnapshot ?? "Unknown",
                    BarcodeValue = scan.BarcodeValue,
                    Location = scan.LocationSnapshot,
                    ExpectedQuantity = scan.ExpectedQuantity ?? 0,
                    ActualQuantity = scan.ActualQuantity,
                    Discrepancy = scan.DiscrepancyQuantity ?? 0,
                    Status = scan.Status,
                    Unit = scan.Unit
                });
            }

            // Items not scanned (Missing)
            var scannedBarcodes = scans.Select(s => s.BarcodeValue).ToHashSet();
            foreach (var item in inventoryItems.Where(i => !scannedBarcodes.Contains(i.BarcodeValue)))
            {
                result.Add(new InventoryComparisonDto
                {
                    ItemName = item.ItemName,
                    BarcodeValue = item.BarcodeValue,
                    Location = item.Location,
                    ExpectedQuantity = item.ExpectedQuantity,
                    ActualQuantity = 0,
                    Discrepancy = -item.ExpectedQuantity,
                    Status = BarcodeScanStatus.Missing,
                    Unit = item.Unit
                });
            }

            return result.OrderBy(r => r.Status).ThenBy(r => r.ItemName).ToList();
        }

        public async Task<bool> DeleteScanAsync(int scanId, int auditId, int companyId)
        {
            var audit = await _context.Audits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);
            if (audit == null) return false;

            var scan = await _context.AuditBarcodeScans
                .FirstOrDefaultAsync(s => s.Id == scanId && s.AuditId == auditId);

            if (scan == null) return false;

            _context.AuditBarcodeScans.Remove(scan);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
