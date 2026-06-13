using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Controllers.Admin
{
    [Authorize(Roles = "SystemAdmin")]
    public class FeatureController : Controller
    {
        private readonly IFeatureService _featureService;

        public FeatureController(IFeatureService featureService)
        {
            _featureService = featureService;
        }

        public IActionResult Index()
        {
            return View("~/Views/Admin/Feature/Index.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var data = await _featureService.GetAllAsync();
            return Json(new { data });
        }

        [HttpPost]
        public async Task<IActionResult> Create(FeatureVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _featureService.CreateAsync(model);
                if (result) return Json(new { success = true, message = "Feature created successfully." });
            }
            return Json(new { success = false, message = "Failed to create feature. Please check your inputs." });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _featureService.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FeatureVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _featureService.UpdateAsync(model);
                if (result) return Json(new { success = true, message = "Feature updated successfully." });
            }
            return Json(new { success = false, message = "Failed to update feature. Please check your inputs." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _featureService.DeleteAsync(id);
            if (result) return Json(new { success = true, message = "Feature deleted successfully." });
            return Json(new { success = false, message = "Failed to delete feature." });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _featureService.ToggleStatusAsync(id);
            if (result) return Json(new { success = true, message = "Status updated successfully." });
            return Json(new { success = false, message = "Failed to update status." });
        }
    }
}
