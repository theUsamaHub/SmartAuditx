using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels
{
    public class RegisterAccountViewModel
    {
        // ─────────────────────────────────────────────
        // USERNAME
        // ─────────────────────────────────────────────

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(
            50,
            MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 50 characters.")]
        [RegularExpression(
            @"^[a-zA-Z0-9_.]+$",
            ErrorMessage =
                "Username can only contain letters, numbers, underscores, and dots.")]
        public string Username { get; set; } = string.Empty;

        // ─────────────────────────────────────────────
        // EMAIL
        // ─────────────────────────────────────────────

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(
            255,
            ErrorMessage = "Email cannot exceed 255 characters.")]
        public string Email { get; set; } = string.Empty;

        // ─────────────────────────────────────────────
        // PHONE NUMBER
        // ─────────────────────────────────────────────

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(
            20,
            MinimumLength = 7,
            ErrorMessage =
                "Phone number must be between 7 and 20 digits.")]
        [RegularExpression(
            @"^[0-9]+$",
            ErrorMessage =
                "Phone number can only contain numbers.")]
        public string PhoneNumber { get; set; } = string.Empty;

        // ─────────────────────────────────────────────
        // PHONE DIAL CODE
        // ─────────────────────────────────────────────

        [Required(ErrorMessage = "Dial code is required.")]
        [StringLength(
            5,
            ErrorMessage = "Dial code cannot exceed 5 characters.")]
        [RegularExpression(
            @"^\+\d{1,4}$",
            ErrorMessage =
                "Dial code format is invalid.")]
        public string PhoneDialCode { get; set; } = string.Empty;

        // ─────────────────────────────────────────────
        // PASSWORD
        // ─────────────────────────────────────────────

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage =
                "Password must be at least 8 characters.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage =
                "Password must contain uppercase, lowercase, number, and special character.")]
        public string Password { get; set; } = string.Empty;

        // ─────────────────────────────────────────────
        // CONFIRM PASSWORD
        // ─────────────────────────────────────────────

        [Required(ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage =
                "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}