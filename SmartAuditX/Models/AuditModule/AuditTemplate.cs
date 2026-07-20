using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    public class AuditTemplate : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditTemplateId { get; set; } 

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPublished { get; set; } = false;

        public int Version { get; set; } = 1;

        public bool IsScoringEnabled { get; set; } = true;

        public DateTime ? DeletedAt { get; set; }

        // Multi-tenant Scoping
        [Required]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        // Navigation Properties
        public virtual ICollection<AuditTemplateItem> Items { get; set; } = new List<AuditTemplateItem>();
        public virtual ICollection<AuditTemplateSection> Sections { get; set; } = new List<AuditTemplateSection>();
        public virtual ICollection<Audit> Audits { get; set; } = new List<Audit>();
    }
}