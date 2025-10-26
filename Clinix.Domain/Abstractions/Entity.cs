namespace Clinix.Domain.Abstractions;

public abstract class Entity
    {
    public long Id { get; protected set; } 
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();
    protected void Raise(IDomainEvent ev) => _events.Add(ev);
    public void ClearDomainEvents() => _events.Clear();
    }

public interface IDomainEvent { DateTimeOffset OccurredOn { get; } }
