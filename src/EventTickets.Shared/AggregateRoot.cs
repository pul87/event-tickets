using MediatR;

namespace EventTickets.Shared;

public interface IHasDomainEvents
{
    IReadOnlyCollection<INotification> DomainEvents { get; }
    void ClearDomainEvents();
}

public abstract class AggregateRoot<TId> : IHasDomainEvents
{
    public TId Id { get; protected set; } = default!;
    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();
    protected void AddDomainEvent(INotification e) => _domainEvents.Add(e);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}