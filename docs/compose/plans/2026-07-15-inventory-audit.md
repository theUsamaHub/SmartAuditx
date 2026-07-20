# Inventory Audit Feature Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use compose:subagent (recommended) or compose:execute to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Enable companies to upload inventory lists via Excel, have auditors scan barcodes during audits, and generate comparison reports showing expected vs actual quantities.

**Architecture:** Company uploads Excel → stored in AuditTemplateInventoryItems → Auditor scans barcodes during audit → system compares expected vs actual → generates discrepancy report. Uses EPPlus for Excel reading, ZXing.Net already installed for barcode support.

**Tech Stack:** ASP.NET Core MVC, Entity Framework Core, EPPlus (Excel), ZXing.Net (barcode), Bootstrap 5

## Global Constraints

- .NET 10.0, C# with nullable enabled
- Company-scoped data isolation via CompanyId on all entities
- Soft delete pattern: IsDeleted, IsActive, CreatedAt, UpdatedAt
- Existing service pattern: Interface + Implementation, Scoped DI
- AJAX endpoints for CRUD operations returning JSON
- Views use _CompanyLayout.cshtml or _AuditorLayout.cshtml

---

## File Structure

| Action | File | Purpose |
|--------|------|---------|
| Create | `Models/AuditModule/AuditTemplateInventoryItem.cs` | Excel inventory data storage |
| Create | `Models/AuditModule/AuditBarcodeScan.cs` | Barcode scan records with quantity tracking |
| Create | `Services/Interfaces/IAuditInventoryService.cs` | Inventory service interface |
| Create | `Services/Implementations/AuditInventoryService.cs` | Inventory service implementation |
| Modify | `Data/ApplicationDbContext.cs` | Add new DbSets and configurations |
| Modify | `Program.cs` | Register new service |
| Create | `Controllers/mvccontrollers/AuditInventoryController.cs` | Company-side inventory management |
| Modify | `Controllers/mvccontrollers/AuditorController.cs` | Add barcode scan endpoints |
| Create | `Views/Audit/Inventory.cshtml` | Company inventory upload/management page |
| Create | `Views/Auditor/Scan.cshtml` | Barcode scan interface for auditor |
| Modify | `Views/Audit/Index.cshtml` | Add Inventory button |
| Modify | `Views/Auditor/Index.cshtml` | Add scan button for active audits |
| Modify | `wwwroot/js/Auditor/auditor-conduct.js` | Add barcode scan handling |

---

## Task 1: Create Inventory Models + DbContext

**Files:**
- Create: `SmartAuditX/Models/AuditModule/AuditTemplateInventoryItem.cs`
- Create: `SmartAuditX/Models/AuditModule/AuditBarcodeScan.cs`
- Modify: `SmartAuditX/Data/ApplicationDbContext.cs`

**Interfaces:**
- Consumes: BaseEntity, AuditableEntity, AuditTemplate, Audit
- Produces: DbSet registrations for EF Core

