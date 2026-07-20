using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels.AuditModule;
using SmartAuditX.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartAuditX.Controllers.mvccontrollers
{
    public class ScanRequest
    {
        public string BarcodeValue { get; set; } = string.Empty;
        public decimal ActualQuantity { get; set; }
    }

    [Authorize]
    public class AuditorController : Controller
    {
        private readonly IAuditorService _auditorService;
        private readonly IAuditInventoryService _inventoryService;
        private readonly IReportService _reportService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditorController(
            IAuditorService auditorService,
            IAuditInventoryService inventoryService,
            IReportService reportService,
            UserManager<ApplicationUser> userManager)
        {
            _auditorService = auditorService;
            _inventoryService = inventoryService;
            _reportService = reportService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            ViewData["Title"] = "Auditor Dashboard";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

            var stats = await _auditorService.GetStatsAsync(user.Id);
            return Json(new { success = true, data = stats });
        }

        [HttpGet]
        public async Task<IActionResult> AssignedAudits()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

            var audits = await _auditorService.GetAssignedAuditsAsync(user.Id);
            return Json(new { success = true, data = audits });
        }

        [HttpGet]
        public async Task<IActionResult> Conduct(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var audit = await _auditorService.GetAuditForConductAsync(id, user.Id);
            if (audit == null) return NotFound();

            ViewData["Title"] = "Conduct Audit";
            return View(audit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

            var result = await _auditorService.StartAuditAsync(id, user.Id);
            return Json(new { success = result, message = result ? "Audit started." : "Failed to start audit." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProgress(int id, [FromBody] List<AuditResponseViewModel> responses)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

            var result = await _auditorService.SaveResponsesAsync(id, user.Id, responses);
            return Json(new { success = result, message = result ? "Progress saved." : "Failed to save progress." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id, [FromBody] List<AuditResponseViewModel> responses)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

            var result = await _auditorService.SubmitAuditAsync(id, user.Id, responses);
            return Json(new { success = result, message = result ? "Audit submitted successfully." : "Failed to submit audit." });
        }

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

        [HttpGet]
        public async Task<IActionResult> DownloadReport(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            try
            {
                var pdfBytes = await _reportService.GenerateAuditReportAsync(id, user.CompanyId);
                return File(pdfBytes, "application/pdf", $"AuditReport_{id}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Report generation error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Failed to generate report: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadInventoryReport(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            try
            {
                var pdfBytes = await _reportService.GenerateInventoryReportAsync(id, user.CompanyId);
                return File(pdfBytes, "application/pdf", $"InventoryReport_{id}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inventory report error: {ex.Message}");
                return BadRequest($"Failed to generate report: {ex.Message}");
            }
        }
    }
}
