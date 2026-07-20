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
    /// <summary>
    /// Handles user login functionality including authentication with email, username, or phone number
    /// and role-based redirection after successful login
    /// </summary>
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        // Manages user sign-in operations (login, logout, etc.)
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Logging service for recording login attempts and errors
        private readonly ILogger<LoginModel> _logger;

        // Manages user account operations (finding users by email/username, checking roles, etc.)
        private readonly UserManager<ApplicationUser> _userManager; //added this for login because we need to use the email/username

        /// <summary>
        /// Initializes a new instance of the LoginModel with required services
        /// </summary>
        /// <param name="signInManager">Manages user sign-in operations</param>
        /// <param name="userManager">Manages user account operations</param>
        /// <param name="logger">Logging service for audit trails</param>
        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        } //updated the constructor to include the user manager for login

        // Binds the login form data to this property when the form is submitted
        [BindProperty]
        public InputModel Input { get; set; }

        // External login providers (commented out as not currently used)
        //public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // Stores the URL to redirect to after successful login
        public string ReturnUrl { get; set; }

        // Temporary storage for error messages between requests
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Represents the data model for the login form input
        /// </summary>
        public class InputModel
        {

            //[Required]
            //[EmailAddress]
            //public string Email { get; set; }

            /// <summary>
            /// The user's login identifier - can be email address, username, 
            /// </summary>
            [Required]
            public string LoginIdentifier { get; set; } = string.Empty;

            //[Required]
            //public string Identifier { get; set; } //writ now we will open it later if we want to use the identifier for login instead of email

            /// <summary>
            /// The user's password
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            /// Whether to persist the login session across browser closures
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        /// <summary>
        /// Handles GET requests for the login page
        /// Clears any previous error messages and prepares the return URL
        /// </summary>
        /// <param name="returnUrl">Optional URL to redirect to after successful login</param>
        public async Task OnGetAsync(string returnUrl = null)
        {
            // Display any error message from previous login attempts
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Set default return URL to home page if none provided
            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            //await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Get available external authentication schemes (commented as not using external login)
            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        /// <summary>
        /// Handles POST requests when the user submits the login form
        /// Authenticates the user using email, username, or phone number
        /// Checks account status (active/non-deleted) before allowing login
        /// Redirects to role-specific dashboards after successful authentication
        /// </summary>
        /// <param name="returnUrl">Optional URL to redirect to after successful login</param>
        /// <returns>Redirect to appropriate dashboard or returns to login page with error</returns>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Set default return URL to home page if none provided
            returnUrl ??= Url.Content("~/");

            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(); //commented this because we are not using the external login 

            // Original password-based authentication code using LoginIdentifier directly - commented for enhancement
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
            //    //} //commented this because we are not using the lockout and 2fa for now
            //    else
            //    {
            //        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            //        return Page();
            //    }
            //}

            // Enhanced login logic that supports email, username, or phone number
            ApplicationUser? user = null;

            // Trim whitespace from login input
            var loginIdentifier = Input.LoginIdentifier.Trim();

            // STEP 1: Determine the login type based on input format
            // If input contains '@', treat as email address
            if (loginIdentifier.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(loginIdentifier);
            }
            // Phone number check - commented out but preserved for future implementation
            //else if (loginIdentifier.All(char.IsDigit))
            //{
            //    user = await _userManager.Users
            //        .FirstOrDefaultAsync(x =>
            //            x.PhoneNumber == loginIdentifier);
            //}
            else
            {
                // Otherwise, treat as username
                user = await _userManager.FindByNameAsync(loginIdentifier);
            }

            // STEP 2: Validate that user exists
            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid login attempt.");

                return Page();
            }



            // STEP 3: INACTIVE CHECK (MUST BE HERE)
            // Verify account is active and not deleted before allowing login
            if (!user.IsActive || user.IsDeleted)
            {
                ModelState.AddModelError("", "Account is inactive.");
                return Page();
            }

            if (!user.EmailConfirmed)
            {

                return RedirectToAction(
                    "EmailVerificationRequired",
                    "Registration",
                       new
                       {
                           userId = user.Id
                       });

            }

            // STEP 4: Attempt password authentication
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,     // Use the found user's username for authentication
                Input.Password,     // The provided password
                Input.RememberMe,   // Whether to persist the login session
                lockoutOnFailure: true); // Lockout after max failed attempts (5 attempts, 15min)

            // STEP 5: Handle lockout
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                ModelState.AddModelError(string.Empty, "Account locked out due to too many failed attempts. Please try again in 15 minutes.");
                return Page();
            }

            // STEP 6: Handle two-factor authentication required
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }

            // STEP 7: Handle successful authentication
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                // Get user's roles for role-based redirection
                var roles = await _userManager.GetRolesAsync(user);

                // Role-based dashboard redirection
                if (roles.Contains("SystemAdmin"))
                    return LocalRedirect("/Admin/Index");

                if (roles.Contains("CompanyOwner"))
                    return LocalRedirect("/Company/Index");

                if (roles.Contains("Manager"))
                    return LocalRedirect("/Manager/Index");

                if (roles.Contains("Auditor"))
                    return LocalRedirect("/Auditor/Index");

                if (roles.Contains("Employee"))
                    return LocalRedirect("/Employee/Index");

                if (roles.Contains("User"))
                    return LocalRedirect("/User/Index");

                // Fallback to default return URL if no specific role match
                return LocalRedirect(returnUrl);
            }

            // STEP 8: Authentication failed - display generic error message
            ModelState.AddModelError(
                string.Empty,
                "Invalid login attempt.");

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public async Task<IActionResult> OnPostAjaxAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var loginIdentifier = Input.LoginIdentifier?.Trim() ?? "";

            ApplicationUser? user = null;

            if (loginIdentifier.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(loginIdentifier);
            }
            else
            {
                user = await _userManager.FindByNameAsync(loginIdentifier);
            }

            if (user == null || !user.IsActive || user.IsDeleted)
            {
                return new JsonResult(new { success = false, message = "Invalid login attempt or account is inactive." });
            }

            if (!user.EmailConfirmed)
            {
                return new JsonResult(new { success = false, requiresVerification = true, userId = user.Id });
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return new JsonResult(new { success = false, message = "Account locked out due to too many failed attempts. Please try again in 15 minutes." });
            }

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in via AJAX.");
                var roles = await _userManager.GetRolesAsync(user);
                
                string targetUrl = returnUrl;
                if (roles.Contains("SystemAdmin")) targetUrl = "/Admin/Index";
                else if (roles.Contains("CompanyOwner")) targetUrl = "/Company/Index";
                else if (roles.Contains("Manager")) targetUrl = "/Manager/Index";
                else if (roles.Contains("Auditor")) targetUrl = "/Auditor/Index";
                else if (roles.Contains("Employee")) targetUrl = "/Employee/Index";
                else if (roles.Contains("User")) targetUrl = "/User/Index";

                return new JsonResult(new { success = true, redirectUrl = targetUrl });
            }

            return new JsonResult(new { success = false, message = "Invalid login attempt." });
        }
    }
}