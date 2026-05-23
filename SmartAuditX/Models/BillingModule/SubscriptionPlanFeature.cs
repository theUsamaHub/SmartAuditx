using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models
{
    /// <summary>
    /// Represents feature flags and limits for each subscription plan
    /// Enables dynamic UI rendering without hardcoding feature lists
    /// </summary>
    [Table("SubscriptionPlanFeatures")]
    public class SubscriptionPlanFeature : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionPlanFeatureId { get; set; }

        [Required(ErrorMessage = "Subscription plan ID is required")]
        [ForeignKey("SubscriptionPlan")]
        public int SubscriptionPlanId { get; set; }

        [Required(ErrorMessage = "Feature name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Feature name must be between 2 and 100 characters")]
        //[Display(Name = "Feature Name")]
        public string FeatureName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Feature value is required")]
        [StringLength(255, ErrorMessage = "Feature value cannot exceed 255 characters")]
        //[Display(Name = "Feature Value")]
        public string FeatureValue { get; set; } = string.Empty;

        // Helper property to check if feature is enabled
        [NotMapped]
        public bool IsEnabled => FeatureValue?.Equals("Enabled", StringComparison.OrdinalIgnoreCase) ?? false;

        // Helper property to get numeric limit if applicable
        [NotMapped]
        public int? NumericLimit => int.TryParse(FeatureValue, out int result) ? result : (int?)null;

        // Navigation properties
        public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
    }
}
