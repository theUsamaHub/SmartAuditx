namespace SmartAuditX.Models.AuditModule.AuditEnums
{
    public enum BarcodeMappingType
    {
        Template = 1,    // Starts a new audit template
        Item = 2,        // Navigates directly to a specific checklist item
        Asset = 3,       // Identifies a physical asset linked to an item
        Location = 4     // Verifies the auditor is physically at a location
    }
}