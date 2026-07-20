using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels.AuditModule;
using SmartAuditX.Services.Interfaces;
using System.Threading.Tasks;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize]
    public class AuditTemplateController : Controller
    {
        private readonly IAuditTemplateService _auditTemplateService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditTemplateController(IAuditTemplateService auditTemplateService, UserManager<ApplicationUser> userManager)
        {
            _auditTemplateService = auditTemplateService;
            _userManager = userManager;
        }

        // GET: AuditTemplate
        public async Task<IActionResult> Index()
        {
            // Get the current user's company ID from the logged-in user
            int companyId = await GetCurrentCompanyId();
            var templates = await _auditTemplateService.GetTemplatesByCompanyIdAsync(companyId);
            return View(templates);
        }

        // GET: AuditTemplate/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AuditTemplate/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuditTemplateViewModel model)
        {
            if (ModelState.IsValid)
            {
                int companyId = await GetCurrentCompanyId();
                int templateId = await _auditTemplateService.CreateTemplateAsync(model, companyId);
                return RedirectToAction(nameof(Builder), new { id = templateId });
            }
            return View(model);
        }

        // GET: AuditTemplate/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            int companyId = await GetCurrentCompanyId();
            var template = await _auditTemplateService.GetTemplateByIdAsync(id, companyId);
            if (template == null)
            {
                return NotFound();
            }
            return View(template);
        }

        // POST: AuditTemplate/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AuditTemplateViewModel model)
        {
            if (id != model.AuditTemplateId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                int companyId = await GetCurrentCompanyId();
                var result = await _auditTemplateService.UpdateTemplateAsync(model, companyId);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Handle error (template not found or other issue)
                    return NotFound();
                }
            }
            return View(model);
        }

        // GET: AuditTemplate/Builder/5
        public async Task<IActionResult> Builder(int id)
        {
            int companyId = await GetCurrentCompanyId();
            var template = await _auditTemplateService.GetTemplateByIdAsync(id, companyId);
            if (template == null)
            {
                return NotFound();
            }
            return View(template);
        }

        // POST: AuditTemplate/Publish/5
        [HttpPost]
        public async Task<IActionResult> Publish(int id)
        {
            int companyId = await GetCurrentCompanyId();
            var result = await _auditTemplateService.PublishTemplateAsync(id, companyId);
            if (result)
            {
                return Json(new { success = true, message = "Template published successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to publish template." });
            }
        }

        // POST: AuditTemplate/Unpublish/5
        [HttpPost]
        public async Task<IActionResult> Unpublish(int id)
        {
            int companyId = await GetCurrentCompanyId();
            var result = await _auditTemplateService.UnpublishTemplateAsync(id, companyId);
            if (result)
            {
                return Json(new { success = true, message = "Template unpublished successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to unpublish template." });
            }
        }

        // POST: AuditTemplate/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            int companyId = await GetCurrentCompanyId();
            var result = await _auditTemplateService.DeleteTemplateAsync(id, companyId);
            if (result)
            {
                return Json(new { success = true, message = "Template deleted successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete template." });
            }
        }

        // AJAX Handlers for Sections
        [HttpPost]
        public async Task<IActionResult> AddSection(int templateId, [FromBody] AuditTemplateSectionViewModel model)
        {
            int companyId = await GetCurrentCompanyId();
            var section = await _auditTemplateService.AddSectionAsync(templateId, model, companyId);
            if (section != null)
            {
                return Json(new { success = true, data = section });
            }
            else
            {
                return Json(new { success = false, message = "Failed to add section." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSection(int templateId, int sectionId, [FromBody] AuditTemplateSectionViewModel model)
        {
            int companyId = await GetCurrentCompanyId();
            var result = await _auditTemplateService.UpdateSectionAsync(templateId, sectionId, model, companyId);
            if (result)
            {
                return Json(new { success = true, message = "Section updated successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to update section." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSection(int templateId, int sectionId)
        {
            int companyId = await GetCurrentCompanyId();
            var result = await _auditTemplateService.DeleteSectionAsync(templateId, sectionId, companyId);
            if (result)
            {
                return Json(new { success = true, message = "Section deleted successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete section." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReorderSections(int templateId, [FromBody] List<int> sectionIdsInOrder)
        {
            int companyId = await GetCurrentCompanyId();
            var result = await _auditTemplateService.ReorderSectionsAsync(templateId, sectionIdsInOrder, companyId);
            if (result)
            {
                return Json(new { success = true, message = "Sections reordered successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to reorder sections." });
            }
        }

        // AJAX Handlers for Fields
        [HttpPost]
        public async Task<IActionResult> AddField(int templateId, int sectionId, [FromBody] AuditTemplateFieldViewModel model)
        {
            int companyId = await GetCurrentCompanyId();
            var field = await _auditTemplateService.AddFieldAsync(templateId, sectionId, model, companyId);
            if (field != null)
            {
                return Json(new { success = true, data = field });
            }
            else
            {
                return Json(new { success = false, message = "Failed to add field." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateField(int templateId, int sectionId, int fieldId, [FromBody] AuditTemplateFieldViewModel model)
        {
            int companyId = await GetCurrentCompanyId();
            var result = await _auditTemplateService.UpdateFieldAsync(templateId, sectionId, fieldId, model, companyId);
            if (result)
            {
                return Json(new { success = true, message = "Field updated successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to update field." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteField(int templateId, int sectionId, int fieldId)
        {
            int companyId = await GetCurrentCompanyId();
            var result = await _auditTemplateService.DeleteFieldAsync(templateId, sectionId, fieldId, companyId);
            if (result)
            {
                return Json(new { success = true, message = "Field deleted successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete field." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReorderFields(int templateId, int sectionId, [FromBody] List<int> fieldIdsInOrder)
        {
            int companyId = await GetCurrentCompanyId();
            var result = await _auditTemplateService.ReorderFieldsAsync(templateId, sectionId, fieldIdsInOrder, companyId);
            if (result)
            {
                return Json(new { success = true, message = "Fields reordered successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to reorder fields." });
            }
        }

        // AJAX Handlers for Field Options
        [HttpPost]
        public async Task<IActionResult> AddOption(int templateId, int sectionId, int fieldId, [FromBody] AuditTemplateFieldOptionViewModel model)
        {
            int companyId = await GetCurrentCompanyId();
            var option = await _auditTemplateService.AddOptionAsync(templateId, sectionId, fieldId, model, companyId);
            if (option != null)
            {
                return Json(new { success = true, data = option });
            }
            else
            {
                return Json(new { success = false, message = "Failed to add option." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOption(int templateId, int sectionId, int fieldId, int optionId, [FromBody] AuditTemplateFieldOptionViewModel model)
        {
            int companyId = await GetCurrentCompanyId();
            var result = await _auditTemplateService.UpdateOptionAsync(templateId, sectionId, fieldId, optionId, model, companyId);
            if (result)
            {
                return Json(new { success = true, message = "Option updated successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to update option." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOption(int templateId, int sectionId, int fieldId, int optionId)
        {
            int companyId = await GetCurrentCompanyId();
            var result = await _auditTemplateService.DeleteOptionAsync(templateId, sectionId, fieldId, optionId, companyId);
            if (result)
            {
                return Json(new { success = true, message = "Option deleted successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete option." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReorderOptions(int templateId, int sectionId, int fieldId, [FromBody] List<int> optionIdsInOrder)
        {
            int companyId = await GetCurrentCompanyId();
            var result = await _auditTemplateService.ReorderOptionsAsync(templateId, sectionId, fieldId, optionIdsInOrder, companyId);
            if (result)
            {
                return Json(new { success = true, message = "Options reordered successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to reorder options." });
            }
        }

        // Helper method to get the current user's company ID
        private async Task<int> GetCurrentCompanyId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                return user.CompanyId;
            }
            
            // Fallback: try to get from claims if user object is null
            var companyIdClaim = User.FindFirst("CompanyId");
            if (companyIdClaim != null && int.TryParse(companyIdClaim.Value, out int companyId))
            {
                return companyId;
            }
            
            throw new UnauthorizedAccessException("Unable to determine user's company ID.");
        }
    }
}