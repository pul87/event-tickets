namespace EventTickets.Ticketing.Contracts.V1;

public sealed record ReservationPlaced
(
    Guid ReservationId,
    Guid PerformanceId,
    int Quantity
);