using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentController(
            IDepartmentService departmentService,
            UserManager<ApplicationUser> userManager)
        {
            _departmentService = departmentService;
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

            ViewData["Title"] = "Departments";
            return View(new DepartmentViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> List(bool? isActive, string? search)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var departments = await _departmentService.GetAllAsync(
                companyId.Value,
                isActive,
                search);

            return Json(new { success = true, data = departments });
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var department = await _departmentService.GetForEditAsync(companyId.Value, id);
            if (department == null)
            {
                return NotFound(new { success = false, message = "Department not found." });
            }

            return Json(new { success = true, data = department });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] DepartmentViewModel model)
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

            var result = await _departmentService.CreateAsync(companyId.Value, model);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Department
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] DepartmentViewModel model)
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

            var result = await _departmentService.UpdateAsync(companyId.Value, id, model);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Department
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

            var result = await _departmentService.DeleteAsync(companyId.Value, id);
            return Json(new { success = result.Success, message = result.Message });
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

            var result = await _departmentService.ToggleActiveAsync(companyId.Value, id);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Department
            });
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
