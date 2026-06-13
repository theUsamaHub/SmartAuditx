using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Controllers.Admin
{
    [Authorize(Roles = "SystemAdmin")]
    public class TeamMemberController : Controller
    {
        private readonly ITeamMemberService _teamMemberService;

        public TeamMemberController(ITeamMemberService teamMemberService)
        {
            _teamMemberService = teamMemberService;
        }

        public IActionResult Index()
        {
            return View("~/Views/Admin/TeamMember/Index.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var data = await _teamMemberService.GetAllAsync();
            return Json(new { data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] TeamMemberVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _teamMemberService.CreateAsync(model);
                if (result) return Json(new { success = true, message = "Team member created successfully." });
            }
            return Json(new { success = false, message = "Failed to create team member. Please check your inputs." });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _teamMemberService.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] TeamMemberVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _teamMemberService.UpdateAsync(model);
                if (result) return Json(new { success = true, message = "Team member updated successfully." });
            }
            return Json(new { success = false, message = "Failed to update team member. Please check your inputs." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _teamMemberService.DeleteAsync(id);
            if (result) return Json(new { success = true, message = "Team member deleted successfully." });
            return Json(new { success = false, message = "Failed to delete team member." });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _teamMemberService.ToggleStatusAsync(id);
            if (result) return Json(new { success = true, message = "Status updated successfully." });
            return Json(new { success = false, message = "Failed to update status." });
        }
    }
}
