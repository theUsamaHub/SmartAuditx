using Microsoft.AspNetCore.Mvc;

namespace SmartAuditX.Controllers.mvccontrollers
{
    public class AuditorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
