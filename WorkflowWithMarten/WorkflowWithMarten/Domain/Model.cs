


// Status enum for our workflow
public enum WorkflowStatus
{
    Draft,
    ManagerApproval,
    FinanceApproval,
    PaymentPending,
    Paid,
    Rejected
}

// Our events
public record InvoiceCreated(Guid InvoiceId, string InvoiceNumber, decimal Amount, string Vendor, DateTimeOffset CreatedAt);
public record InvoiceSubmittedForApproval(Guid InvoiceId, string SubmittedBy, DateTimeOffset SubmittedAt);
public record InvoiceApprovedByManager(Guid InvoiceId, string ApprovedBy, DateTimeOffset ApprovedAt);
public record InvoiceApprovedByFinance(Guid InvoiceId, string ApprovedBy, DateTimeOffset ApprovedAt);
public record InvoiceRejected(Guid InvoiceId, string RejectedBy, string Reason, DateTimeOffset RejectedAt);
public record InvoiceSetForPayment(Guid InvoiceId, DateTimeOffset ScheduledFor);
public record InvoiceMarkedAsPaid(Guid InvoiceId, string Reference, DateTimeOffset PaidAt);

// Our aggregate - represents the current state of an invoice
public class Invoice
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; }
    public decimal Amount { get; set; }
    public string Vendor { get; set; }
    public WorkflowStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedByManagerAt { get; set; }
    public DateTimeOffset? ApprovedByFinanceAt { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string RejectedReason { get; set; }
    public DateTimeOffset? ScheduledForPaymentAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public string PaymentReference { get; set; }

    // Who performed each action
    public string SubmittedBy { get; set; }
    public string ApprovedByManager { get; set; }
    public string ApprovedByFinance { get; set; }
    public string RejectedBy { get; set; }

    // Tracks the version of the stream (used for optimistic concurrency)
    public int Version { get; set; }

    // List of workflow transitions for audit purposes
    public List<WorkflowTransition> WorkflowHistory { get; set; } = new();
}

// For tracking the history of workflow transitions
public class WorkflowTransition
{
    public WorkflowStatus FromStatus { get; set; }
    public WorkflowStatus ToStatus { get; set; }
    public string PerformedBy { get; set; }
    public DateTimeOffset TransitionedAt { get; set; }
    public string Notes { get; set; }
}