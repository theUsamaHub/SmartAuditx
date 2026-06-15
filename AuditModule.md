# Audit Module — Complete Design Document
### SmartAuditX / OpsPulse
---

## Table of Contents
1. [Module Overview](#1-module-overview)
2. [Core Concept](#2-core-concept)
3. [Three-Step Workflow](#3-three-step-workflow)
4. [Barcode Scanning Feature](#4-barcode-scanning-feature)
5. [Excel Comparison Feature](#5-excel-comparison-feature)
6. [Report Generation](#6-report-generation)
7. [Folder Structure](#7-folder-structure)
8. [Enums](#8-enums)
9. [Model Classes](#9-model-classes)
10. [NuGet Packages](#10-nuget-packages)
11. [Table Reference](#11-table-reference)

---

## 1. Module Overview

The Audit Module is the core feature of SmartAuditX. It is designed to serve **80% of businesses across all industries** from a small retail shop doing daily inventory checks to a large enterprise running ISO compliance audits across 50 branches.

### What it covers
- Company creates their own reusable audit templates (fully custom)
- Templates support all input types: Yes/No, text, number, rating, photo, barcode scan, signature
- Audits are assigned to auditors with a scheduled date
- Auditors conduct audits via a panel or mobile interface
- Barcode scanning populates field values automatically
- Scanned data can be compared against an uploaded Excel sheet
- Managers receive notifications when audits are assigned and completed
- Reports are generated per audit with full response detail and scoring

### Industries it covers out of the box
- Retail (inventory, stock audit)
- Hospitality (kitchen hygiene, safety checks)
- Healthcare (equipment checks, cleanliness)
- Manufacturing (quality control, machine inspection)
- Education (facility checks, compliance)
- Logistics (warehouse audits, dispatch checks)
- Corporate (HR compliance, IT security audits)
- Any other industry — templates are fully company-defined

---

## 2. Core Concept

```
COMPANY CREATES:
AuditTemplate
  └── AuditTemplateSections        (e.g. "Kitchen Area", "Storage Room")
        └── AuditTemplateFields    (e.g. "Are exits clear?", "Scan product barcode")
              └── AuditTemplateFieldOptions   (for dropdown/checkbox fields)

COMPANY UPLOADS (optional, for barcode comparison):
AuditTemplateInventoryItem
  └── Excel sheet rows loaded here per template

AUDIT IS CONDUCTED:
Audit  (instance of template, assigned to auditor, scheduled date)
  └── AuditResponses              (one row per field per audit)
        └── AuditResponseAttachments  (photos, files as evidence)
        └── AuditBarcodeScans     (scanned barcodes with quantity tracking)
  └── AuditFindings               (issues raised, assigned for resolution)
  └── AuditReport                 (generated report after completion)

RECURRING AUDITS:
AuditSchedule  (runs template on a frequency, auto-creates Audit records)
```

---

## 3. Three-Step Workflow

---

### STEP 1 — Company Panel (Admin / Manager)
**Full CRUD operations. Everything is set up here.**

#### 1.1 Template Management
- Create, edit, delete, publish audit templates
- Add/remove/reorder sections within a template
- Add/remove/reorder fields within a section
- Choose field type for each field (YesNo, Text, Number, Rating, Dropdown, Checkbox, Date, Photo, BarcodeScanner, Signature)
- Set scoring weight per field (if scoring is enabled)
- Set required/optional per field
- Upload Excel inventory sheet to compare during barcode audit
- Publish template when ready (Draft to Published)
- Version control: editing a published template increments version; historical audits are unaffected

#### 1.2 Audit Assignment
- Select a published template
- Set audit title, scheduled date, and target branch
- Assign to an auditor (from employee list)
- Assign a reviewing manager
- Set due date
- System sends notification to auditor (in-panel + email)
- System sends notification to manager (audit coming up on X date)
- Audit status starts as: Scheduled

#### 1.3 Schedule Management
- Create recurring audit schedules (Daily, Weekly, Monthly etc.)
- System auto-creates Audit records at the scheduled frequency
- Assign a default auditor per schedule

#### 1.4 Findings and Resolution Tracking
- View all open findings across all audits
- Assign findings to employees for corrective action
- Set severity and due date per finding
- Track resolution status

#### 1.5 Dashboard and Reports
- View all audits by status, branch, template, date range
- Download generated PDF/Excel reports
- Compare audit scores over time

---

### STEP 2 — Auditor Panel
**The auditor sees only what is assigned to them.**

#### 2.1 Auditor Dashboard
- List of upcoming assigned audits sorted by scheduled date
- Status badges: Scheduled / InProgress / Completed
- Start button only active on or after the scheduled date

#### 2.2 Conducting the Audit
When auditor starts:
- Status changes: Scheduled to InProgress
- StartedAt timestamp is recorded
- All sections and fields load from the template
- Auditor fills each field:
  - YesNo → Toggle
  - Text → Text input
  - Number → Numeric input
  - Rating → Star or scale selector
  - Dropdown → Single select
  - Checkbox → Multi select
  - Date → Date picker
  - Photo → Camera or file upload (stored as attachment)
  - BarcodeScanner → Scan or type barcode
  - Signature → Touch or mouse signature pad

#### 2.3 Barcode Scanning Flow (detailed in Section 4)

#### 2.4 Submitting the Audit
- Auditor can save progress (status stays InProgress)
- On submit: all required fields validated
- Status changes: InProgress to Completed
- CompletedAt timestamp recorded
- Overall score calculated and stored (if scoring enabled)
- Manager receives notification: "Audit X has been completed by Auditor Y"
- Report auto-generated in background

---

### STEP 3 — Manager Panel
**Review, approve, and act on completed audits.**

#### 3.1 Manager Notifications
Manager receives notification in these situations:

| Trigger | Notification |
|---|---|
| Audit assigned to auditor | "Audit [Title] assigned to [Auditor] on [Date]" |
| Audit start date approaching (1 day before) | "Reminder: [Title] is scheduled for tomorrow" |
| Audit overdue (past scheduled date, not started) | "Alert: [Title] is overdue" |
| Audit completed by auditor | "[Auditor] has completed [Title] — review pending" |
| Finding raised | "New finding raised in [Title] — Severity: Critical" |

#### 3.2 Review Flow
- Manager opens completed audit
- Reviews responses section by section
- Can raise additional findings
- Adds review notes
- Approves or sends back for correction
- Status: Completed to UnderReview to Approved

#### 3.3 Finding Resolution
- Assign findings to responsible employees
- Set resolution deadline
- Employee gets notification
- Employee marks as resolved with notes
- Manager verifies and closes finding

---

## 4. Barcode Scanning Feature

### How it works

A BarcodeScanner field type is available when building templates. When an auditor reaches this field during an audit, they scan the barcode with their device camera or a physical scanner.

### Scan Flow

```
Auditor scans barcode
       ↓
System checks AuditTemplateInventoryItems (Excel data loaded for this template)
       ↓
Match found?
  YES → Auto-populate: Item Name, Expected Quantity, Location
  NO  → Flag as "Unrecognized barcode" — auditor notes it manually
       ↓
Auditor enters ACTUAL quantity found physically
       ↓
System compares: Expected vs Actual
       ↓
AuditBarcodeScan row created:
  - BarcodeValue
  - ItemNameSnapshot
  - ExpectedQuantity (from Excel/inventory)
  - ActualQuantity (entered by auditor)
  - DiscrepancyQuantity = ActualQuantity - ExpectedQuantity
  - Status: Matched / Surplus / Shortage / Unrecognized
       ↓
If same barcode scanned again in the same audit → quantity UPDATES
(does not create a duplicate row)
```

### Same Barcode Scanned Twice

Handled by checking AuditId + BarcodeValue before inserting:
- Row already exists → update ActualQuantity and LastScannedAt
- Row does not exist → insert new row

Scanning a product 3 times in different shelf locations accumulates the quantity correctly.

---

## 5. Excel Comparison Feature

### Purpose
Companies upload their expected stock list as an Excel file. The auditor scans barcodes during the physical audit. At the end, the system compares expected vs physically found.

### Upload Flow (Company Panel)
1. Company goes to Template settings
2. Uploads Excel file (.xlsx or .csv)
3. System reads rows using EPPlus or ClosedXML
4. Maps columns: Barcode, Item Name, Expected Quantity, Location (optional)
5. Rows are saved to AuditTemplateInventoryItems table
6. At audit time, these rows are used for barcode lookup

### Excel File Expected Format

| BarcodeValue | ItemName | ExpectedQuantity | Location |
|---|---|---|---|
| 8901234567890 | Nestle Milk 1L | 50 | Shelf A-3 |
| 7501234567890 | Surf Excel 1kg | 30 | Shelf B-1 |

### Comparison Report (generated after audit)

| Item | Barcode | Expected | Actual | Difference | Status |
|---|---|---|---|---|---|
| Nestle Milk 1L | 8901234567890 | 50 | 47 | -3 | Shortage |
| Surf Excel 1kg | 7501234567890 | 30 | 35 | +5 | Surplus |
| Unknown Item | 1234567890000 | — | 2 | — | Unrecognized |
| Dettol 500ml | 5901234567890 | 20 | 0 | -20 | Missing |

Items not scanned at all are shown as Missing (Expected > 0, Actual = 0).

---

## 6. Report Generation

Report is auto-generated when audit status moves to Completed.

### Report Contents
1. Cover: Audit title, template name, branch, auditor name, date, overall score
2. Summary: Total fields, answered, skipped, score breakdown
3. Section-by-section responses with field label, response value, and notes
4. Barcode/Inventory comparison table (if HasBarcodeData = true)
5. Photo attachments with captions
6. Findings summary: title, severity, assigned to, status
7. Signature (if signature field used)
8. Manager review notes (added after approval)

### Output Formats
- PDF (primary — for sharing and archiving)
- Excel (for data analysis and comparison)

### Storage
- Report file generated and uploaded to cloud storage (Azure Blob / S3)
- URL stored in AuditReports.ReportFileUrl
- Link available in both company panel and auditor panel

---

## 7. Folder Structure

```
Models/
  AuditModule/
    Enums/
      AuditEnums.cs
    AuditTemplate.cs
    AuditTemplateSection.cs
    AuditTemplateField.cs
    AuditTemplateFieldOption.cs
    AuditTemplateInventoryItem.cs
    AuditSchedule.cs
    Audit.cs
    AuditResponse.cs
    AuditResponseAttachment.cs
    AuditBarcodeScan.cs
    AuditFinding.cs
    AuditReport.cs
    AuditNotification.cs
```

---

## 8. Enums

```csharp
// Models/AuditModule/Enums/AuditEnums.cs

namespace SmartAuditX.Models.AuditModule.Enums
{
    /// <summary>
    /// What input type this field collects from the auditor.
    /// Drives the UI control rendered on the audit form.
    /// </summary>
    public enum AuditFieldType
    {
        YesNo,           // Pass/Fail toggle — most common in safety audits
        Text,            // Free text input
        Number,          // Numeric value (counts, measurements, temperatures)
        Rating,          // Scale input e.g. 1-5 or 1-10
        Dropdown,        // Single select from defined options
        Checkbox,        // Multi-select from defined options
        Date,            // Date picker
        Photo,           // Image capture/upload (evidence)
        BarcodeScanner,  // Scan with camera/device or type manually — triggers inventory lookup
        Signature        // Touch or mouse signature pad
    }

    /// <summary>
    /// Lifecycle state of an Audit instance.
    /// </summary>
    public enum AuditStatus
    {
        Draft,        // Created but not yet assigned or scheduled
        Scheduled,    // Assigned to auditor with a date set
        InProgress,   // Auditor has started filling responses
        Completed,    // Auditor submitted — pending manager review
        UnderReview,  // Manager is reviewing responses and findings
        Approved,     // Manager signed off — audit closed
        Overdue,      // Past scheduled date, not started
        Cancelled     // Cancelled before completion
    }

    /// <summary>
    /// Severity level of a finding or issue raised during audit.
    /// </summary>
    public enum FindingSeverity
    {
        Critical,       // Immediate shutdown or action required
        High,           // Resolve within 24 hours
        Medium,         // Resolve within a week
        Low,            // Minor — monitor
        Informational   // No action needed — just noted
    }

    /// <summary>
    /// Resolution lifecycle of a finding.
    /// </summary>
    public enum FindingStatus
    {
        Open,
        InProgress,
        Resolved,
        Closed,
        Disputed    // Assigned person challenges the finding
    }

    /// <summary>
    /// Recurrence frequency for automated audit scheduling.
    /// </summary>
    public enum AuditFrequency
    {
        Daily,
        Weekly,
        BiWeekly,
        Monthly,
        Quarterly,
        BiAnnual,
        Yearly
    }

    /// <summary>
    /// Result of a barcode scan compared to the expected inventory.
    /// </summary>
    public enum BarcodeScanStatus
    {
        Matched,        // Actual quantity equals expected quantity
        Surplus,        // Actual quantity is MORE than expected
        Shortage,       // Actual quantity is LESS than expected
        Missing,        // Item in Excel but not scanned at all (quantity = 0)
        Unrecognized    // Barcode not found in template inventory data
    }

    /// <summary>
    /// Output format for generated audit reports.
    /// </summary>
    public enum ReportFormat
    {
        PDF,
        Excel,
        Both
    }

    /// <summary>
    /// Generation status of the audit report.
    /// </summary>
    public enum ReportStatus
    {
        Pending,     // Audit completed, report not yet generated
        Generating,  // Background job is building the report
        Ready,       // Report generated and URL is available
        Failed       // Report generation failed — can retry
    }

    /// <summary>
    /// Type of notification sent for audit-related events.
    /// </summary>
    public enum AuditNotificationType
    {
        AuditAssigned,          // Sent to auditor on assignment
        AuditScheduledReminder, // 1 day before scheduled date
        AuditOverdue,           // Past scheduled date, not started
        AuditCompleted,         // Sent to manager when auditor submits
        FindingRaised,          // Sent to manager when Critical/High finding added
        FindingAssigned,        // Sent to employee assigned to resolve finding
        ReportReady             // Sent when PDF report is generated
    }
}
```

---

## 9. Model Classes

```csharp
// ─── AuditTemplate.cs ────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.AuditModule.Enums;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// A reusable blank audit form created by the company.
    /// Think of it as the blank paper form. Audit is the filled form.
    /// Companies define their own templates. OpsPulse provides no pre-built templates.
    ///
    /// VERSIONING:
    /// When a published template is edited, Version increments.
    /// Old audits keep their TemplateVersionSnapshot.
    /// Historical audits are never affected by changes.
    ///
    /// SCORING:
    /// Optional. When enabled, each field has a weight and each option/response
    /// has a score. Overall score is 0-100.
    ///
    /// INVENTORY:
    /// When HasInventoryComparison = true, company uploads an Excel file.
    /// Rows are stored in AuditTemplateInventoryItems.
    /// BarcodeScanner fields use these rows for lookup during audit.
    /// </summary>
    [Table("AuditTemplates")]
    public class AuditTemplate : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditTemplateId { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Free-text category for filtering.
        /// e.g. "Safety", "Hygiene", "Inventory", "Compliance", "Quality".
        /// </summary>
        [MaxLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// False = template is still being built (Draft).
        /// True = template is ready to be used for creating audits.
        /// </summary>
        public bool IsPublished { get; set; } = false;

        /// <summary>
        /// Starts at 1. Increments each time a published template is modified.
        /// Allows tracking which version of the form each historical audit used.
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// When true: fields have score weights, options have score values.
        /// Overall audit score (0-100) is calculated and stored on Audit.
        /// When false: audit is pass/fail or observation-only with no scoring.
        /// </summary>
        public bool IsScoringEnabled { get; set; } = false;

        /// <summary>
        /// When true: template has an inventory list uploaded via Excel.
        /// BarcodeScanner fields will look up scanned barcodes in AuditTemplateInventoryItems.
        /// Used for stock audits, warehouse checks, retail inventory counts.
        /// </summary>
        public bool HasInventoryComparison { get; set; } = false;

        /// <summary>Rough estimate shown to auditors before they start.</summary>
        public int? EstimatedDurationMinutes { get; set; }

        /// <summary>UserId of the person who created this template.</summary>
        public int CreatedByUserId { get; set; }

        // Navigation
        public virtual Company? Company { get; set; }
        public virtual ICollection<AuditTemplateSection> Sections { get; set; } = new List<AuditTemplateSection>();
        public virtual ICollection<AuditTemplateInventoryItem> InventoryItems { get; set; } = new List<AuditTemplateInventoryItem>();
        public virtual ICollection<Audit> Audits { get; set; } = new List<Audit>();
        public virtual ICollection<AuditSchedule> Schedules { get; set; } = new List<AuditSchedule>();
    }
}
```

```csharp
// ─── AuditTemplateSection.cs ─────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// Groups related fields into a logical section within a template.
    ///
    /// Example — Template: "Restaurant Audit"
    ///   Section 1: "Kitchen Area"
    ///   Section 2: "Storage Room"
    ///   Section 3: "Front of House"
    ///   Section 4: "Inventory Check"
    ///
    /// DisplayOrder controls the sequence on the audit form.
    /// IsRequired means auditor must answer at least one field before submitting.
    /// </summary>
    [Table("AuditTemplateSections")]
    public class AuditTemplateSection : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditTemplateSectionId { get; set; }

        [Required]
        [ForeignKey("AuditTemplate")]
        public int AuditTemplateId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>Lower number = appears first on the form.</summary>
        public int DisplayOrder { get; set; } = 0;

        public bool IsRequired { get; set; } = false;

        // Navigation
        public virtual AuditTemplate? AuditTemplate { get; set; }
        public virtual ICollection<AuditTemplateField> Fields { get; set; } = new List<AuditTemplateField>();
    }
}
```

```csharp
// ─── AuditTemplateField.cs ───────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.AuditModule.Enums;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// A single question or data-capture field within a template section.
    /// FieldType determines which UI control the auditor sees.
    ///
    /// Field Type to UI Control mapping:
    ///   YesNo          → Toggle (Pass/Fail)
    ///   Text           → Text input box
    ///   Number         → Numeric input with optional min/max validation
    ///   Rating         → Star rating or numeric scale (MinValue to MaxValue)
    ///   Dropdown       → Single select from AuditTemplateFieldOptions
    ///   Checkbox       → Multi-select from AuditTemplateFieldOptions
    ///   Date           → Date picker
    ///   Photo          → Camera capture or file upload
    ///   BarcodeScanner → Scan with camera/device or type manually
    ///   Signature      → Touch or mouse signature pad
    ///
    /// ScoreWeight only applies when AuditTemplate.IsScoringEnabled = true.
    /// AllowNotes = true adds a per-field notes box for auditor comments.
    /// </summary>
    [Table("AuditTemplateFields")]
    public class AuditTemplateField : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditTemplateFieldId { get; set; }

        [Required]
        [ForeignKey("AuditTemplateSection")]
        public int AuditTemplateSectionId { get; set; }

        /// <summary>
        /// Denormalized — stored directly for efficient field-level queries
        /// without joining back through Section.
        /// </summary>
        [Required]
        public int AuditTemplateId { get; set; }

        /// <summary>The question shown to the auditor. e.g. "Are fire exits unobstructed?"</summary>
        [Required]
        [MaxLength(500)]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Guidance shown below the label.
        /// e.g. "Check all 3 exits on the ground floor. Take photo if blocked."
        /// </summary>
        [MaxLength(1000)]
        public string? HelpText { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public AuditFieldType FieldType { get; set; }

        public bool IsRequired { get; set; } = false;

        /// <summary>Lower number = appears first within the section.</summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Points this field contributes toward the overall audit score.
        /// e.g. "Fire exit clear?" has weight 20, "Floor clean?" has weight 5.
        /// Only used when IsScoringEnabled = true on the parent template.
        /// </summary>
        public int? ScoreWeight { get; set; }

        /// <summary>For Number/Rating fields: minimum allowed value.</summary>
        [Column(TypeName = "decimal(10,4)")]
        public decimal? MinValue { get; set; }

        /// <summary>For Number/Rating fields: maximum allowed value.</summary>
        [Column(TypeName = "decimal(10,4)")]
        public decimal? MaxValue { get; set; }

        /// <summary>
        /// When true a notes/comment text box appears below this field.
        /// Auditor can add context about their response.
        /// </summary>
        public bool AllowNotes { get; set; } = true;

        /// <summary>
        /// For Photo fields: minimum number of photos auditor must upload.
        /// 0 = optional, 1 or more = required photo count.
        /// </summary>
        public int? MinPhotoCount { get; set; }

        // Navigation
        public virtual AuditTemplateSection? AuditTemplateSection { get; set; }
        public virtual ICollection<AuditTemplateFieldOption> Options { get; set; } = new List<AuditTemplateFieldOption>();
        public virtual ICollection<AuditResponse> Responses { get; set; } = new List<AuditResponse>();
    }
}
```

```csharp
// ─── AuditTemplateFieldOption.cs ─────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// Predefined choices for Dropdown and Checkbox field types.
    ///
    /// Example — Field: "Cleanliness Level"
    ///   Option 1: "Excellent"  Score: 10
    ///   Option 2: "Good"       Score: 7
    ///   Option 3: "Poor"       Score: 3
    ///   Option 4: "Fail"       Score: 0  IsFailOption: true
    ///
    /// Score: numeric value this option contributes to the audit score.
    /// IsFailOption: selecting this option auto-raises a Finding on this audit.
    /// </summary>
    [Table("AuditTemplateFieldOptions")]
    public class AuditTemplateFieldOption : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditTemplateFieldOptionId { get; set; }

        [Required]
        [ForeignKey("AuditTemplateField")]
        public int AuditTemplateFieldId { get; set; }

        [Required]
        [MaxLength(255)]
        public string OptionText { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Score { get; set; }

        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// When auditor selects this option, system automatically raises a Finding.
        /// Useful for "Fail" or "Non-Compliant" options where action is always required.
        /// </summary>
        public bool IsFailOption { get; set; } = false;

        // Navigation
        public virtual AuditTemplateField? AuditTemplateField { get; set; }
    }
}
```

```csharp
// ─── AuditTemplateInventoryItem.cs ───────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// Stores inventory data uploaded via Excel for a specific template.
    /// Only used when AuditTemplate.HasInventoryComparison = true.
    ///
    /// UPLOAD FLOW:
    /// 1. Company uploads Excel file in Template settings
    /// 2. System reads rows using EPPlus or ClosedXML
    /// 3. Each row saved here with BarcodeValue as the lookup key
    ///
    /// DURING AUDIT:
    /// Auditor scans barcode
    /// → System looks up BarcodeValue here
    /// → Auto-fills ItemName and ExpectedQuantity
    /// → Auditor enters ActualQuantity physically counted
    /// → AuditBarcodeScan row created with comparison result
    ///
    /// Items in this table not scanned at all = Missing in the final report.
    /// </summary>
    [Table("AuditTemplateInventoryItems")]
    public class AuditTemplateInventoryItem : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditTemplateInventoryItemId { get; set; }

        [Required]
        [ForeignKey("AuditTemplate")]
        public int AuditTemplateId { get; set; }

        /// <summary>
        /// The barcode value as returned when scanned.
        /// e.g. "8901234567890" (EAN-13), "ABC-001" (internal code).
        /// Must be unique per template — this is the lookup key.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string BarcodeValue { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Where this item should be physically found.
        /// e.g. "Shelf A-3", "Warehouse Zone B", "Freezer 2".
        /// </summary>
        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(100)]
        public string? SKU { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// How many units the company expects to find.
        /// decimal(10,4) supports partial units (1.5 kg, 0.5 litres).
        /// </summary>
        [Column(TypeName = "decimal(10,4)")]
        public decimal ExpectedQuantity { get; set; }

        /// <summary>e.g. "units", "kg", "litres", "boxes".</summary>
        [MaxLength(30)]
        public string? Unit { get; set; }

        // Navigation
        public virtual AuditTemplate? AuditTemplate { get; set; }
    }
}
```

```csharp
// ─── AuditSchedule.cs ────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.AuditModule.Enums;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// Automates recurring audit creation based on a template and frequency.
    ///
    /// A background job (Hangfire or Quartz.NET) reads this table and creates
    /// Audit records automatically when NextScheduledDate is reached.
    ///
    /// Example:
    ///   "Run Kitchen Hygiene Check on Branch A every Monday,
    ///    assigned to auditor Ahmed, reviewed by manager Sara."
    ///
    /// After each run:
    ///   LastRunDate = now
    ///   NextScheduledDate = calculated based on Frequency
    /// </summary>
    [Table("AuditSchedules")]
    public class AuditSchedule : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditScheduleId { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [ForeignKey("Branch")]
        public int? BranchId { get; set; }

        [Required]
        [ForeignKey("AuditTemplate")]
        public int AuditTemplateId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(15)")]
        public AuditFrequency Frequency { get; set; }

        [Required]
        public DateTime NextScheduledDate { get; set; }

        public DateTime? LastRunDate { get; set; }

        /// <summary>Default auditor for auto-created audits from this schedule.</summary>
        [ForeignKey("AssignedToUser")]
        public int? AssignedToUserId { get; set; }

        /// <summary>Default reviewing manager for auto-created audits.</summary>
        [ForeignKey("ReviewerUser")]
        public int? ReviewerUserId { get; set; }

        // Navigation
        public virtual Company? Company { get; set; }
        public virtual Branch? Branch { get; set; }
        public virtual AuditTemplate? AuditTemplate { get; set; }
        public virtual ApplicationUser? AssignedToUser { get; set; }
        public virtual ApplicationUser? ReviewerUser { get; set; }
    }
}
```

```csharp
// ─── Audit.cs ─────────────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.AuditModule.Enums;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// A single conducted audit — the filled version of an AuditTemplate.
    ///
    /// LIFECYCLE:
    ///   Draft → Scheduled → InProgress → Completed → UnderReview → Approved
    ///                                  ↘ Overdue (not started on time)
    ///                     ↘ Cancelled (at any point before Approved)
    ///
    /// TemplateVersionSnapshot:
    ///   Stores the version of the template active when audit was created.
    ///   Historical audits are never affected by template changes.
    ///
    /// OverallScore:
    ///   Computed from responses when IsScoringEnabled = true.
    ///   Stored here for fast reporting — not recalculated on every query.
    ///
    /// HasBarcodeData:
    ///   True if BarcodeScanner fields were used.
    ///   Drives whether inventory comparison section appears in the report.
    /// </summary>
    [Table("Audits")]
    public class Audit : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditId { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        /// <summary>Null = company-wide audit. Set = specific branch being audited.</summary>
        [ForeignKey("Branch")]
        public int? BranchId { get; set; }

        [Required]
        [ForeignKey("AuditTemplate")]
        public int AuditTemplateId { get; set; }

        /// <summary>
        /// Snapshot of AuditTemplate.Version at audit creation time.
        /// Ensures template updates do not affect historical audit records.
        /// </summary>
        public int TemplateVersionSnapshot { get; set; }

        /// <summary>e.g. "Kitchen Hygiene Check — Branch A — June 2025".</summary>
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(15)")]
        public AuditStatus Status { get; set; } = AuditStatus.Draft;

        public DateTime? ScheduledDate { get; set; }

        /// <summary>Set when auditor clicks Start.</summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>Set when auditor clicks Submit.</summary>
        public DateTime? CompletedAt { get; set; }

        [ForeignKey("AssignedToUser")]
        public int? AssignedToUserId { get; set; }

        [ForeignKey("ReviewedByUser")]
        public int? ReviewedByUserId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Percentage score 0.00 to 100.00.
        /// Null if scoring not enabled on the template.
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? OverallScore { get; set; }

        /// <summary>General notes by auditor at the overall audit level.</summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        /// <summary>Manager notes added during review.</summary>
        [MaxLength(2000)]
        public string? ReviewNotes { get; set; }

        /// <summary>
        /// True when any BarcodeScanner fields were answered.
        /// Drives whether inventory comparison section appears in report.
        /// </summary>
        public bool HasBarcodeData { get; set; } = false;

        public int CreatedByUserId { get; set; }

        // Navigation
        public virtual Company? Company { get; set; }
        public virtual Branch? Branch { get; set; }
        public virtual AuditTemplate? AuditTemplate { get; set; }
        public virtual ApplicationUser? AssignedToUser { get; set; }
        public virtual ApplicationUser? ReviewedByUser { get; set; }
        public virtual ICollection<AuditResponse> Responses { get; set; } = new List<AuditResponse>();
        public virtual ICollection<AuditBarcodeScan> BarcodeScans { get; set; } = new List<AuditBarcodeScan>();
        public virtual ICollection<AuditFinding> Findings { get; set; } = new List<AuditFinding>();
        public virtual AuditReport? Report { get; set; }
        public virtual ICollection<AuditNotification> Notifications { get; set; } = new List<AuditNotification>();
    }
}
```

```csharp
// ─── AuditResponse.cs ────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.AuditModule.Enums;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// Stores the auditor's answer for one field in one audit.
    /// One row per field per audit.
    ///
    /// MULTIPLE VALUE COLUMNS:
    /// Different field types need different data types. Only one column
    /// is populated per row based on FieldTypeSnapshot:
    ///   YesNo          → ResponseBoolean
    ///   Text           → ResponseText
    ///   Number/Rating  → ResponseNumber
    ///   Date           → ResponseDate
    ///   Dropdown       → SelectedOptionId
    ///   Checkbox       → Multiple rows, one per selected option
    ///   Photo          → AuditResponseAttachments table
    ///   BarcodeScanner → AuditBarcodeScan table + ResponseText = scanned value
    ///   Signature      → ResponseText = URL to signature image in storage
    ///
    /// SNAPSHOTS:
    /// FieldLabelSnapshot and FieldTypeSnapshot preserve the question at audit time.
    /// Template field renames never corrupt historical audit records.
    /// </summary>
    [Table("AuditResponses")]
    public class AuditResponse : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditResponseId { get; set; }

        [Required]
        [ForeignKey("Audit")]
        public int AuditId { get; set; }

        [Required]
        [ForeignKey("AuditTemplateField")]
        public int AuditTemplateFieldId { get; set; }

        // Snapshots (protect historical accuracy)
        [Required]
        [MaxLength(500)]
        public string FieldLabelSnapshot { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(20)")]
        public AuditFieldType FieldTypeSnapshot { get; set; }

        // Response Values (only one populated per row)
        [MaxLength(2000)]
        public string? ResponseText { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? ResponseNumber { get; set; }

        public bool? ResponseBoolean { get; set; }

        public DateTime? ResponseDate { get; set; }

        /// <summary>For Dropdown fields — which option the auditor selected.</summary>
        [ForeignKey("SelectedOption")]
        public int? SelectedOptionId { get; set; }

        // Scoring
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Score { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>True if field was optional and auditor left it blank.</summary>
        public bool IsSkipped { get; set; } = false;

        // Navigation
        public virtual Audit? Audit { get; set; }
        public virtual AuditTemplateField? AuditTemplateField { get; set; }
        public virtual AuditTemplateFieldOption? SelectedOption { get; set; }
        public virtual ICollection<AuditResponseAttachment> Attachments { get; set; } = new List<AuditResponseAttachment>();
    }
}
```

```csharp
// ─── AuditResponseAttachment.cs ──────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// Photo or file evidence attached to a specific audit response.
    /// Separate table because one Photo field can have multiple uploads.
    ///
    /// e.g. "Take photo of blocked exit" → auditor uploads 3 photos.
    ///
    /// Files stored in cloud storage (Azure Blob or S3).
    /// FileUrl is the public or signed URL to the file.
    /// AuditId is denormalized for efficient audit-level queries without joining responses.
    /// </summary>
    [Table("AuditResponseAttachments")]
    public class AuditResponseAttachment : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditResponseAttachmentId { get; set; }

        [Required]
        [ForeignKey("AuditResponse")]
        public int AuditResponseId { get; set; }

        /// <summary>Denormalized — query all attachments for an audit without joining through responses.</summary>
        [Required]
        public int AuditId { get; set; }

        [Required]
        [MaxLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? FileName { get; set; }

        /// <summary>jpg, png, pdf, mp4 etc.</summary>
        [MaxLength(20)]
        public string? FileType { get; set; }

        public long? FileSizeBytes { get; set; }

        /// <summary>Optional caption added by auditor to describe what the photo shows.</summary>
        [MaxLength(500)]
        public string? Caption { get; set; }

        // Navigation
        public virtual AuditResponse? AuditResponse { get; set; }
    }
}
```

```csharp
// ─── AuditBarcodeScan.cs ─────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.AuditModule.Enums;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// Records every barcode scan performed during an audit.
    ///
    /// DUPLICATE SCAN HANDLING:
    /// When the same barcode is scanned again in the same audit:
    ///   ActualQuantity is UPDATED (quantity accumulates, no duplicate row)
    ///   LastScannedAt and ScanCount are updated
    /// Uniqueness enforced by index on (AuditId, BarcodeValue).
    ///
    /// COMPARISON LOGIC:
    ///   DiscrepancyQuantity = ActualQuantity - ExpectedQuantity
    ///   Positive  = Surplus
    ///   Negative  = Shortage
    ///   Zero      = Matched
    ///   Null expected = Unrecognized (barcode not in inventory data)
    ///   ActualQuantity = 0 with entry in inventory = Missing
    ///
    /// Snapshots preserve expected data at audit time so future Excel
    /// uploads do not alter historical audit scan records.
    /// </summary>
    [Table("AuditBarcodeScans")]
    public class AuditBarcodeScan : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditBarcodeScanId { get; set; }

        [Required]
        [ForeignKey("Audit")]
        public int AuditId { get; set; }

        /// <summary>Links back to the AuditResponse for the BarcodeScanner field.</summary>
        [ForeignKey("AuditResponse")]
        public int? AuditResponseId { get; set; }

        /// <summary>Raw barcode value as returned by the scanner device.</summary>
        [Required]
        [MaxLength(100)]
        public string BarcodeValue { get; set; } = string.Empty;

        // Snapshots from AuditTemplateInventoryItem at scan time
        [MaxLength(255)]
        public string? ItemNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? LocationSnapshot { get; set; }

        [MaxLength(100)]
        public string? SKUSnapshot { get; set; }

        /// <summary>Expected quantity from uploaded Excel data. Null if unrecognized barcode.</summary>
        [Column(TypeName = "decimal(10,4)")]
        public decimal? ExpectedQuantity { get; set; }

        // Auditor Input
        /// <summary>
        /// Physically counted quantity entered by auditor.
        /// UPDATED (not duplicated) when same barcode scanned again in this audit.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,4)")]
        public decimal ActualQuantity { get; set; } = 0;

        [MaxLength(30)]
        public string? Unit { get; set; }

        // Comparison Result
        /// <summary>
        /// ActualQuantity minus ExpectedQuantity.
        /// Positive = Surplus, Negative = Shortage, Zero = Matched.
        /// Null when ExpectedQuantity is null (unrecognized barcode).
        /// </summary>
        [Column(TypeName = "decimal(10,4)")]
        public decimal? DiscrepancyQuantity { get; set; }

        [Column(TypeName = "nvarchar(15)")]
        public BarcodeScanStatus Status { get; set; } = BarcodeScanStatus.Unrecognized;

        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>How many times this barcode was scanned. Updated on each duplicate scan.</summary>
        public int ScanCount { get; set; } = 1;

        public DateTime FirstScannedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastScannedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Audit? Audit { get; set; }
        public virtual AuditResponse? AuditResponse { get; set; }
    }
}
```

```csharp
// ─── AuditFinding.cs ─────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.AuditModule.Enums;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// An issue or non-conformance identified during or after an audit.
    ///
    /// SOURCES:
    /// 1. Auto-raised: auditor selects an option with IsFailOption = true
    /// 2. Manual by auditor: raised during the audit against a specific response
    /// 3. Manual by manager: raised during review after audit is submitted
    ///
    /// LIFECYCLE:
    ///   Open → InProgress → Resolved → Closed
    ///        ↘ Disputed (assigned person challenges the finding)
    ///
    /// NOTIFICATIONS:
    ///   Critical findings trigger immediate notification to manager.
    ///   AssignedToUser receives notification when finding is assigned to them.
    /// </summary>
    [Table("AuditFindings")]
    public class AuditFinding : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditFindingId { get; set; }

        [Required]
        [ForeignKey("Audit")]
        public int AuditId { get; set; }

        /// <summary>The response that is evidence for this finding. Null = general finding not tied to a specific field.</summary>
        [ForeignKey("AuditResponse")]
        public int? AuditResponseId { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Column(TypeName = "nvarchar(15)")]
        public FindingSeverity Severity { get; set; } = FindingSeverity.Medium;

        [Column(TypeName = "nvarchar(15)")]
        public FindingStatus Status { get; set; } = FindingStatus.Open;

        /// <summary>Employee responsible for corrective action.</summary>
        [ForeignKey("AssignedToUser")]
        public int? AssignedToUserId { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [ForeignKey("ResolvedByUser")]
        public int? ResolvedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ResolutionNotes { get; set; }

        public int RaisedByUserId { get; set; }

        // Navigation
        public virtual Audit? Audit { get; set; }
        public virtual AuditResponse? AuditResponse { get; set; }
        public virtual ApplicationUser? AssignedToUser { get; set; }
        public virtual ApplicationUser? ResolvedByUser { get; set; }
    }
}
```

```csharp
// ─── AuditReport.cs ──────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.AuditModule.Enums;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// Stores the generated report for a completed audit.
    /// One-to-one with Audit.
    ///
    /// GENERATION TRIGGER:
    /// Audit status moves to Completed.
    /// Background job (Hangfire) picks it up, sets Status to Generating,
    /// builds the file, uploads to cloud storage, updates PdfUrl, sets Status to Ready.
    ///
    /// REPORT CONTENTS:
    /// 1. Cover page (title, auditor, branch, date, score)
    /// 2. Summary (total fields, completed, skipped, score breakdown)
    /// 3. Section-by-section responses with notes
    /// 4. Inventory comparison table (if HasBarcodeData = true)
    /// 5. Photo evidence with captions
    /// 6. Findings summary with severities and statuses
    /// 7. Signatures (if Signature field was used)
    /// 8. Manager review notes (appended after Approved)
    /// </summary>
    [Table("AuditReports")]
    public class AuditReport : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditReportId { get; set; }

        [Required]
        [ForeignKey("Audit")]
        public int AuditId { get; set; }

        [Column(TypeName = "nvarchar(12)")]
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        [Column(TypeName = "nvarchar(10)")]
        public ReportFormat Format { get; set; } = ReportFormat.PDF;

        /// <summary>Cloud storage URL to the generated PDF.</summary>
        [MaxLength(500)]
        public string? PdfUrl { get; set; }

        /// <summary>Cloud storage URL to the generated Excel file.</summary>
        [MaxLength(500)]
        public string? ExcelUrl { get; set; }

        public DateTime? GeneratedAt { get; set; }

        /// <summary>Error detail if generation failed — for retry and debugging.</summary>
        [MaxLength(1000)]
        public string? FailureReason { get; set; }

        public int? PageCount { get; set; }

        public long? FileSizeBytes { get; set; }

        // Navigation
        public virtual Audit? Audit { get; set; }
    }
}
```

```csharp
// ─── AuditNotification.cs ────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.AuditModule.Enums;

namespace SmartAuditX.Models.AuditModule
{
    /// <summary>
    /// Logs every notification sent for audit-related events.
    ///
    /// SENT WHEN:
    ///   AuditAssigned          → Auditor (when audit is assigned)
    ///   AuditScheduledReminder → Auditor + Manager (1 day before scheduled date)
    ///   AuditOverdue           → Manager (scheduled date passed, not started)
    ///   AuditCompleted         → Manager (auditor submitted)
    ///   FindingRaised          → Manager (Critical or High severity finding added)
    ///   FindingAssigned        → Employee assigned to resolve the finding
    ///   ReportReady            → Manager + Auditor (PDF report generated)
    ///
    /// WHY LOG:
    ///   Prevents duplicate sends, provides trail for delivery disputes,
    ///   enables manual resend from admin panel.
    /// </summary>
    [Table("AuditNotifications")]
    public class AuditNotification : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditNotificationId { get; set; }

        [Required]
        [ForeignKey("Audit")]
        public int AuditId { get; set; }

        [Required]
        public int RecipientUserId { get; set; }

        [Column(TypeName = "nvarchar(30)")]
        public AuditNotificationType Type { get; set; }

        /// <summary>Email | SMS | InApp | All</summary>
        [Required]
        [MaxLength(10)]
        public string Channel { get; set; } = "Email";

        /// <summary>Sent | Delivered | Failed | Bounced</summary>
        [Required]
        [MaxLength(15)]
        public string DeliveryStatus { get; set; } = "Sent";

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Audit? Audit { get; set; }
    }
}
```

---

## 10. NuGet Packages

```
BARCODE SCANNING
ZXing.Net                          Decode barcodes server-side (QR, EAN, Code128 etc.)
ZXing.Net.Bindings.SkiaSharp       Image processing for barcode reading from uploaded photos

EXCEL READ AND WRITE
EPPlus                             Read and write .xlsx files (inventory upload + Excel report)
ClosedXML                          Alternative for simpler Excel tasks

PDF REPORT GENERATION
QuestPDF                           Modern fluent API for PDF generation (recommended)
PdfSharpCore                       Lightweight alternative for basic PDFs

BACKGROUND JOBS
Hangfire                           Job scheduler with built-in dashboard
Hangfire.SqlServer                 SQL Server storage backend for Hangfire

EMAIL NOTIFICATIONS
MailKit                            Send emails (audit notifications, reminders)
MimeKit                            Build MIME email messages (used with MailKit)

FILE STORAGE
Azure.Storage.Blobs                Azure Blob Storage for photos, PDFs, Excel files
AWSSDK.S3                          Amazon S3 alternative

IMAGE PROCESSING
SixLabors.ImageSharp               Resize and compress photos uploaded by auditors

REAL-TIME NOTIFICATIONS
Microsoft.AspNetCore.SignalR       Push live notifications to manager panel
```

Install all at once:
```bash
dotnet add package ZXing.Net
dotnet add package EPPlus
dotnet add package QuestPDF
dotnet add package Hangfire
dotnet add package Hangfire.SqlServer
dotnet add package MailKit
dotnet add package MimeKit
dotnet add package Azure.Storage.Blobs
dotnet add package SixLabors.ImageSharp
dotnet add package Microsoft.AspNetCore.SignalR
```

---

## 11. Table Reference

| # | Table | Purpose |
|---|---|---|
| 1 | AuditTemplates | Reusable blank audit forms created by the company |
| 2 | AuditTemplateSections | Logical groupings of fields within a template |
| 3 | AuditTemplateFields | Individual questions or fields within sections |
| 4 | AuditTemplateFieldOptions | Dropdown and checkbox choices per field |
| 5 | AuditTemplateInventoryItems | Excel inventory data uploaded per template for barcode comparison |
| 6 | AuditSchedules | Recurring audit definitions that auto-create Audit records |
| 7 | Audits | Actual conducted audit instances |
| 8 | AuditResponses | One answer per field per audit |
| 9 | AuditResponseAttachments | Photos and files attached to responses |
| 10 | AuditBarcodeScans | Barcode scan results with quantity comparison data |
| 11 | AuditFindings | Issues raised during or after audit with full resolution lifecycle |
| 12 | AuditReports | Generated PDF and Excel report per completed audit |
| 13 | AuditNotifications | Log of all audit-related notifications sent to users |

**Total: 13 tables**

---

## Key Design Decisions Summary

| Decision | Reason |
|---|---|
| Company-defined templates only | Covers all industries without hardcoding any audit types |
| Section → Field → Option hierarchy | Mirrors how real paper audit forms are structured |
| FieldLabelSnapshot and FieldTypeSnapshot on responses | Template edits never corrupt historical audit data |
| TemplateVersionSnapshot on Audit | Know exactly which version of the form was used for each audit |
| AuditTemplateInventoryItems separate table | Excel data is template-scoped, not a separate inventory module |
| AuditBarcodeScans separate table | Complex scan data with quantity tracking needs its own structure |
| Update not insert on duplicate barcode | Same item scanned at multiple locations accumulates quantity correctly |
| IsFailOption on FieldOption | Auto-raise findings without requiring auditor to create them manually |
| AuditReport as separate table | Report generation is async via background job — needs status tracking |
| HasBarcodeData flag on Audit | Efficiently drive inventory section in report without scanning all responses |
| AuditNotifications log table | Prevent duplicate sends, enable resend, resolve delivery disputes |
| Hangfire for background jobs | Report generation, reminders, overdue alerts all run async without blocking |
| SignalR for real-time updates | Manager panel updates instantly when audit is submitted |