using Microsoft.AspNetCore.Mvc;

namespace SmartAuditX.Controllers.mvccontrollers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
