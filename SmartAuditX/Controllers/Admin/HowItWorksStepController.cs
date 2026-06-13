using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Controllers.Admin
{
    [Authorize(Roles = "SystemAdmin")]
    public class HowItWorksStepController : Controller
    {
        private readonly IHowItWorksService _howItWorksService;

        public HowItWorksStepController(IHowItWorksService howItWorksService)
        {
            _howItWorksService = howItWorksService;
        }

        public IActionResult Index()
        {
            return View("~/Views/Admin/HowItWorksStep/Index.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var data = await _howItWorksService.GetAllAsync();
            return Json(new { data });
        }

        [HttpPost]
        public async Task<IActionResult> Create(HowItWorksStepVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _howItWorksService.CreateAsync(model);
                if (result) return Json(new { success = true, message = "Step created successfully." });
            }
            return Json(new { success = false, message = "Failed to create step. Please check your inputs." });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _howItWorksService.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(HowItWorksStepVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _howItWorksService.UpdateAsync(model);
                if (result) return Json(new { success = true, message = "Step updated successfully." });
            }
            return Json(new { success = false, message = "Failed to update step." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _howItWorksService.DeleteAsync(id);
            if (result) return Json(new { success = true, message = "Step deleted successfully." });
            return Json(new { success = false, message = "Failed to delete step." });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _howItWorksService.ToggleStatusAsync(id);
            if (result) return Json(new { success = true, message = "Status updated successfully." });
            return Json(new { success = false, message = "Failed to update status." });
        }
    }
}
