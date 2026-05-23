using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models
{
    public class SubscriptionPlan : BaseEntity
    {
        /// <summary>
        /// Represents a top-level subscription plan tier (Basic, Professional, Enterprise)
        /// No pricing information is stored here to maintain historical accuracy
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionPlanId { get; set; }

        [Required(ErrorMessage = "Plan name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Plan name must be between 2 and 100 characters")]
        //[Display(Name = "Plan Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        //[Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        //[Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        //[Display(Name = "Is Deleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<SubscriptionPlanPricing> PricingOptions { get; set; } = new List<SubscriptionPlanPricing>();
        public virtual ICollection<SubscriptionPlanFeature> Features { get; set; } = new List<SubscriptionPlanFeature>();
        public virtual ICollection<CompanySubscription> CompanySubscriptions { get; set; } = new List<CompanySubscription>();
    }

}
