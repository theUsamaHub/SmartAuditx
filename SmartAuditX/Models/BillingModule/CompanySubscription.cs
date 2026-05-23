using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models
{
    /// <summary>
    /// Tracks individual company subscriptions including status, dates, and auto-renewal
    /// Each subscription links to a specific pricing option to maintain historical accuracy
    /// </summary>
    [Table("CompanySubscriptions")]
    public class CompanySubscription : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanySubscriptionId { get; set; }

        [Required(ErrorMessage = "Company ID is required")]
        [Display(Name = "Company ID")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Subscription plan pricing ID is required")]
        [ForeignKey("SubscriptionPlanPricing")]
        [Display(Name = "Pricing Option")]
        public int SubscriptionPlanPricingId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Status must be between 4 and 50 characters")]
        [RegularExpression("^(Active|Expired|Cancelled|Pending|Suspended)$",
            ErrorMessage = "Status must be Active, Expired, Cancelled, Pending, or Suspended")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Expiry date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Expiry Date")]
        [CustomValidation(typeof(CompanySubscription), nameof(ValidateExpiryDate))]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [Display(Name = "Auto Renew")]
        public bool AutoRenew { get; set; } = true;

        // Computed properties
        [NotMapped]
        [Display(Name = "Days Remaining")]
        public int DaysRemaining => (ExpiryDate - DateTime.UtcNow).Days;

        [NotMapped]
        [Display(Name = "Is Expired")]
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;

        [NotMapped]
        [Display(Name = "Is Active Status")]
        public bool IsActiveStatus => Status?.Equals("Active", StringComparison.OrdinalIgnoreCase) ?? false;

        // Navigation properties
        public virtual SubscriptionPlanPricing? SubscriptionPlanPricing { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // Custom validation method
        public static ValidationResult? ValidateExpiryDate(DateTime expiryDate, ValidationContext context)
        {
            var subscription = (CompanySubscription)context.ObjectInstance;
            if (expiryDate <= subscription.StartDate)
            {
                return new ValidationResult("Expiry date must be after start date");
            }
            return ValidationResult.Success;
        }
    }
}