- [ ] **Step 1: Create AuditTemplateInventoryItem model**

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    [Table("AuditTemplateInventoryItems")]
    public class AuditTemplateInventoryItem : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AuditTemplateId { get; set; }

        [ForeignKey("AuditTemplateId")]
        public virtual AuditTemplate? AuditTemplate { get; set; }

        [Required]
        [MaxLength(100)]
        public string BarcodeValue { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(100)]
        public string? SKU { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal ExpectedQuantity { get; set; }

        [MaxLength(30)]
        public string? Unit { get; set; }

        public virtual AuditTemplate? Template { get; set; }
    }
}
```

- [ ] **Step 2: Create AuditBarcodeScan model**

```csharp
using SmartAuditX.Models.AuditModule.AuditEnums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    [Table("AuditBarcodeScans")]
    public class AuditBarcodeScan : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AuditId { get; set; }

        [ForeignKey("AuditId")]
        public virtual Audit? Audit { get; set; }

        [ForeignKey("AuditResponse")]
        public int? AuditResponseId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BarcodeValue { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? ItemNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? LocationSnapshot { get; set; }

        [MaxLength(100)]
        public string? SKUSnapshot { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? ExpectedQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,4)")]
        public decimal ActualQuantity { get; set; } = 0;

        [MaxLength(30)]
        public string? Unit { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? DiscrepancyQuantity { get; set; }

        public BarcodeScanStatus Status { get; set; } = BarcodeScanStatus.Unrecognized;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int ScanCount { get; set; } = 1;

        public DateTime FirstScannedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastScannedAt { get; set; } = DateTime.UtcNow;

        public virtual Audit? Audit { get; set; }
        public virtual AuditResponse? AuditResponse { get; set; }
    }
}
```

- [ ] **Step 3: Add BarcodeScanStatus enum if not exists**

Check if `SmartAuditX/Models/AuditModule/AuditEnums/BarcodeScanStatus.cs` exists. If not, create it:

```csharp
namespace SmartAuditX.Models.AuditModule.AuditEnums
{
    public enum BarcodeScanStatus
    {
        Matched,
        Surplus,
        Shortage,
        Missing,
        Unrecognized
    }
}
```

- [ ] **Step 4: Add DbSets to ApplicationDbContext**

Add to `Data/ApplicationDbContext.cs`:
```csharp
public DbSet<AuditTemplateInventoryItem> AuditTemplateInventoryItems { get; set; }
public DbSet<AuditBarcodeScan> AuditBarcodeScans { get; set; }
```

Add entity configurations in `OnModelCreating`:
```csharp
builder.Entity<AuditTemplateInventoryItem>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasOne(e => e.AuditTemplate)
        .WithMany()
        .HasForeignKey(e => e.AuditTemplateId)
        .OnDelete(DeleteBehavior.Cascade);
    entity.HasIndex(e => e.AuditTemplateId);
    entity.HasIndex(e => e.BarcodeValue);
});

builder.Entity<AuditBarcodeScan>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasOne(e => e.Audit)
        .WithMany()
        .HasForeignKey(e => e.AuditId)
        .OnDelete(DeleteBehavior.Cascade);
    entity.HasIndex(e => new { e.AuditId, e.BarcodeValue }).IsUnique();
    entity.HasIndex(e => e.AuditId);
});
```

- [ ] **Step 5: Build and verify**

Run: `dotnet build SmartAuditX/SmartAuditX.csproj`
Expected: Build succeeded

- [ ] **Step 6: Create EF migration**

Run: `dotnet ef migrations add AddInventoryAuditModels --project SmartAuditX/SmartAuditX.csproj`
Run: `dotnet ef database update --project SmartAuditX/SmartAuditX.csproj`

---

## Task 2: Install EPPlus + Create Inventory Service

**Files:**
- Modify: `SmartAuditX/SmartAuditX.csproj` (add EPPlus package)
- Create: `SmartAuditX/Services/Interfaces/IAuditInventoryService.cs`
- Create: `SmartAuditX/Services/Implementations/AuditInventoryService.cs`
- Modify: `SmartAuditX/Program.cs` (register service)

**Interfaces:**
- Consumes: AuditTemplateInventoryItem, AuditBarcodeScan, AuditTemplate, Audit, ApplicationDbContext
- Produces: IAuditInventoryService with CRUD + Excel upload + barcode scan methods

- [ ] **Step 1: Add EPPlus NuGet package**

Run: `dotnet add SmartAuditX/SmartAuditX.csproj package EPPlus`

- [ ] **Step 2: Create IAuditInventoryService interface**

```csharp
using SmartAuditX.Models.AuditModule;
using SmartAuditX.Models.AuditModule.AuditEnums;

namespace SmartAuditX.Services.Interfaces
{
    public class InventoryItemDto
    {
        public int Id { get; set; }
        public string BarcodeValue { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? SKU { get; set; }
        public string? Category { get; set; }
        public decimal ExpectedQuantity { get; set; }
        public string? Unit { get; set; }
    }

    public class BarcodeScanResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public InventoryItemDto? InventoryItem { get; set; }
        public bool IsNewScan { get; set; }
        public decimal ActualQuantity { get; set; }
        public BarcodeScanStatus Status { get; set; }
    }

