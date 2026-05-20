using Microsoft.AspNetCore.Mvc;

namespace SmartAuditX.Controllers.mvccontrollers
{
    public class CompanyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
