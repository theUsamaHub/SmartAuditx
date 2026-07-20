namespace SmartAuditX.Models.AuditModule.AuditEnums
{
    public enum TemplateItemType
    {
        Boolean = 1,     // Yes/No (Pass/Fail)
        Text = 2,        // Free text input
        Number = 3,      // Numerical value (comparisons)
        Photo = 4,       // Image attachment required
        Barcode = 5,     // Scanner validation required
        Selection = 6,   // Single-choice select list (Dropdown)
        Rating = 7,      // Star/scale rating (1-5 or 1-10)
        Date = 8,        // Date picker
        Signature = 9    // Signature pad
    }
}