    public class InventoryComparisonDto
    {
        public string ItemName { get; set; } = string.Empty;
        public string BarcodeValue { get; set; } = string.Empty;
        public string? Location { get; set; }
        public decimal ExpectedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal Discrepancy { get; set; }
        public BarcodeScanStatus Status { get; set; }
        public string? Unit { get; set; }
    }

    public interface IAuditInventoryService
    {
        Task<List<InventoryItemDto>> GetInventoryItemsAsync(int templateId, int companyId);
        Task<int> UploadInventoryFromExcelAsync(int templateId, int companyId, Stream fileStream, string fileName);
        Task<bool> DeleteInventoryItemAsync(int itemId, int templateId, int companyId);
        Task<bool> ClearInventoryAsync(int templateId, int companyId);
        Task<BarcodeScanResultDto> ScanBarcodeAsync(int auditId, string barcodeValue, decimal actualQuantity, int companyId);
        Task<List<InventoryComparisonDto>> GetComparisonReportAsync(int auditId, int companyId);
        Task<bool> DeleteScanAsync(int scanId, int auditId, int companyId);
    }
}
```

- [ ] **Step 3: Create AuditInventoryService implementation**

```csharp
using SmartAuditX.Data;
using SmartAuditX.Models.AuditModule;
using SmartAuditX.Models.AuditModule.AuditEnums;
using SmartAuditX.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace SmartAuditX.Services.Implementations
{
    public class AuditInventoryService : IAuditInventoryService
    {
        private readonly ApplicationDbContext _context;

        public AuditInventoryService(ApplicationDbContext context)
        {
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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

            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null) return 0;

            int rowCount = worksheet.Dimension?.Rows ?? 0;
            int importedCount = 0;

            // Find column indices by header names
            int barcodeCol = -1, nameCol = -1, qtyCol = -1, locationCol = -1, skuCol = -1, categoryCol = -1, unitCol = -1;

            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var header = worksheet.Cells[1, col].Text?.Trim().ToLower();
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
                var barcode = worksheet.Cells[row, barcodeCol].Text?.Trim();
                var itemName = worksheet.Cells[row, nameCol].Text?.Trim();

                if (string.IsNullOrWhiteSpace(barcode) || string.IsNullOrWhiteSpace(itemName))
                    continue;

                var item = new AuditTemplateInventoryItem
                {
                    AuditTemplateId = templateId,
                    BarcodeValue = barcode,
                    ItemName = itemName,
                    Location = locationCol > 0 ? worksheet.Cells[row, locationCol].Text?.Trim() : null,
                    SKU = skuCol > 0 ? worksheet.Cells[row, skuCol].Text?.Trim() : null,
                    Category = categoryCol > 0 ? worksheet.Cells[row, categoryCol].Text?.Trim() : null,
                    ExpectedQuantity = decimal.TryParse(worksheet.Cells[row, qtyCol].Text, out var qty) ? qty : 0,
                    Unit = unitCol > 0 ? worksheet.Cells[row, unitCol].Text?.Trim() : null,
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
```

- [ ] **Step 4: Register service in Program.cs**

Add to `Program.cs` after other service registrations:
```csharp
builder.Services.AddScoped<IAuditInventoryService, AuditInventoryService>();
```

- [ ] **Step 5: Build and verify**

Run: `dotnet build SmartAuditX/SmartAuditX.csproj`
Expected: Build succeeded

---

## Task 3: Create Inventory Controller + Company View

**Files:**
- Create: `SmartAuditX/Controllers/mvccontrollers/AuditInventoryController.cs`
- Create: `SmartAuditX/Views/Audit/Inventory.cshtml`
- Modify: `SmartAuditX/Views/Audit/Index.cshtml` (add Inventory button)

**Interfaces:**
- Consumes: IAuditInventoryService, UserManager, IAuditTemplateService
- Produces: Inventory management page for company panel

- [ ] **Step 1: Create AuditInventoryController**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Models;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize]
    public class AuditInventoryController : Controller
    {
        private readonly IAuditInventoryService _inventoryService;
        private readonly IAuditTemplateService _templateService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditInventoryController(
            IAuditInventoryService inventoryService,
            IAuditTemplateService templateService,
            UserManager<ApplicationUser> userManager)
        {
            _inventoryService = inventoryService;
            _templateService = templateService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int templateId)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Forbid();

            var template = await _templateService.GetTemplateByIdAsync(templateId, companyId.Value);
            if (template == null) return NotFound();

            ViewData["Title"] = $"Inventory - {template.Title}";
            ViewBag.TemplateId = templateId;
            ViewBag.TemplateTitle = template.Title;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List(int templateId)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Unauthorized(new { success = false, message = "Unable to resolve company context." });

            var items = await _inventoryService.GetInventoryItemsAsync(templateId, companyId.Value);
            return Json(new { success = true, data = items });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int templateId, IFormFile file)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Unauthorized(new { success = false, message = "Unable to resolve company context." });

            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "Please select an Excel file." });

            var validExtensions = new[] { ".xlsx", ".xls", ".csv" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!validExtensions.Contains(extension))
                return BadRequest(new { success = false, message = "Only .xlsx, .xls, or .csv files are allowed." });

            try
            {
                using var stream = file.OpenReadStream();
                var count = await _inventoryService.UploadInventoryFromExcelAsync(templateId, companyId.Value, stream, file.FileName);
                return Json(new { success = true, message = $"Successfully imported {count} inventory items.", count });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Failed to process the Excel file." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int templateId, int itemId)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Unauthorized(new { success = false, message = "Unable to resolve company context." });

            var result = await _inventoryService.DeleteInventoryItemAsync(itemId, templateId, companyId.Value);
            return Json(new { success = result, message = result ? "Item deleted." : "Failed to delete item." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear(int templateId)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Unauthorized(new { success = false, message = "Unable to resolve company context." });

            var result = await _inventoryService.ClearInventoryAsync(templateId, companyId.Value);
            return Json(new { success = result, message = result ? "Inventory cleared." : "Failed to clear inventory." });
        }

        private async Task<int?> GetCurrentCompanyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.IsDeleted || !user.IsActive) return null;
            return user.CompanyId;
        }
    }
}
```

- [ ] **Step 2: Create Inventory.cshtml view**

```html
@{
    ViewData["Title"] = "Inventory Management";
    Layout = "~/Views/Shared/Layouts/_CompanyLayout.cshtml";
}

@section Styles {
    <link rel="stylesheet" href="~/css/Company/css/audit-template.css" asp-append-version="true" />
}

<div class="page-header">
    <div class="page-header-content">
        <div class="page-title">
            <h1>Inventory Management</h1>
            <p class="page-subtitle">Manage inventory items for template: @ViewBag.TemplateTitle</p>
        </div>
        <div class="page-actions">
            <a href="/Audit" class="btn btn-secondary">
                <i class="bi bi-arrow-left"></i> Back to Audits
            </a>
        </div>
    </div>
</div>

<!-- Upload Section -->
<div class="content-card mb-4" style="padding: var(--spacing-xl);">
    <h3 class="form-section-title">Upload Inventory</h3>
    <p class="text-muted mb-3">Upload an Excel file (.xlsx, .xls, or .csv) with columns: Barcode, ItemName, Quantity, Location (optional), SKU (optional), Category (optional), Unit (optional)</p>
    
    <form id="uploadForm" enctype="multipart/form-data">
        <input type="hidden" name="templateId" value="@ViewBag.TemplateId" />
        <div class="row g-3 align-items-end">
            <div class="col-md-6">
                <input type="file" class="form-control" id="inventoryFile" accept=".xlsx,.xls,.csv" />
            </div>
            <div class="col-md-3">
                <button type="submit" class="btn btn-primary" id="uploadBtn">
                    <i class="bi bi-upload"></i> Upload
                </button>
            </div>
            <div class="col-md-3 text-end">
                <button type="button" class="btn btn-danger" onclick="clearInventory()">
                    <i class="bi bi-trash"></i> Clear All
                </button>
            </div>
        </div>
    </form>
</div>

<!-- Inventory Table -->
<div class="content-card">
    <div class="px-3 px-md-4 pt-3 pt-md-4 pb-0">
        <h2 class="h5 mb-1">Inventory Items</h2>
        <p class="text-muted mb-0">Total items: <span id="itemCount">0</span></p>
    </div>
    <div class="table-responsive px-3 px-md-4 pb-3 pb-md-4 pt-3">
        <table class="table align-middle mb-0">
            <thead>
                <tr>
                    <th>Barcode</th>
                    <th>Item Name</th>
                    <th>Expected Qty</th>
                    <th>Unit</th>
                    <th>Location</th>
                    <th>SKU</th>
                    <th>Category</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody id="inventoryTableBody">
                <tr>
                    <td colspan="8" class="text-center">Loading inventory...</td>
                </tr>
            </tbody>
        </table>
    </div>
</div>

@section Scripts {
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        const templateId = @ViewBag.TemplateId;

        $(document).ready(function() {
            loadInventory();
            $('#uploadForm').on('submit', uploadFile);
        });

        async function loadInventory() {
            try {
                const response = await fetch('/AuditInventory/List?templateId=' + templateId);
                const data = await response.json();
                if (data.success) {
                    renderTable(data.data);
                }
            } catch (error) {
                console.error('Error loading inventory:', error);
            }
        }

        function renderTable(items) {
            const tbody = document.getElementById('inventoryTableBody');
            document.getElementById('itemCount').textContent = items.length;

            if (!items || items.length === 0) {
                tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted">No inventory items uploaded yet</td></tr>';
                return;
            }

            tbody.innerHTML = items.map(item => `
                <tr>
                    <td><code>${escapeHtml(item.barcodeValue)}</code></td>
                    <td><strong>${escapeHtml(item.itemName)}</strong></td>
                    <td>${item.expectedQuantity}</td>
                    <td>${escapeHtml(item.unit || '-')}</td>
                    <td>${escapeHtml(item.location || '-')}</td>
                    <td>${escapeHtml(item.sku || '-')}</td>
                    <td>${escapeHtml(item.category || '-')}</td>
                    <td>
                        <button type="button" class="btn btn-sm btn-outline-danger" onclick="deleteItem(${item.id})">
                            <i class="bi bi-trash"></i>
                        </button>
                    </td>
                </tr>
            `).join('');
        }

        async function uploadFile(e) {
            e.preventDefault();
            const fileInput = document.getElementById('inventoryFile');
            if (!fileInput.files[0]) {
                Swal.fire('Error', 'Please select a file.', 'error');
                return;
            }

            const formData = new FormData();
            formData.append('file', fileInput.files[0]);
            formData.append('templateId', templateId);

            const btn = document.getElementById('uploadBtn');
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Uploading...';

            try {
                const response = await fetch('/AuditInventory/Upload?templateId=' + templateId, {
                    method: 'POST',
                    body: formData
                });
                const data = await response.json();

                if (data.success) {
                    Swal.fire('Success', data.message, 'success');
                    fileInput.value = '';
                    loadInventory();
                } else {
                    Swal.fire('Error', data.message, 'error');
                }
            } catch (error) {
                Swal.fire('Error', 'Upload failed.', 'error');
            } finally {
                btn.disabled = false;
                btn.innerHTML = '<i class="bi bi-upload"></i> Upload';
            }
        }

        async function deleteItem(itemId) {
            const result = await Swal.fire({
                title: 'Delete Item',
                text: 'Are you sure?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d45656',
                confirmButtonText: 'Delete'
            });

            if (result.isConfirmed) {
                const formData = new FormData();
                formData.append('templateId', templateId);
                formData.append('itemId', itemId);

                const response = await fetch('/AuditInventory/DeleteItem?templateId=' + templateId + '&itemId=' + itemId, {
                    method: 'POST',
                    body: formData
                });
                const data = await response.json();

                if (data.success) {
                    loadInventory();
                } else {
                    Swal.fire('Error', data.message, 'error');
                }
            }
        }

        async function clearInventory() {
            const result = await Swal.fire({
                title: 'Clear All Inventory',
                text: 'This will remove all inventory items. Are you sure?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d45656',
                confirmButtonText: 'Clear All'
            });

            if (result.isConfirmed) {
                const formData = new FormData();
                formData.append('templateId', templateId);

                const response = await fetch('/AuditInventory/Clear?templateId=' + templateId, {
                    method: 'POST',
                    body: formData
                });
                const data = await response.json();

                if (data.success) {
                    Swal.fire('Success', data.message, 'success');
                    loadInventory();
                } else {
                    Swal.fire('Error', data.message, 'error');
                }
            }
        }

        function escapeHtml(text) {
            if (!text) return '';
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }
    </script>
}
```

- [ ] **Step 3: Add Inventory button to Audit Index**

In `Views/Audit/Index.cshtml`, add a button in the actions column for each audit:
```html
${audit.status === 2 ? `<a href="/AuditInventory/Index?templateId=${audit.auditTemplateId}" class="btn btn-outline-info btn-sm" title="Manage Inventory"><i class="bi bi-box-seam"></i></a>` : ''}
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build SmartAuditX/SmartAuditX.csproj`
Expected: Build succeeded

---

## Task 4: Add Barcode Scan to Auditor Flow

**Files:**
- Modify: `SmartAuditX/Controllers/mvccontrollers/AuditorController.cs` (add scan endpoints)
- Modify: `SmartAuditX/Views/Auditor/Conduct.cshtml` (add barcode scan section)
- Modify: `SmartAuditX/Views/Auditor/Index.cshtml` (add scan button)

**Interfaces:**
- Consumes: IAuditInventoryService, IAuditorService
- Produces: Barcode scan during audit, comparison report

- [ ] **Step 1: Add scan endpoints to AuditorController**

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ScanBarcode(int id, [FromBody] ScanRequest request)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

    var result = await _inventoryService.ScanBarcodeAsync(id, request.BarcodeValue, request.ActualQuantity, user.CompanyId);
    return Json(result);
}

[HttpGet]
public async Task<IActionResult> ComparisonReport(int id)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

    var report = await _inventoryService.GetComparisonReportAsync(id, user.CompanyId);
    return Json(new { success = true, data = report });
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteScan(int scanId, int auditId)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

    var result = await _inventoryService.DeleteScanAsync(scanId, auditId, user.CompanyId);
    return Json(new { success = result, message = result ? "Scan deleted." : "Failed to delete scan." });
}
```

Add ScanRequest model (can be in the same file or a ViewModels folder):
```csharp
public class ScanRequest
{
    public string BarcodeValue { get; set; } = string.Empty;
    public decimal ActualQuantity { get; set; }
}
```

- [ ] **Step 2: Add barcode scan section to Conduct.cshtml**

Add after the form sections, before the submit button:
```html
@if (Model.Status == SmartAuditX.Models.AuditModule.AuditEnums.AuditStatus.InProgress)
{
    <div class="form-section">
        <h3 class="form-section-title">Barcode Scanning</h3>
        <div class="row g-3 mb-3">
            <div class="col-md-5">
                <label class="form-label">Barcode Value</label>
                <input type="text" class="form-control" id="barcodeInput" placeholder="Scan or type barcode" />
            </div>
            <div class="col-md-3">
                <label class="form-label">Actual Quantity</label>
                <input type="number" class="form-control" id="barcodeQuantity" value="1" min="0" step="0.01" />
            </div>
            <div class="col-md-4 d-flex align-items-end">
                <button type="button" class="btn btn-primary" onclick="scanBarcode()">
                    <i class="bi bi-upc-scan"></i> Scan
                </button>
            </div>
        </div>
        <div id="scanResult" class="alert d-none mb-3"></div>
        
        <h4 class="h6 mb-2">Scanned Items</h4>
        <div class="table-responsive">
            <table class="table table-sm" id="scansTable">
                <thead>
                    <tr>
                        <th>Barcode</th>
                        <th>Item</th>
                        <th>Expected</th>
                        <th>Actual</th>
                        <th>Status</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody id="scansTableBody">
                    <tr><td colspan="6" class="text-center text-muted">No scans yet</td></tr>
                </tbody>
            </table>
        </div>
    </div>
}
```

- [ ] **Step 3: Add scan JavaScript to auditor-conduct.js**

```javascript
async function scanBarcode() {
    const barcode = document.getElementById('barcodeInput').value.trim();
    const quantity = parseFloat(document.getElementById('barcodeQuantity').value) || 1;
    const resultDiv = document.getElementById('scanResult');

    if (!barcode) {
        resultDiv.className = 'alert alert-danger mb-3';
        resultDiv.textContent = 'Please enter a barcode value.';
        resultDiv.classList.remove('d-none');
        return;
    }

    try {
        const response = await fetch('/Auditor/ScanBarcode/' + auditId, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ barcodeValue: barcode, actualQuantity: quantity })
        });

        const result = await response.json();

        if (result.success) {
            resultDiv.className = 'alert alert-success mb-3';
            resultDiv.textContent = result.message;
            resultDiv.classList.remove('d-none');
            document.getElementById('barcodeInput').value = '';
            document.getElementById('barcodeQuantity').value = '1';
            loadScans();
        } else {
            resultDiv.className = 'alert alert-danger mb-3';
            resultDiv.textContent = result.message;
            resultDiv.classList.remove('d-none');
        }
    } catch (error) {
        console.error('Error scanning barcode:', error);
    }
}

