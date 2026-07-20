using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels
{
    public class CreateSystemUserViewModel
    {
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [MaxLength(256)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "System Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone dial code is required.")]
        [MaxLength(5)]
        [Display(Name = "Phone Dial Code")]
        public string PhoneDialCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public string? Role { get; set; }
    }
}
