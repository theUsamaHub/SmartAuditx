using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels.AuditModule;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize]
    public class ManagerController : Controller
    {
        private readonly IManagerService _managerService;
        private readonly IReportService _reportService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ManagerController(
            IManagerService managerService,
            IReportService reportService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _managerService = managerService;
            _reportService = reportService;
            _userManager = userManager;
            _context = context;
        }

        private async Task<int?> GetManagerBranchIdAsync(ApplicationUser user)
        {
            if (user.EmployeeId.HasValue)
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeId == user.EmployeeId.Value && !e.IsDeleted);
                return employee?.BranchId;
            }
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var branchId = await GetManagerBranchIdAsync(user);
            ViewData["Title"] = "Manager Dashboard";
            ViewBag.ManagerBranchId = branchId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStats(string? allBranches)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

            int? effectiveBranchId = null;
            if (allBranches != "true")
            {
                effectiveBranchId = await GetManagerBranchIdAsync(user);
            }

            var stats = await _managerService.GetStatsAsync(user.CompanyId, effectiveBranchId);
            return Json(new { success = true, data = stats });
        }

        [HttpGet]
        public async Task<IActionResult> Audits()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            ViewData["Title"] = "Audit Reviews";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AuditList(string? allBranches)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

            int? effectiveBranchId = null;
            if (allBranches != "true")
            {
                effectiveBranchId = await GetManagerBranchIdAsync(user);
            }

            var audits = await _managerService.GetAuditsByCompanyAsync(user.CompanyId, effectiveBranchId);
            return Json(new { success = true, data = audits });
        }

        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var audit = await _managerService.GetAuditForReviewAsync(id, user.CompanyId);
            if (audit == null) return NotFound();

            ViewData["Title"] = "Review Audit";
            return View(audit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? reviewNotes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

            var (success, message) = await _managerService.ApproveAuditAsync(id, user.CompanyId, user.Id, reviewNotes);
            return Json(new { success = success, message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reviewNotes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

            var (success, message) = await _managerService.RejectAuditAsync(id, user.CompanyId, user.Id, reviewNotes);
            return Json(new { success = success, message = message });
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
