        using Microsoft.AspNetCore.Identity;
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.AspNetCore.Mvc.Rendering;
        using Microsoft.AspNetCore.WebUtilities;
        using SmartAuditX.Data;
        using SmartAuditX.Models;
        using SmartAuditX.Models.ViewModels;
        using SmartAuditX.Services.Interfaces;
        using System.Text;
        using Microsoft.EntityFrameworkCore;
        namespace SmartAuditX.Controllers.mvccontrollers
        {
        public class RegistrationController : Controller
        {
            private readonly IRegistrationService _registrationService;
            private readonly ICountryService _countryService;
            private readonly IEmailService _emailService;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly ApplicationDbContext _context;
            public RegistrationController(
            UserManager<ApplicationUser> userManager,
            IRegistrationService registrationService,
            ICountryService countryService,
            IEmailService emailService,
            ApplicationDbContext context
            )

            {
                _registrationService = registrationService;
                _countryService = countryService;
                _emailService = emailService;
                _userManager = userManager;
                _context = context;
            }

            [TempData]
            public string SuccessMessage { get; set; }

            [TempData]
            public string ErrorMessage { get; set; }

        private bool HasRegistrationSession()
        {
            return
                !string.IsNullOrWhiteSpace(
                    HttpContext.Session.GetString("Username")) &&

                !string.IsNullOrWhiteSpace(
                    HttpContext.Session.GetString("Email")) &&

                !string.IsNullOrWhiteSpace(
                    HttpContext.Session.GetString("PhoneNumber")) &&

                !string.IsNullOrWhiteSpace(
                    HttpContext.Session.GetString("Password"));
        }

        private async Task<string?> ValidateAccountInfoAsync(RegisterAccountViewModel model)
        {
            var emailExists =
                await _userManager.FindByEmailAsync(model.Email);

            if (emailExists != null)
            {
                return "Email already exists.";
            }

            var usernameExists =
                await _userManager.FindByNameAsync(model.Username);

            if (usernameExists != null)
            {
                return "Username already exists.";
            }

            var normalizedPhone =
                model.PhoneNumber
                    .Replace(" ", "")
                    .Replace("-", "")
                    .Trim();

            var phoneExists =
                await _userManager.Users.AnyAsync(x =>
                    x.PhoneNumber == normalizedPhone &&
                    x.PhoneDialCode == model.PhoneDialCode);

            if (phoneExists)
            {
                return "Phone number already exists.";
            }

            return null;
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
        public async Task<IActionResult> AccountInfo(
            RegisterAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var validationError =
                await ValidateAccountInfoAsync(model);

            if (validationError != null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    validationError);

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

            HttpContext.Session.SetString(
                "PhoneDialCode",
                model.PhoneDialCode);

            return RedirectToAction(nameof(CompanyInfo));
        }

        // ─────────────────────────────────────────────
        // STEP 2: COMPANY INFO
        // ─────────────────────────────────────────────

        [HttpGet]
        public IActionResult CompanyInfo()
        {
            if (!HasRegistrationSession())
            {
                return RedirectToAction(nameof(AccountInfo));
            }

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

                //its failing here in the registeration the result status is not getting the success solved

                if (!result.Success)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        result.ErrorMessage);

                    model.Countries = _countryService.GetCountries()
                        .Select(x => new SelectListItem
                        {
                            Value = x.Code,
                            Text = x.Name
                        })
                        .ToList();

                    return View(model);
                }


                var verificationUrl = Url.Action(
            action: "ConfirmEmail",
            controller: "Registration",
            values: new
            {
                userId = result.UserId,
                token = result.EncodedToken
            },
            protocol: Request.Scheme);
            
            
                await _emailService.SendEmailAsync(
        accountModel.Email,
        "Verify Your SmartAuditX Account",
        $@"
        <h2>Welcome to SmartAuditX</h2>

        <p>
            Thank you for registering.
        </p>

        <p>
            Please verify your email address by clicking below:
        </p>

        <p>
            <a href='{verificationUrl}'>
                Verify Email
            </a>
        </p>

        <p>
            If you did not create this account,
            ignore this email.
        </p>");
            TempData["RegisteredUserId"] = result.UserId;

            HttpContext.Session.Clear();

                return RedirectToAction(nameof(RegistrationSuccess));
            }


        // ─────────────────────────────────────────────

        public IActionResult RegistrationSuccess()
        {
            var userId =
                TempData["RegisteredUserId"]?.ToString();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("AccountInfo");
            }

            TempData.Keep("RegisteredUserId");

            return View(
                "RegisterationSuccess",
                new RegistrationSuccessViewModel
                {
                    UserId = userId
                });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationEmail(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                ErrorMessage = "Invalid request.";

                return RedirectToAction(
                    nameof(RegistrationSuccess));
            }

            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ErrorMessage = "User not found.";

                return RedirectToAction(
                    nameof(RegistrationSuccess));
            }

            if (user.EmailConfirmed)
            {
                SuccessMessage =
                    "Email already verified.";

                return RedirectToAction(
                    nameof(RegistrationSuccess));
            }

            var token =
                await _userManager
                    .GenerateEmailConfirmationTokenAsync(user);

            var encodedToken =
                WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes(token));

            var verificationUrl =
                Url.Action(
                    "ConfirmEmail",
                    "Registration",
                    new
                    {
                        userId = user.Id,
                        token = encodedToken
                    },
                    Request.Scheme);

            await _emailService.SendEmailAsync(
                user.Email!,
                "Verify Your SmartAuditX Account",
                $@"
        <h2>Email Verification</h2>

        <p>
            Please click below to verify your account.
        </p>

        <p>
            <a href='{verificationUrl}'>
                Verify Email
            </a>
        </p>");

            SuccessMessage =
                "Verification email sent successfully.";

            TempData["RegisteredUserId"] = user.Id;

            return RedirectToAction(
                nameof(RegistrationSuccess));
        }


        public IActionResult EmailVerificationRequired(
        string userId)
        {
            ViewBag.UserId = userId;

            return View();
        } 

        [HttpGet]
            public async Task<IActionResult> ConfirmEmail(
                string userId,
                string token)
            {
                if (string.IsNullOrWhiteSpace(userId) ||
                    string.IsNullOrWhiteSpace(token))
                {
                    return View("EmailConfirmationFailed");
                }

                var user =
                    await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return View("EmailConfirmationFailed");
                }

                var decodedToken =
                    Encoding.UTF8.GetString(
                        WebEncoders.Base64UrlDecode(token));
                
                var result =
                    await _userManager.ConfirmEmailAsync(
                        user,
                        decodedToken);

                if (!result.Succeeded)
                {
                    return View("EmailConfirmationFailed");
                }

                var company =
                    await _context.Companies
                        .FirstOrDefaultAsync(
                            x => x.CompanyId == user.CompanyId);

                if (company != null)
                {
                    company.OnboardingStatus =
                        OnboardingStatus.EmailVerified;

                    await _context.SaveChangesAsync();
                }

                return View("EmailConfirmationSuccess");
            }
        }
        }