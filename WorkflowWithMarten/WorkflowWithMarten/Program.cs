using Marten.Events.Daemon.Resiliency;
using Marten;
using Weasel.Core;
using Marten.Events;
using WorkflowWithMarten.Projections;
using Marten.Events.Projections;
using WorkflowWithMarten.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure PostgreSQL connection
var connectionString = "Host=localhost;Port=5432;Database=marten_workflow;Username=postgres;Password=postgres";

// Configure Marten
builder.Services.AddMarten(opts =>
{
    opts.Connection(connectionString);

    // Set the database to automatically create tables and schema objects as needed
    opts.AutoCreateSchemaObjects = AutoCreate.All;

    // Register projections
    opts.Projections.Add<InvoiceProjection>(ProjectionLifecycle.Inline);

    // Optional: Configure EventStore settings
    opts.Events.StreamIdentity = StreamIdentity.AsGuid;
    opts.Events.MetadataConfig.EnableAll();

})
// Optional: Add event processing daemon for async projections
.AddAsyncDaemon(DaemonMode.Solo);

// Register our services
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseCors();

app.UseHttpsRedirection();

// API endpoints
var api = app.MapGroup("/api");

// Invoices endpoints
var invoices = api.MapGroup("/invoices");

// Get invoice by id
invoices.MapGet("/{id}", async (Guid id, IInvoiceService invoiceService) =>
{
    try
    {
        var invoice = await invoiceService.GetInvoice(id);
        return Results.Ok(invoice);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(ex.Message);
    }
});

// Create a new invoice
invoices.MapPost("/", async (CreateInvoiceCommand command, IInvoiceService invoiceService) =>
{
    var id = await invoiceService.CreateInvoice(command);
    return Results.Created($"/api/invoices/{id}", new { Id = id });
});

// Submit for approval
invoices.MapPost("/{id}/submit", async (Guid id, SubmitRequest request, IInvoiceService invoiceService) =>
{
    try
    {
        await invoiceService.SubmitForApproval(new SubmitInvoiceCommand(id, request.SubmittedBy));
        return Results.Ok();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Manager approval
invoices.MapPost("/{id}/manager-approval", async (Guid id, ApproveRequest request, IInvoiceService invoiceService) =>
{
    try
    {
        await invoiceService.ApproveByManager(new ApproveByManagerCommand(id, request.ApprovedBy));
        return Results.Ok();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Finance approval
invoices.MapPost("/{id}/finance-approval", async (Guid id, ApproveRequest request, IInvoiceService invoiceService) =>
{
    try
    {
        await invoiceService.ApproveByFinance(new ApproveByFinanceCommand(id, request.ApprovedBy));
        return Results.Ok();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Reject invoice
invoices.MapPost("/{id}/reject", async (Guid id, RejectRequest request, IInvoiceService invoiceService) =>
{
    try
    {
        await invoiceService.RejectInvoice(new RejectInvoiceCommand(id, request.RejectedBy, request.Reason));
        return Results.Ok();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Schedule payment
invoices.MapPost("/{id}/schedule-payment", async (Guid id, SchedulePaymentRequest request, IInvoiceService invoiceService) =>
{
    try
    {
        await invoiceService.SchedulePayment(new SchedulePaymentCommand(id, request.ScheduledFor));
        return Results.Ok();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Mark as paid
invoices.MapPost("/{id}/mark-paid", async (Guid id, MarkPaidRequest request, IInvoiceService invoiceService) =>
{
    try
    {
        await invoiceService.MarkAsPaid(new MarkAsPaidCommand(id, request.Reference));
        return Results.Ok();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});
app.Run();

// API Request DTOs
record SubmitRequest(string SubmittedBy);
record ApproveRequest(string ApprovedBy);
record RejectRequest(string RejectedBy, string Reason);
record SchedulePaymentRequest(DateTimeOffset ScheduledFor);
record MarkPaidRequest(string Reference);


