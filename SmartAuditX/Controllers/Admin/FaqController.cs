using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.Models.CMS.ViewModels.CMS;

namespace SmartAuditX.Controllers.Admin
{
    [Authorize(Roles = "SystemAdmin")]
    public class FaqController : Controller
    {
        private readonly IFaqService _faqService;

        public FaqController(IFaqService faqService)
        {
            _faqService = faqService;
        }

        public IActionResult Index()
        {
            return View("~/Views/Admin/Faq/Index.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var data = await _faqService.GetAllAsync();
            return Json(new { data });
        }

        [HttpPost]
        public async Task<IActionResult> Create(FaqVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _faqService.CreateAsync(model);
                if (result) return Json(new { success = true, message = "FAQ created successfully." });
            }
            return Json(new { success = false, message = "Failed to create FAQ. Please check your inputs." });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _faqService.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FaqVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _faqService.UpdateAsync(model);
                if (result) return Json(new { success = true, message = "FAQ updated successfully." });
            }
            return Json(new { success = false, message = "Failed to update FAQ. Please check your inputs." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _faqService.DeleteAsync(id);
            if (result) return Json(new { success = true, message = "FAQ deleted successfully." });
            return Json(new { success = false, message = "Failed to delete FAQ." });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _faqService.ToggleStatusAsync(id);
            if (result) return Json(new { success = true, message = "Status updated successfully." });
            return Json(new { success = false, message = "Failed to update status." });
        }
    }
}
