using Marten;

namespace WorkflowWithMarten.Services;

public interface IInvoiceService
{
    Task<Guid> CreateInvoice(CreateInvoiceCommand command);
    Task SubmitForApproval(SubmitInvoiceCommand command);
    Task ApproveByManager(ApproveByManagerCommand command);
    Task ApproveByFinance(ApproveByFinanceCommand command);
    Task RejectInvoice(RejectInvoiceCommand command);
    Task SchedulePayment(SchedulePaymentCommand command);
    Task MarkAsPaid(MarkAsPaidCommand command);
    Task<Invoice> GetInvoice(Guid invoiceId);
}

public class InvoiceService : IInvoiceService
{
    private readonly IDocumentSession _session;

    public InvoiceService(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<Guid> CreateInvoice(CreateInvoiceCommand command)
    {
        var invoiceId = Guid.NewGuid();

        var @event = new InvoiceCreated(
            invoiceId,
            command.InvoiceNumber,
            command.Amount,
            command.Vendor,
            DateTimeOffset.UtcNow
        );

        _session.Events.StartStream<Invoice>(invoiceId, @event);
        await _session.SaveChangesAsync();

        return invoiceId;
    }

    public async Task SubmitForApproval(SubmitInvoiceCommand command)
    {
        // Validate current state
        var invoice = await GetInvoice(command.InvoiceId);
        if (invoice.Status != WorkflowStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot submit invoice in {invoice.Status} status");
        }

        var @event = new InvoiceSubmittedForApproval(
            command.InvoiceId,
            command.SubmittedBy,
            DateTimeOffset.UtcNow
        );

        _session.Events.Append(command.InvoiceId, @event);
        await _session.SaveChangesAsync();
    }

    public async Task ApproveByManager(ApproveByManagerCommand command)
    {
        // Validate current state
        var invoice = await GetInvoice(command.InvoiceId);
        if (invoice.Status != WorkflowStatus.ManagerApproval)
        {
            throw new InvalidOperationException($"Cannot approve invoice in {invoice.Status} status");
        }

        var @event = new InvoiceApprovedByManager(
            command.InvoiceId,
            command.ApprovedBy,
            DateTimeOffset.UtcNow
        );

        _session.Events.Append(command.InvoiceId, @event);
        await _session.SaveChangesAsync();
    }

    public async Task ApproveByFinance(ApproveByFinanceCommand command)
    {
        // Validate current state
        var invoice = await GetInvoice(command.InvoiceId);
        if (invoice.Status != WorkflowStatus.FinanceApproval)
        {
            throw new InvalidOperationException($"Cannot approve invoice in {invoice.Status} status");
        }

        var @event = new InvoiceApprovedByFinance(
            command.InvoiceId,
            command.ApprovedBy,
            DateTimeOffset.UtcNow
        );

        _session.Events.Append(command.InvoiceId, @event);
        await _session.SaveChangesAsync();
    }

    public async Task RejectInvoice(RejectInvoiceCommand command)
    {
        // Can reject from multiple states
        var invoice = await GetInvoice(command.InvoiceId);
        if (invoice.Status == WorkflowStatus.Draft ||
            invoice.Status == WorkflowStatus.Paid ||
            invoice.Status == WorkflowStatus.Rejected)
        {
            throw new InvalidOperationException($"Cannot reject invoice in {invoice.Status} status");
        }

        var @event = new InvoiceRejected(
            command.InvoiceId,
            command.RejectedBy,
            command.Reason,
            DateTimeOffset.UtcNow
        );

        _session.Events.Append(command.InvoiceId, @event);
        await _session.SaveChangesAsync();
    }

    public async Task SchedulePayment(SchedulePaymentCommand command)
    {
        // Validate current state
        var invoice = await GetInvoice(command.InvoiceId);
        if (invoice.Status != WorkflowStatus.PaymentPending)
        {
            throw new InvalidOperationException($"Cannot schedule payment for invoice in {invoice.Status} status");
        }

        var @event = new InvoiceSetForPayment(
            command.InvoiceId,
            command.ScheduledFor
        );

        _session.Events.Append(command.InvoiceId, @event);
        await _session.SaveChangesAsync();
    }

    public async Task MarkAsPaid(MarkAsPaidCommand command)
    {
        // Validate current state
        var invoice = await GetInvoice(command.InvoiceId);
        if (invoice.Status != WorkflowStatus.PaymentPending)
        {
            throw new InvalidOperationException($"Cannot mark invoice as paid in {invoice.Status} status");
        }

        var @event = new InvoiceMarkedAsPaid(
            command.InvoiceId,
            command.Reference,
            DateTimeOffset.UtcNow
        );

        _session.Events.Append(command.InvoiceId, @event);
        await _session.SaveChangesAsync();
    }

    public async Task<Invoice> GetInvoice(Guid invoiceId)
    {
        // This will get the current state of the invoice by replaying all events
        var invoice = await _session.Events.AggregateStreamAsync<Invoice>(invoiceId);

        if (invoice == null)
        {
            throw new InvalidOperationException($"Invoice with ID {invoiceId} not found");
        }

        return invoice;
    }
}

// Commands for our workflow
public record CreateInvoiceCommand(string InvoiceNumber, decimal Amount, string Vendor);
public record SubmitInvoiceCommand(Guid InvoiceId, string SubmittedBy);
public record ApproveByManagerCommand(Guid InvoiceId, string ApprovedBy);
public record ApproveByFinanceCommand(Guid InvoiceId, string ApprovedBy);
public record RejectInvoiceCommand(Guid InvoiceId, string RejectedBy, string Reason);
public record SchedulePaymentCommand(Guid InvoiceId, DateTimeOffset ScheduledFor);
public record MarkAsPaidCommand(Guid InvoiceId, string Reference);