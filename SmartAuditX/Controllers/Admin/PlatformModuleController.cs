using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.Models.CMS.ViewModels.CMS;

namespace SmartAuditX.Controllers.Admin
{
    [Authorize(Roles = "SystemAdmin")]
    public class PlatformModuleController : Controller
    {
        private readonly IPlatformModuleService _platformModuleService;

        public PlatformModuleController(IPlatformModuleService platformModuleService)
        {
            _platformModuleService = platformModuleService;
        }

        public IActionResult Index()
        {
            return View("~/Views/Admin/PlatformModule/Index.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var data = await _platformModuleService.GetAllAsync();
            return Json(new { data });
        }

        [HttpPost]
        public async Task<IActionResult> Create(PlatformModuleVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _platformModuleService.CreateAsync(model);
                if (result) return Json(new { success = true, message = "Module created successfully." });
            }
            return Json(new { success = false, message = "Failed to create module. Please check your inputs." });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _platformModuleService.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PlatformModuleVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _platformModuleService.UpdateAsync(model);
                if (result) return Json(new { success = true, message = "Module updated successfully." });
            }
            return Json(new { success = false, message = "Failed to update module." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _platformModuleService.DeleteAsync(id);
            if (result) return Json(new { success = true, message = "Module deleted successfully." });
            return Json(new { success = false, message = "Failed to delete module." });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _platformModuleService.ToggleStatusAsync(id);
            if (result) return Json(new { success = true, message = "Status updated successfully." });
            return Json(new { success = false, message = "Failed to update status." });
        }
    }
}
