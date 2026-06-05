using Microsoft.AspNetCore.Mvc;

namespace SmartAuditX.Controllers.mvccontrollers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "SmartAuditX - Enterprise Audit & Compliance Platform";
            return View();
        }
    }
}
