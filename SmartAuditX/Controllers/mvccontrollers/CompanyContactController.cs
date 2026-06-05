using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Controllers.mvccontrollers
{

    [Authorize(Roles ="CompanyOwner")]
    public class CompanyContactController : Controller
    {
        private readonly ICompanyContactService _companyContactService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyContactController(
            ICompanyContactService companyContactService,
            UserManager<ApplicationUser> userManager)
        {
            _companyContactService = companyContactService;
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

            ViewData["Title"] = "Company Contacts";
            return View(new CompanyContactViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> List(ContactType? contactType, string? search)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var contacts = await _companyContactService.GetAllAsync(
                companyId.Value,
                contactType,
                search);

            return Json(new { success = true, data = contacts });
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var contact = await _companyContactService.GetForEditAsync(companyId.Value, id);
            if (contact == null)
            {
                return NotFound(new { success = false, message = "Contact not found." });
            }

            return Json(new { success = true, data = contact });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CompanyContactViewModel model)
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

            var result = await _companyContactService.CreateAsync(companyId.Value, model);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Contact
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] CompanyContactViewModel model)
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

            var result = await _companyContactService.UpdateAsync(companyId.Value, id, model);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Contact
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

            var result = await _companyContactService.DeleteAsync(companyId.Value, id);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimary(int id)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
            {
                return Unauthorized(new { success = false, message = "Unable to resolve company context." });
            }

            var result = await _companyContactService.SetPrimaryAsync(companyId.Value, id);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Contact
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
