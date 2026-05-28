using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Controllers.mvccontrollers
{
    public class RegistrationController : Controller
    {
        private readonly IRegistrationService _registrationService;
        private readonly ICountryService _countryService;

        public RegistrationController(
            IRegistrationService registrationService,
            ICountryService countryService)
        {
            _registrationService = registrationService;
            _countryService = countryService;
        }

        // ─────────────────────────────────────────────
        // STEP 1: ACCOUNT INFO
        // ─────────────────────────────────────────────

        [HttpGet]
        public IActionResult AccountInfo()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AccountInfo(RegisterAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            HttpContext.Session.SetString("Username", model.Username);
            HttpContext.Session.SetString("Email", model.Email);
            HttpContext.Session.SetString("PhoneNumber", model.PhoneNumber);
            HttpContext.Session.SetString("Password", model.Password);
            HttpContext.Session.SetString("PhoneDialCode", model.PhoneDialCode);

            return RedirectToAction(nameof(CompanyInfo));
        }

        // ─────────────────────────────────────────────
        // STEP 2: COMPANY INFO
        // ─────────────────────────────────────────────

        [HttpGet]
        public IActionResult CompanyInfo()
        {
            var model = new RegisterCompanyViewModel
            {
                Countries = _countryService.GetCountries()
                    .Select(x => new SelectListItem
                    {
                        Value = x.Code,
                        Text = x.Name
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompanyInfo(RegisterCompanyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Countries = _countryService.GetCountries()
                    .Select(x => new SelectListItem
                    {
                        Value = x.Code,
                        Text = x.Name
                    })
                    .ToList();

                return View(model);
            }

            var username = HttpContext.Session.GetString("Username");
            var email = HttpContext.Session.GetString("Email");
            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
            var password = HttpContext.Session.GetString("Password");
            var phoneDialCode = HttpContext.Session.GetString("PhoneDialCode");

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Session expired. Please complete registration again.";
                return RedirectToAction(nameof(AccountInfo));
            }

            var accountModel = new RegisterAccountViewModel
            {
                Username = username,
                Email = email,
                PhoneNumber = phoneNumber,
                Password = password,
                PhoneDialCode = phoneDialCode
            };

            var result =
                await _registrationService.RegisterCompanyOwnerAsync(
                    accountModel,
                    model);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Registration failed.");

                model.Countries = _countryService.GetCountries()
                    .Select(x => new SelectListItem
                    {
                        Value = x.Code,
                        Text = x.Name
                    })
                    .ToList();

                return View(model);
            }

            HttpContext.Session.Clear();

            return RedirectToAction(nameof(RegistrationSuccess));
        }

        // ─────────────────────────────────────────────

        public IActionResult RegistrationSuccess()
        {
            return View("RegisterationSuccess");
        }
    }
}