using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;
using System.Threading.Tasks;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize]
    public class EmployeeDocumentTypeController : Controller
    {
        private readonly IEmployeeDocumentTypeService _employeeDocumentTypeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeDocumentTypeController(
            IEmployeeDocumentTypeService employeeDocumentTypeService,
            UserManager<ApplicationUser> userManager)
        {
            _employeeDocumentTypeService = employeeDocumentTypeService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Forbid();
            }

            ViewData["Title"] = "Employee Document Types";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null, string? sortColumn = null, string? sortOrder = null)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var (items, totalCount) = await _employeeDocumentTypeService.GetPagedListAsync(
                companyId.Value,
                pageNumber,
                pageSize,
                searchTerm,
                isActive,
                sortColumn,
                sortOrder);

            var itemList = items.Select(x => new
            {
                x.EmployeeDocumentTypeId,
                x.Name,
                x.Description,
                x.IsRequired,
                x.IsActive,
                Status = x.IsActive ? "Active" : "Inactive",
                x.CreatedAt
            });

            return Json(new
            {
                success = true,
                data = itemList,
                total = totalCount,
                page = pageNumber,
                pageSize = pageSize
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var item = await _employeeDocumentTypeService.GetByIdAsync(id, companyId.Value);
            if (item == null)
            {
                return NotFound(new { success = false, message = "Employee document type not found." });
            }

            return Json(new { success = true, data = item });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] EmployeeDocumentTypeViewModel model)
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

            // Check for duplicate name
            bool nameExists = await _employeeDocumentTypeService.IsNameExistsAsync(model.Name, companyId.Value);
            if (nameExists)
            {
                ModelState.AddModelError("Name", "An employee document type with this name already exists.");
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

            var entity = new EmployeeDocumentType
            {
                CompanyId = companyId.Value,
                Name = model.Name,
                Description = model.Description,
                IsRequired = model.IsRequired,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _employeeDocumentTypeService.CreateAsync(entity);

            return Json(new
            {
                success = true,
                message = "Employee document type created successfully.",
                data = created
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] EmployeeDocumentTypeViewModel model)
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

            // Check for duplicate name (excluding current entity)
            bool nameExists = await _employeeDocumentTypeService.IsNameExistsAsync(model.Name, companyId.Value, id);
            if (nameExists)
            {
                ModelState.AddModelError("Name", "An employee document type with this name already exists.");
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

            var entity = new EmployeeDocumentType
            {
                EmployeeDocumentTypeId = id,
                CompanyId = companyId.Value,
                Name = model.Name,
                Description = model.Description,
                IsRequired = model.IsRequired,
                IsActive = model.IsActive
            };

            var updated = await _employeeDocumentTypeService.UpdateAsync(id, entity, companyId.Value);
            if (updated == null)
            {
                return NotFound(new { success = false, message = "Employee document type not found." });
            }

            return Json(new
            {
                success = true,
                message = "Employee document type updated successfully.",
                data = updated
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

            var result = await _employeeDocumentTypeService.DeleteAsync(id, companyId.Value);
            if (!result)
            {
                return NotFound(new { success = false, message = "Employee document type not found." });
            }

            return Json(new { success = true, message = "Employee document type deleted successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var result = await _employeeDocumentTypeService.ToggleActiveStatusAsync(id, companyId.Value);
            if (!result)
            {
                return NotFound(new { success = false, message = "Employee document type not found." });
            }

            return Json(new { success = true, message = "Employee document type status updated successfully." });
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