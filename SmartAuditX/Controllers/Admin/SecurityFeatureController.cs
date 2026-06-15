using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.Models.CMS.ViewModels.CMS;

namespace SmartAuditX.Controllers.Admin
{
    [Authorize(Roles = "SystemAdmin")]
    public class SecurityFeatureController : Controller
    {
        private readonly ISecurityFeatureService _securityFeatureService;

        public SecurityFeatureController(ISecurityFeatureService securityFeatureService)
        {
            _securityFeatureService = securityFeatureService;
        }

        public IActionResult Index()
        {
            return View("~/Views/Admin/SecurityFeature/Index.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var data = await _securityFeatureService.GetAllAsync();
            return Json(new { data });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SecurityFeatureVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _securityFeatureService.CreateAsync(model);
                if (result) return Json(new { success = true, message = "Security feature created successfully." });
            }
            return Json(new { success = false, message = "Failed to create security feature. Please check your inputs." });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _securityFeatureService.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SecurityFeatureVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _securityFeatureService.UpdateAsync(model);
                if (result) return Json(new { success = true, message = "Security feature updated successfully." });
            }
            return Json(new { success = false, message = "Failed to update security feature." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _securityFeatureService.DeleteAsync(id);
            if (result) return Json(new { success = true, message = "Security feature deleted successfully." });
            return Json(new { success = false, message = "Failed to delete security feature." });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _securityFeatureService.ToggleStatusAsync(id);
            if (result) return Json(new { success = true, message = "Status updated successfully." });
            return Json(new { success = false, message = "Failed to update status." });
        }
    }
}
