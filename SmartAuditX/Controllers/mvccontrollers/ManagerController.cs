using Microsoft.AspNetCore.Mvc;

namespace SmartAuditX.Controllers.mvccontrollers
{
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
