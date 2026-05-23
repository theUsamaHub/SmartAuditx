using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models
{
    /// <summary>
    /// Immutable payment ledger recording all payment attempts, successes, and failures
    /// Records are append-only and never deleted to maintain complete audit trail
    /// </summary>
    [Table("Payments")]
    public class Payment : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [Required(ErrorMessage = "Company subscription ID is required")]
        [ForeignKey("CompanySubscription")]
        [Display(Name = "Subscription")]
        public int CompanySubscriptionId { get; set; }

        [Required(ErrorMessage = "Transaction reference is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Transaction reference must be between 5 and 100 characters")]
        [Display(Name = "Transaction Reference")]
        public string TransactionReference { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999.99")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Currency is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO code")]
        [RegularExpression("^(USD|EUR|GBP|PKR|CAD|AUD)$",
            ErrorMessage = "Currency must be USD, EUR, GBP, PKR, CAD, or AUD")]
        [Display(Name = "Currency")]
        public string Currency { get; set; } = "USD";

        [Required(ErrorMessage = "Payment method is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Payment method must be between 3 and 50 characters")]
        [RegularExpression("^(Card|BankTransfer|Wallet|PayPal|Stripe)$",
            ErrorMessage = "Payment method must be Card, BankTransfer, Wallet, PayPal, or Stripe")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Display(Name = "Gateway Response")]
        [Column(TypeName = "text")]
        public string? GatewayResponse { get; set; }

        [Required(ErrorMessage = "Payment status is required")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Payment status must be between 4 and 50 characters")]
        [RegularExpression("^(Success|Failed|Pending|Refunded|Disputed)$",
            ErrorMessage = "Payment status must be Success, Failed, Pending, Refunded, or Disputed")]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Pending";

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Paid At")]
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        // Computed properties
        [NotMapped]
        [Display(Name = "Is Successful")]
        public bool IsSuccessful => PaymentStatus?.Equals("Success", StringComparison.OrdinalIgnoreCase) ?? false;

        [NotMapped]
        [Display(Name = "Is Refunded")]
        public bool IsRefunded => PaymentStatus?.Equals("Refunded", StringComparison.OrdinalIgnoreCase) ?? false;

        // Navigation properties
        public virtual CompanySubscription? CompanySubscription { get; set; }
    }
}
