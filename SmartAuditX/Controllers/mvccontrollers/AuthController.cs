using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartAuditX.Controllers.mvccontrollers
{
    [Authorize]
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
