using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize]
    public class DesignationController : Controller
    {
        private readonly IDesignationService _designationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DesignationController(
            IDesignationService designationService,
            UserManager<ApplicationUser> userManager)
        {
            _designationService = designationService;
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

            ViewData["Title"] = "Designations";
            return View(new DesignationViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> List(bool? isActive, string? search)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var designations = await _designationService.GetAllAsync(
                companyId.Value,
                isActive,
                search);

            return Json(new { success = true, data = designations });
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var designation = await _designationService.GetForEditAsync(companyId.Value, id);
            if (designation == null)
            {
                return NotFound(new { success = false, message = "Designation not found." });
            }

            return Json(new { success = true, data = designation });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] DesignationViewModel model)
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

            var result = await _designationService.CreateAsync(companyId.Value, model);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Designation
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] DesignationViewModel model)
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

            var result = await _designationService.UpdateAsync(companyId.Value, id, model);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Designation
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

            var result = await _designationService.DeleteAsync(companyId.Value, id);
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

            var result = await _designationService.ToggleActiveAsync(companyId.Value, id);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Designation
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
