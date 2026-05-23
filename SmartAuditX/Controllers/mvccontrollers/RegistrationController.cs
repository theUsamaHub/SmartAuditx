using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Controllers.mvccontrollers
{
    public class RegistrationController : Controller
    {

        private readonly IRegistrationService _registrationService;

        public RegistrationController(
            IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
