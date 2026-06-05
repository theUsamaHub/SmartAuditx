using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize]
    public class BranchController : Controller
    {
        private readonly IBranchService _branchService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BranchController(
            IBranchService branchService,
            UserManager<ApplicationUser> userManager)
        {
            _branchService = branchService;
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

            ViewData["Title"] = "Branches";
            return View(new BranchViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> List(bool? isActive, string? search)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var branches = await _branchService.GetAllAsync(
                companyId.Value,
                isActive,
                search);

            return Json(new { success = true, data = branches });
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var branch = await _branchService.GetForEditAsync(companyId.Value, id);
            if (branch == null)
            {
                return NotFound(new { success = false, message = "Branch not found." });
            }

            return Json(new { success = true, data = branch });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] BranchViewModel model)
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

            var result = await _branchService.CreateAsync(companyId.Value, model);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Branch
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] BranchViewModel model)
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

            var result = await _branchService.UpdateAsync(companyId.Value, id, model);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Branch
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

            var result = await _branchService.DeleteAsync(companyId.Value, id);
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

            var result = await _branchService.ToggleActiveAsync(companyId.Value, id);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Branch
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
