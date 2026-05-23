using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;
using System.Globalization;

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
        [HttpGet]
        public IActionResult AccountInfo()
        {
            //This only shows the form.
            return View();
        }

        //this store data temporarily in session and then redirects to the next step of registration which is company info.
        [HttpPost]
        public IActionResult AccountInfo(RegisterAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            HttpContext.Session.SetString(
                "Username",
                model.Username);

            HttpContext.Session.SetString(
                "Email",
                model.Email);

            HttpContext.Session.SetString(
                "PhoneNumber",
                model.PhoneNumber);

            HttpContext.Session.SetString(
                "Password",
                model.Password);

            return RedirectToAction(nameof(CompanyInfo));
        }

        //Displays company form
        [HttpGet]
        public IActionResult CompanyInfo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CompanyInfo(RegisterCompanyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var accountModel =
                new RegisterAccountViewModel
                {
                    Username =
                        HttpContext.Session.GetString("Username"),

                    Email =
                        HttpContext.Session.GetString("Email"),

                    PhoneNumber =
                        HttpContext.Session.GetString("PhoneNumber"),

                    Password =
                        HttpContext.Session.GetString("Password")
                };

            var result =
                await _registrationService
                    .RegisterCompanyOwnerAsync(
                        accountModel,
                        model);

            if (!result)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Registration failed.");

                return View(model);
            }

            HttpContext.Session.Clear();

            return RedirectToAction(
                nameof(RegistrationSuccess));
        }

        
        public IActionResult RegistrationSuccess()
        {
            return View("RegisterationSuccess");
        }


//        RegistrationController

//GET  AccountInfo
//POST AccountInfo

//GET  CompanyInfo
//POST CompanyInfo

//GET  RegistrationSuccess
    }
}
