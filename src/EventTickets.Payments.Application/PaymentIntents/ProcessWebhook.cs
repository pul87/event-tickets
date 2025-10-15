using System.Text.Json.Serialization;
using EventTickets.Payments.Application.Abstractions;
using EventTickets.Payments.Application.Converters;
using EventTickets.Payments.Domain;
using MediatR;

namespace EventTickets.Payments.Application.PaymentIntents;

public enum  WebhookEventType {
    PaymentSucceeded,
    PaymentFailed,
    PaymentCancelled,
}
public sealed record ProcessWebhook(
    [property: JsonConverter(typeof(WebhookEventTypeConverter))]
    WebhookEventType EventType,
    Guid PaymentIntentId,
    string? ProviderTransactionId,
    string? FailureReason,
    decimal? Amount,
    DateTime Timestamp
) : IRequest<ProcessWebhookResult?>;
public sealed record ProcessWebhookResult(
    bool Success,
    string? ErrorMessage = null,
    PaymentStatus? NewStatus = null
);

public sealed class ProcessWebhookHandler : IRequestHandler<ProcessWebhook, ProcessWebhookResult?>
{
    private readonly IPaymentIntentService _service;

    public ProcessWebhookHandler(IPaymentIntentService service) => _service = service;
    
    public async Task<ProcessWebhookResult?> Handle(ProcessWebhook request, CancellationToken ct)
    {
        return await _service.ProcessWebhookAsync(request, ct);
    }
}