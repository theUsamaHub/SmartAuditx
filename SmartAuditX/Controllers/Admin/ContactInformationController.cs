using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Controllers.Admin
{
    [Authorize(Roles = "SystemAdmin")]
    public class ContactInformationController : Controller
    {
        private readonly IContactInformationService _contactInfoService;

        public ContactInformationController(IContactInformationService contactInfoService)
        {
            _contactInfoService = contactInfoService;
        }

        public IActionResult Index()
        {
            return View("~/Views/Admin/ContactInformation/Index.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var data = await _contactInfoService.GetAllAsync();
            return Json(new { data });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ContactInformationVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _contactInfoService.CreateAsync(model);
                if (result) return Json(new { success = true, message = "Contact information created successfully." });
            }
            return Json(new { success = false, message = "Failed to create contact information. Please check your inputs." });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _contactInfoService.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ContactInformationVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _contactInfoService.UpdateAsync(model);
                if (result) return Json(new { success = true, message = "Contact information updated successfully." });
            }
            return Json(new { success = false, message = "Failed to update contact information." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _contactInfoService.DeleteAsync(id);
            if (result) return Json(new { success = true, message = "Record deleted successfully." });
            return Json(new { success = false, message = "Failed to delete record." });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _contactInfoService.ToggleStatusAsync(id);
            if (result) return Json(new { success = true, message = "Status updated successfully." });
            return Json(new { success = false, message = "Failed to update status." });
        }
    }
}
