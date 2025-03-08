using Marten.Events.Aggregation;

namespace WorkflowWithMarten.Projections;

public class InvoiceProjection : SingleStreamProjection<Invoice>
{
    public InvoiceProjection()
    {
        // We can set up any special event handling in the constructor if needed
    }

    // Create a new Invoice when InvoiceCreated event occurs
    public static Invoice Create(InvoiceCreated created)
    {
        return new Invoice
        {
            Id = created.InvoiceId,
            InvoiceNumber = created.InvoiceNumber,
            Amount = created.Amount,
            Vendor = created.Vendor,
            Status = WorkflowStatus.Draft,
            CreatedAt = created.CreatedAt,
            WorkflowHistory = new List<WorkflowTransition>
            {
                new WorkflowTransition
                {
                    ToStatus = WorkflowStatus.Draft,
                    TransitionedAt = created.CreatedAt,
                    Notes = "Invoice created"
                }
            }
        };
    }

    // Apply InvoiceSubmittedForApproval event
    public static Invoice Apply(InvoiceSubmittedForApproval submitted, Invoice invoice)
    {
        var previousStatus = invoice.Status;
        invoice.Status = WorkflowStatus.ManagerApproval;
        invoice.SubmittedAt = submitted.SubmittedAt;
        invoice.SubmittedBy = submitted.SubmittedBy;

        invoice.WorkflowHistory.Add(new WorkflowTransition
        {
            FromStatus = previousStatus,
            ToStatus = WorkflowStatus.ManagerApproval,
            PerformedBy = submitted.SubmittedBy,
            TransitionedAt = submitted.SubmittedAt,
            Notes = "Submitted for approval"
        });

        return invoice;
    }

    // Apply InvoiceApprovedByManager event
    public static Invoice Apply(InvoiceApprovedByManager approved, Invoice invoice)
    {
        var previousStatus = invoice.Status;
        invoice.Status = WorkflowStatus.FinanceApproval;
        invoice.ApprovedByManagerAt = approved.ApprovedAt;
        invoice.ApprovedByManager = approved.ApprovedBy;

        invoice.WorkflowHistory.Add(new WorkflowTransition
        {
            FromStatus = previousStatus,
            ToStatus = WorkflowStatus.FinanceApproval,
            PerformedBy = approved.ApprovedBy,
            TransitionedAt = approved.ApprovedAt,
            Notes = "Approved by manager"
        });

        return invoice;
    }

    // Apply InvoiceApprovedByFinance event
    public static Invoice Apply(InvoiceApprovedByFinance approved, Invoice invoice)
    {
        var previousStatus = invoice.Status;
        invoice.Status = WorkflowStatus.PaymentPending;
        invoice.ApprovedByFinanceAt = approved.ApprovedAt;
        invoice.ApprovedByFinance = approved.ApprovedBy;

        invoice.WorkflowHistory.Add(new WorkflowTransition
        {
            FromStatus = previousStatus,
            ToStatus = WorkflowStatus.PaymentPending,
            PerformedBy = approved.ApprovedBy,
            TransitionedAt = approved.ApprovedAt,
            Notes = "Approved by finance"
        });

        return invoice;
    }

    // Apply InvoiceRejected event
    public static Invoice Apply(InvoiceRejected rejected, Invoice invoice)
    {
        var previousStatus = invoice.Status;
        invoice.Status = WorkflowStatus.Rejected;
        invoice.RejectedAt = rejected.RejectedAt;
        invoice.RejectedBy = rejected.RejectedBy;
        invoice.RejectedReason = rejected.Reason;

        invoice.WorkflowHistory.Add(new WorkflowTransition
        {
            FromStatus = previousStatus,
            ToStatus = WorkflowStatus.Rejected,
            PerformedBy = rejected.RejectedBy,
            TransitionedAt = rejected.RejectedAt,
            Notes = $"Rejected: {rejected.Reason}"
        });

        return invoice;
    }

    // Apply InvoiceSetForPayment event
    public static Invoice Apply(InvoiceSetForPayment scheduled, Invoice invoice)
    {
        invoice.ScheduledForPaymentAt = scheduled.ScheduledFor;
        return invoice;
    }

    // Apply InvoiceMarkedAsPaid event
    public static Invoice Apply(InvoiceMarkedAsPaid paid, Invoice invoice)
    {
        var previousStatus = invoice.Status;
        invoice.Status = WorkflowStatus.Paid;
        invoice.PaidAt = paid.PaidAt;
        invoice.PaymentReference = paid.Reference;

        invoice.WorkflowHistory.Add(new WorkflowTransition
        {
            FromStatus = previousStatus,
            ToStatus = WorkflowStatus.Paid,
            TransitionedAt = paid.PaidAt,
            Notes = $"Payment processed with reference: {paid.Reference}"
        });

        return invoice;
    }
}
