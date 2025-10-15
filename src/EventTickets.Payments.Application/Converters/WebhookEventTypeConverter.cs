using System.Text.Json;
using System.Text.Json.Serialization;
using EventTickets.Payments.Application.PaymentIntents;

namespace EventTickets.Payments.Application.Converters;

public class WebhookEventTypeConverter : JsonConverter<WebhookEventType>
{
    public override WebhookEventType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch {
            "payment.succeeded" => WebhookEventType.PaymentSucceeded,
            "payment.failed" => WebhookEventType.PaymentFailed,
            "payment.cancelled" => WebhookEventType.PaymentCancelled,
            _ => throw new JsonException($"Unknown event type: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, WebhookEventType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch {
            WebhookEventType.PaymentSucceeded => "payment.suceeded",
            WebhookEventType.PaymentFailed => "payment.failed",
            WebhookEventType.PaymentCancelled => "payment.cancelled",
            _ => throw new JsonException($"Unknown event type {value}")
        });
    }
}