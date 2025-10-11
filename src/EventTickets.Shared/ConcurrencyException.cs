namespace EventTickets.Shared;

public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(string? message = null, Exception? inner = null)
        : base(message ?? "Concurrency conflict", inner) { }
}
