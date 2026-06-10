// ─── TaxConfiguration.cs ─────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Country-specific tax rules for invoice generation.
    /// Pakistan: no software VAT currently.
    /// UAE: 5% VAT on digital services.
    /// India: 18% GST on SaaS.
    /// Applied at invoice generation time based on Company.CountryCode.
    /// Updating a tax rate = updating one row. No code deploy needed.
    /// </summary>
    [Table("TaxConfigurations")]
    public class TaxConfiguration : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaxConfigurationId { get; set; }

        /// <summary>ISO 3166-1 alpha-2. e.g. "PK", "AE", "IN", "US".</summary>
        [Required]
        [MaxLength(2)]
        public string CountryCode { get; set; } = string.Empty;

        /// <summary>e.g. "GST", "VAT", "Sales Tax".</summary>
        [Required]
        [MaxLength(50)]
        public string TaxName { get; set; } = string.Empty;

        /// <summary>
        /// Rate as decimal. 0.1800 = 18%, 0.0500 = 5%.
        /// decimal(5,4) supports rates up to 99.99%.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(5,4)")]
        public decimal TaxRate { get; set; }

        /// <summary>Tax applied on top of an already-taxed amount. Rare.</summary>
        public bool IsCompound { get; set; } = false;
    }
}