async function loadScans() {
    try {
        const response = await fetch('/Auditor/ComparisonReport/' + auditId);
        const data = await response.json();
        if (data.success) {
            renderScansTable(data.data);
        }
    } catch (error) {
        console.error('Error loading scans:', error);
    }
}

function renderScansTable(scans) {
    const tbody = document.getElementById('scansTableBody');
    if (!scans || scans.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No scans yet</td></tr>';
        return;
    }

    const statusColors = {
        'Matched': 'success',
        'Surplus': 'info',
        'Shortage': 'warning',
        'Missing': 'danger',
        'Unrecognized': 'secondary'
    };

    tbody.innerHTML = scans.map(scan => `
        <tr>
            <td><code>${escapeHtml(scan.barcodeValue)}</code></td>
            <td>${escapeHtml(scan.itemName)}</td>
            <td>${scan.expectedQuantity}</td>
            <td>${scan.actualQuantity}</td>
            <td><span class="badge bg-${statusColors[scan.status] || 'secondary'}">${scan.status}</span></td>
            <td>
                ${scan.status !== 'Missing' ? `<button type="button" class="btn btn-sm btn-outline-danger" onclick="deleteScan(${scan.id || 0})"><i class="bi bi-trash"></i></button>` : ''}
            </td>
        </tr>
    `).join('');
}

