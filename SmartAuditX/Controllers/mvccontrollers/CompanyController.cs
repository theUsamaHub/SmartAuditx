using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Models;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly ICompanyDashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyController(
            ICompanyDashboardService dashboardService,
            UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            ViewData["Title"] = "Company Dashboard";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "Unable to resolve user." });

            var stats = await _dashboardService.GetStatsAsync(user.CompanyId);
            return Json(new { success = true, data = stats });
        }
    }
}
