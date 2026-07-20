using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Models;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize(Roles = "SystemAdmin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IAdminService adminService, UserManager<ApplicationUser> userManager)
        {
            _adminService = adminService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Admin Dashboard";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return Json(new { success = true, data = stats });
        }

        // User Management
        public IActionResult Users()
        {
            ViewData["Title"] = "User Management";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UserList(string? search, bool? isActive)
        {
            var users = await _adminService.GetAllUsersAsync(search, isActive);
            return Json(new { success = true, data = users });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserActive(int id)
        {
            var result = await _adminService.ToggleUserActiveAsync(id);
            return Json(new { success = result, message = result ? "User status updated." : "Failed to update user." });
        }

        // Company Management
        public IActionResult Companies()
        {
            ViewData["Title"] = "Company Management";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CompanyList(string? search, bool? isActive)
        {
            var companies = await _adminService.GetAllCompaniesAsync(search, isActive);
            return Json(new { success = true, data = companies });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCompanyActive(int id)
        {
            var result = await _adminService.ToggleCompanyActiveAsync(id);
            return Json(new { success = result, message = result ? "Company status updated." : "Failed to update company." });
        }
    }
}
