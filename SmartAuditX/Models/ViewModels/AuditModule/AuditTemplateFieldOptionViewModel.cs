using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels.AuditModule
{
    public class AuditTemplateFieldOptionViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        // Optional value if the option has an underlying value (e.g., for scoring)
        public string? Value { get; set; }
    }
}