using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels.AuditModule;
using SmartAuditX.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize]
    public class AuditController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly IAuditTemplateService _templateService;
        private readonly IBranchService _branchService;
        private readonly IReportService _reportService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditController(
            IAuditService auditService,
            IAuditTemplateService templateService,
            IBranchService branchService,
            IReportService reportService,
            UserManager<ApplicationUser> userManager)
        {
            _auditService = auditService;
            _templateService = templateService;
            _branchService = branchService;
            _reportService = reportService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Forbid();

            ViewData["Title"] = "Audits";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Unauthorized(new { success = false, message = "Unable to resolve company context." });

            var audits = await _auditService.GetAuditsByCompanyAsync(companyId.Value);
            return Json(new { success = true, data = audits });
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Unauthorized(new { success = false, message = "Unable to resolve company context." });

            var audit = await _auditService.GetAuditByIdAsync(id, companyId.Value);
            if (audit == null) return NotFound(new { success = false, message = "Audit not found." });

            return Json(new { success = true, data = audit });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Forbid();

            var templates = await _templateService.GetTemplatesByCompanyIdAsync(companyId.Value, includePublishedOnly: true);
            var branches = await _branchService.GetAllAsync(companyId.Value, isActive: true, search: null);
            var auditors = await _auditService.GetAuditorsForCompanyAsync(companyId.Value);

            var model = new CreateAuditViewModel
            {
                Templates = new SelectList(templates.Select(t => new { Value = t.AuditTemplateId, Text = t.Title }), "Value", "Text"),
                Branches = new SelectList(branches.Select(b => new { Value = b.BranchId, Text = b.BranchName }), "Value", "Text"),
                Auditors = new SelectList(auditors.Select(a => new { Value = a.Id, Text = a.FullName ?? a.Name, Email = a.Email }), "Value", "Text"),
                ScheduledStartDate = DateTimeOffset.Now.AddDays(1)
            };

            ViewData["Title"] = "Create Audit";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAuditViewModel model)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Unauthorized(new { success = false, message = "Unable to resolve company context." });

            if (!ModelState.IsValid)
            {
                var templates = await _templateService.GetTemplatesByCompanyIdAsync(companyId.Value, includePublishedOnly: true);
                var branches = await _branchService.GetAllAsync(companyId.Value, isActive: true, search: null);
                var auditors = await _auditService.GetAuditorsForCompanyAsync(companyId.Value);

                model.Templates = new SelectList(templates.Select(t => new { Value = t.AuditTemplateId, Text = t.Title }), "Value", "Text");
                model.Branches = new SelectList(branches.Select(b => new { Value = b.BranchId, Text = b.BranchName }), "Value", "Text");
                model.Auditors = new SelectList(auditors.Select(a => new { Value = a.Id, Text = a.FullName ?? a.Name, Email = a.Email }), "Value", "Text");

                return View(model);
            }

            var userId = (await _userManager.GetUserAsync(User))?.Id ?? 0;
            var auditId = await _auditService.CreateAuditAsync(model, companyId.Value, userId);

            if (auditId == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to create audit. Please check the template is published.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Forbid();

            var audit = await _auditService.GetAuditByIdAsync(id, companyId.Value);
            if (audit == null) return NotFound();

            var templates = await _templateService.GetTemplatesByCompanyIdAsync(companyId.Value, includePublishedOnly: true);
            var branches = await _branchService.GetAllAsync(companyId.Value, isActive: true, search: null);
            var auditors = await _auditService.GetAuditorsForCompanyAsync(companyId.Value);

            var model = new CreateAuditViewModel
            {
                AuditTemplateId = audit.AuditTemplateId,
                Title = audit.Title,
                BranchId = audit.BranchId,
                ScheduledStartDate = audit.ScheduledStartDate ?? DateTimeOffset.Now,
                ScheduledEndDate = audit.ScheduledEndDate,
                AssignedToUserId = audit.AssignedToUserId,
                Notes = audit.Notes,
                Templates = new SelectList(templates.Select(t => new { Value = t.AuditTemplateId, Text = t.Title }), "Value", "Text"),
                Branches = new SelectList(branches.Select(b => new { Value = b.BranchId, Text = b.BranchName }), "Value", "Text"),
                Auditors = new SelectList(auditors.Select(a => new { Value = a.Id, Text = a.FullName ?? a.Name, Email = a.Email }), "Value", "Text")
            };

            ViewData["Title"] = "Edit Audit";
            ViewBag.AuditId = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateAuditViewModel model)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Unauthorized(new { success = false, message = "Unable to resolve company context." });

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _auditService.UpdateAuditAsync(model, id, companyId.Value);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to update audit.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Unauthorized(new { success = false, message = "Unable to resolve company context." });

            var result = await _auditService.DeleteAuditAsync(id, companyId.Value);
            return Json(new { success = result, message = result ? "Audit deleted successfully." : "Failed to delete audit." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Unauthorized(new { success = false, message = "Unable to resolve company context." });

            if (!Enum.TryParse<Models.AuditModule.AuditEnums.AuditStatus>(status, true, out var auditStatus))
            {
                return Json(new { success = false, message = "Invalid status value." });
            }

            var result = await _auditService.UpdateAuditStatusAsync(id, auditStatus, companyId.Value);
            return Json(new { success = result, message = result ? "Status updated." : "Failed to update status." });
        }

        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Forbid();

            var audit = await _auditService.GetAuditForReviewAsync(id, companyId.Value);
            if (audit == null) return NotFound();

            ViewData["Title"] = "Review Audit";
            return View(audit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? reviewNotes)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Unauthorized(new { success = false, message = "Unable to resolve company context." });

            var userId = (await _userManager.GetUserAsync(User))?.Id ?? 0;
            var result = await _auditService.ApproveAuditAsync(id, companyId.Value, userId, reviewNotes);
            return Json(new { success = result, message = result ? "Audit approved successfully." : "Failed to approve audit." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reviewNotes)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Unauthorized(new { success = false, message = "Unable to resolve company context." });

            var userId = (await _userManager.GetUserAsync(User))?.Id ?? 0;
            var result = await _auditService.RejectAuditAsync(id, companyId.Value, userId, reviewNotes);
            return Json(new { success = result, message = result ? "Audit sent back for correction." : "Failed to reject audit." });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReport(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Forbid();

            try
            {
                var pdfBytes = await _reportService.GenerateAuditReportAsync(id, companyId.Value);
                return File(pdfBytes, "application/pdf", $"AuditReport_{id}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadInventoryReport(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null) return Forbid();

            try
            {
                var pdfBytes = await _reportService.GenerateInventoryReportAsync(id, companyId.Value);
                return File(pdfBytes, "application/pdf", $"InventoryReport_{id}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<int?> GetCurrentCompanyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.IsDeleted || !user.IsActive) return null;
            return user.CompanyId;
        }
    }
}
