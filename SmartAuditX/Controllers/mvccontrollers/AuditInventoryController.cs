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
