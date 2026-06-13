using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Controllers.Admin
{
    [Authorize(Roles = "SystemAdmin")]
    public class AboutUsController : Controller
    {
        private readonly IAboutUsService _aboutUsService;

        public AboutUsController(IAboutUsService aboutUsService)
        {
            _aboutUsService = aboutUsService;
        }

        public IActionResult Index()
        {
            return View("~/Views/Admin/AboutUs/Index.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var data = await _aboutUsService.GetAllAsync();
            return Json(new { data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] AboutUsVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _aboutUsService.CreateAsync(model);
                if (result) return Json(new { success = true, message = "About Us record created successfully." });
            }
            return Json(new { success = false, message = "Failed to create record. Please check your inputs." });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _aboutUsService.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] AboutUsVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _aboutUsService.UpdateAsync(model);
                if (result) return Json(new { success = true, message = "About Us record updated successfully." });
            }
            return Json(new { success = false, message = "Failed to update record. Please check your inputs." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _aboutUsService.DeleteAsync(id);
            if (result) return Json(new { success = true, message = "Record deleted successfully." });
            return Json(new { success = false, message = "Failed to delete record." });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _aboutUsService.ToggleStatusAsync(id);
            if (result) return Json(new { success = true, message = "Status updated successfully." });
            return Json(new { success = false, message = "Failed to update status." });
        }
    }
}
