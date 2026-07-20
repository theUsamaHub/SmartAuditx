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
