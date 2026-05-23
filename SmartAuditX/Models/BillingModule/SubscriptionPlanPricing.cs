using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models
{
    /// <summary>
    /// Represents pricing options for subscription plans including billing cycles and discounts
    /// Historical pricing is preserved by deactivating old rows and creating new ones
    /// </summary>
    [Table("SubscriptionPlanPricing")]
    public class SubscriptionPlanPricing : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionPlanPricingId { get; set; }

        [Required(ErrorMessage = "Subscription plan ID is required")]
        [ForeignKey("SubscriptionPlan")]
        public int SubscriptionPlanId { get; set; }

        [Required(ErrorMessage = "Billing cycle is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Billing cycle must be between 3 and 50 characters")]
        [RegularExpression("^(Monthly|Yearly|FourYear|Quarterly|BiAnnual)$",
            ErrorMessage = "Billing cycle must be Monthly, Yearly, FourYear, Quarterly, or BiAnnual")]
        [Display(Name = "Billing Cycle")]
        public string BillingCycle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 999999.99, ErrorMessage = "Price must be between 0 and 999,999.99")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Base Price")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
        [Display(Name = "Discount Percentage")]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Computed property for final price after discount
        [NotMapped]
        [Display(Name = "Final Price")]
        public decimal FinalPrice => Price - (Price * (DiscountPercentage / 100));

        // Navigation properties
        public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
        public virtual ICollection<CompanySubscription> CompanySubscriptions { get; set; } = new List<CompanySubscription>();
    }
}
