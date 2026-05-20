using Microsoft.AspNetCore.Mvc;

namespace SmartAuditX.Controllers.mvccontrollers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
