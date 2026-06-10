// ─── Enums/BillingEnums.cs ────────────────────────────────────────────────

namespace SmartAuditX.Models.BillingModule.Enums
{
    public enum PaymentStatus
    {
        Initiated,
        Pending,
        RequiresAction,
        RequiresCapture,
        Authorized,
        Captured,
        Success,
        Failed,
        Declined,
        Cancelled,
        Abandoned,
        TimedOut,
        Expired,
        FraudBlocked,
        Voided,
        Refunded,
        PartialRefund,
        DisputeOpened,
        DisputeWon,
        DisputeLost,
        Chargeback
    }

    public enum SubscriptionStatus
    {
        Pending,
        Trial,
        Active,
        PastDue,
        Suspended,
        Cancelled,
        Expired
    }

    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        BankTransfer,
        DirectDebit,
        JazzCash,
        EasyPaisa,
        NayaPay,
        SadaPay,
        PayPal,
        ApplePay,
        GooglePay,
        Cash,
        Cheque,
        Crypto
    }

    public enum CardBrand
    {
        Unknown,
        Visa,
        Mastercard,
        Amex,
        UnionPay,
        Discover
    }

    public enum PaymentFailureCode
    {
        None,
        InsufficientFunds,
        CardDeclined,
        CardExpired,
        InvalidCardNumber,
        InvalidCVV,
        CardLostOrStolen,
        CardBlocked,
        DailyLimitExceeded,
        InternationalBlocked,
        ThreeDSFailed,
        ThreeDSAbandoned,
        ThreeDSTimeout,
        GatewayTimeout,
        GatewayError,
        NetworkError,
        GatewayMaintenance,
        DuplicateTransaction,
        FraudSuspected,
        VelocityCheckFailed,
        CurrencyNotSupported,
        AmountMismatch,
        SessionExpired,
        UserCancelled,
        RefundFailed,
        RefundWindowExpired
    }

    public enum BillingCycle
    {
        Monthly = 1,
        Quarterly = 3,
        BiAnnual = 6,
        Yearly = 12
    }

    public enum GatewayScope
    {
        Local,
        Global
    }

    public enum DiscountType
    {
        Percentage,
        FixedAmount
    }

    public enum RefundReason
    {
        CustomerRequest,
        Duplicate,
        Fraud,
        ServiceIssue,
        Other
    }

    public enum RefundStatus
    {
        Pending,
        Success,
        Failed
    }

    public enum WebhookStatus
    {
        Received,
        Processed,
        Failed,
        Ignored
    }

    public enum InvoiceStatus
    {
        Draft,
        Issued,
        Paid,
        Void
    }

    public enum DunningStatus
    {
        Pending,
        Success,
        Failed,
        Cancelled
    }

    public enum PlanChangeType
    {
        Upgrade,
        Downgrade,
        Renewal,
        Reactivation
    }

    public enum NotificationType
    {
        PaymentSuccess,
        PaymentFailed,
        UpcomingRenewal,
        GracePeriodWarning,
        Suspended,
        RefundProcessed
    }

    public enum CreditReason
    {
        Refund,
        Goodwill,
        Referral,
        ProratedCredit,
        Promo
    }
}