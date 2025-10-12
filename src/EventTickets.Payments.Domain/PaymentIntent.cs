namespace EventTickets.Payments.Domain;

public enum PaymentStatus { Requested, Authorized, Captured, Failed, Canceled }

public sealed class PaymentIntent
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ReservationId { get; private set; }
    public decimal Amount { get; private set; }
    public string? PayUrl { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Requested;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    public PaymentIntent() { }

    private PaymentIntent(Guid reservationId, decimal amount, string? payUrl)
        => (ReservationId, Amount, PayUrl) = (reservationId, amount, payUrl);

    public static PaymentIntent Create(Guid reservationId, decimal amount, string payUrl)
        => new(reservationId, amount, payUrl);

    public void Authorize() { Status = PaymentStatus.Authorized; UpdatedAtUtc = DateTime.UtcNow; }
    public void Capture() { Status = PaymentStatus.Captured; UpdatedAtUtc = DateTime.UtcNow; }
    public void Fail() { Status = PaymentStatus.Failed; UpdatedAtUtc = DateTime.UtcNow; }
    public void Cancel() { Status = PaymentStatus.Canceled; UpdatedAtUtc = DateTime.UtcNow; }
}

