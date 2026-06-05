using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize]
    public class BranchDepartmentController : Controller
    {
        private readonly IBranchDepartmentService _branchDepartmentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BranchDepartmentController(
            IBranchDepartmentService branchDepartmentService,
            UserManager<ApplicationUser> userManager)
        {
            _branchDepartmentService = branchDepartmentService;
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

            ViewData["Title"] = "Branch Departments";
            return View(new BranchDepartmentViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> List(int? branchId, int? departmentId)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var links = await _branchDepartmentService.GetAllAsync(
                companyId.Value,
                branchId,
                departmentId);

            return Json(new { success = true, data = links });
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var branches = await _branchDepartmentService.GetBranchesForDropdownAsync(companyId.Value);
            return Json(new { success = true, data = branches });
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var departments = await _branchDepartmentService.GetDepartmentsForDropdownAsync(companyId.Value);
            return Json(new { success = true, data = departments });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] BranchDepartmentViewModel model)
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

            var result = await _branchDepartmentService.CreateAsync(companyId.Value, model);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.BranchDepartment
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

            var result = await _branchDepartmentService.DeleteAsync(companyId.Value, id);
            return Json(new { success = result.Success, message = result.Message });
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
