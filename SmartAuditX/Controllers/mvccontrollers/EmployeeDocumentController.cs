using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize]
    public class EmployeeDocumentController : Controller
    {
        private readonly IEmployeeDocumentService _documentService;
        private readonly IEmployeeDocumentTypeService _documentTypeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeDocumentController(
            IEmployeeDocumentService documentService,
            IEmployeeDocumentTypeService documentTypeService,
            UserManager<ApplicationUser> userManager)
        {
            _documentService = documentService;
            _documentTypeService = documentTypeService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetDocuments(int employeeId)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var documents = await _documentService.GetByEmployeeAsync(employeeId, companyId.Value);
            var docList = documents.Select(d => new
            {
                d.EmployeeDocumentId,
                d.EmployeeDocumentTypeId,
                DocumentTypeName = d.DocumentTypeNameSnapshot ?? d.EmployeeDocumentType?.Name ?? "Unknown",
                d.FileUrl,
                d.FileName,
                d.FileType,
                d.IsVerified,
                d.UploadedAt
            }).ToList();

            return Json(new { success = true, data = docList });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int employeeId, [FromForm] EmployeeDocumentViewModel model)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState
                        .Where(entry => entry.Value?.Errors.Count > 0)
                        .ToDictionary(
                            entry => entry.Key,
                            entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray())
                });
            }

            var result = await _documentService.UploadAsync(employeeId, companyId.Value, model);
            return Json(new
            {
                success = result.success,
                message = result.message,
                data = result.document != null ? new
                {
                    result.document.EmployeeDocumentId,
                    result.document.EmployeeDocumentTypeId,
                    DocumentTypeName = result.document.DocumentTypeNameSnapshot,
                    result.document.FileUrl,
                    result.document.FileName,
                    result.document.FileType,
                    result.document.IsVerified,
                    result.document.UploadedAt
                } : null
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var result = await _documentService.DeleteAsync(id, companyId.Value);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVerified(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var result = await _documentService.ToggleVerifiedAsync(id, companyId.Value);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        public async Task<IActionResult> GetDocumentTypes()
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var types = await _documentTypeService.GetAllAsync(companyId.Value);
            var typeList = types.Where(t => t.IsActive && !t.IsDeleted).Select(t => new
            {
                t.EmployeeDocumentTypeId,
                t.Name,
                t.IsRequired
            }).ToList();

            return Json(new { success = true, data = typeList });
        }

        private async Task<int?> GetCurrentCompanyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.IsDeleted || !user.IsActive)
            {
                return null;
            }

            return user.CompanyId;
        }
    }
}