async function deleteScan(scanId) {
    if (!scanId) return;
    try {
        await fetch('/Auditor/DeleteScan?scanId=' + scanId + '&auditId=' + auditId, {
            method: 'POST',
            headers: { 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value }
        });
        loadScans();
    } catch (error) {
        console.error('Error deleting scan:', error);
    }
}
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build SmartAuditX/SmartAuditX.csproj`
Expected: Build succeeded

---

## Task 5: End-to-End Test

- [ ] **Step 1: Start the application**

Run: `dotnet run --project SmartAuditX/SmartAuditX.csproj`

- [ ] **Step 2: Test Company Flow**
1. Login as CompanyOwner
2. Go to Audit Templates → Create a template with Barcode field
3. Go to Audits → Create an audit from the template
4. Go to Inventory → Upload Excel file with inventory items
5. Verify items appear in the table

- [ ] **Step 3: Test Auditor Flow**
1. Login as Auditor
2. Start the assigned audit
3. Scan barcodes → verify items match inventory
4. Enter quantities → verify status (Matched/Surplus/Shortage)
5. Submit audit

- [ ] **Step 4: Test Review Flow**
1. Login as CompanyOwner
2. Go to Audits → Click Review on completed audit
3. Verify comparison report shows all items
4. Approve or reject the audit
