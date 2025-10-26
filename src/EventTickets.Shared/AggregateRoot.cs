namespace EventTickets.Shared;

public interface IDomainEvent { }
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

public abstract class AggregateRoot<Tid> : IHasDomainEvents
{
    public Tid Id { get; protected set; } = default!;
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    // would be interesting to use domain events to fire notifications to other services via domain handlers.
    // example: when a reservation is placed, we could fire a notification to the payments service to create a payment intent
    // instead of manually firing the integration events.
    protected void AddDomainEvent(IDomainEvent e) => _domainEvents.Add(e);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}