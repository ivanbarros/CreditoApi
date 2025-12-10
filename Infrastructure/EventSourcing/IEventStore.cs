namespace CreditoAPI.Infrastructure.EventSourcing;

public interface IEventStore
{
    Task SaveEventAsync<T>(T @event) where T : CreditoEvent;
    Task<IEnumerable<T>> GetEventsAsync<T>(DateTime? desde = null) where T : CreditoEvent;
    Task<IEnumerable<CreditoEvent>> GetEventsByAggregateIdAsync(string aggregateId);
}
