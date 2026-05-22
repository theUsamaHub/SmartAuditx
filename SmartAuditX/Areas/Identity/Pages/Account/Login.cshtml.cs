// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SmartAuditX.Models;
using Microsoft.EntityFrameworkCore; //added this
namespace SmartAuditX.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager; //added this for login becuase we need to use the email/username/phoneno

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        } //updated the constructor to include the user manager for login

        
        [BindProperty]
        public InputModel Input { get; set; }

      
        //public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

   
        [TempData]
        public string ErrorMessage { get; set; }

       
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            //[Required]
            //[EmailAddress]
            //public string Email { get; set; }
            [Required]
            public string LoginIdentifier { get; set; } = string.Empty;
            //[Required]
            //public string Identifier { get; set; } //writ now we will open it later if we want to use the identifier for login instead of email

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            //await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(); //commented this bcz we are not using the external login 

            //if (ModelState.IsValid)
            //{
            //    // This doesn't count login failures towards account lockout
            //    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            //    var result = await _signInManager.PasswordSignInAsync(Input.LoginIdentifier, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            //    if (result.Succeeded)
            //    {
            //        _logger.LogInformation("User logged in.");
            //        return LocalRedirect(returnUrl);
            //    }
            //    //if (result.RequiresTwoFactor)
            //    //{
            //    //    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            //    //}
            //    //if (result.IsLockedOut)
            //    //{
            //    //    _logger.LogWarning("User account locked out.");
            //    //    return RedirectToPage("./Lockout");
            //    //} //commented this bcz we are not using the lockout and 2fa for now
            //    else
            //    {
            //        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            //        return Page();
            //    }
            //}
            ApplicationUser? user = null;

            var loginIdentifier = Input.LoginIdentifier.Trim();
            if (loginIdentifier.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(loginIdentifier);
            }
            //else if (loginIdentifier.All(char.IsDigit))
            //{
            //    user = await _userManager.Users
            //        .FirstOrDefaultAsync(x =>
            //            x.PhoneNumber == loginIdentifier);
            //}
            else
            {
                user = await _userManager.FindByNameAsync(loginIdentifier);
            }


            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid login attempt.");

                return Page();
            }

            // STEP 3: INACTIVE CHECK (MUST BE HERE)
            if (!user.IsActive || user.IsDeleted)
            {
                ModelState.AddModelError("", "Account is inactive.");
                return Page();
            }
            var result =
    await _signInManager.PasswordSignInAsync(
        user.UserName!,
        Input.Password,
        Input.RememberMe,
        lockoutOnFailure: false);

            if (result.Succeeded)
            { 
                _logger.LogInformation("User logged in.");


                var roles = await _userManager.GetRolesAsync(user); //better way

                if (roles.Contains("SystemAdmin"))
                    return LocalRedirect("/Admin/Index");

                if (roles.Contains("CompanyOwner"))
                    return LocalRedirect("/Company/Index");

                if (roles.Contains("Manager"))
                    return LocalRedirect("/Manager/Index");

                if (roles.Contains("Auditor"))
                    return LocalRedirect("/Auditor/Index");

                if(roles.Contains("Employee"))
                    return LocalRedirect("/Employee/Index");
                if(roles.Contains("User"))
                    return LocalRedirect("/User/Index");

                return LocalRedirect(returnUrl);
            }

            ModelState.AddModelError(
                string.Empty,
                "Invalid login attempt.");


            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
