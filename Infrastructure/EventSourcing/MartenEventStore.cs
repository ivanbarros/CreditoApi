using Marten;

namespace CreditoAPI.Infrastructure.EventSourcing;

public class MartenEventStore : IEventStore
{
    private readonly IDocumentStore _documentStore;
    private readonly ILogger<MartenEventStore> _logger;

    public MartenEventStore(IDocumentStore documentStore, ILogger<MartenEventStore> logger)
    {
        _documentStore = documentStore;
        _logger = logger;
    }

    public async Task SaveEventAsync<T>(T @event) where T : CreditoEvent
    {
        try
        {
            using var session = _documentStore.LightweightSession();
            session.Events.Append(@event.Id, @event);
            await session.SaveChangesAsync();
            
            _logger.LogInformation("Event {EventType} saved with ID {EventId}", 
                @event.EventType, @event.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving event {EventType}", typeof(T).Name);
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetEventsAsync<T>(DateTime? desde = null) where T : CreditoEvent
    {
        try
        {
            using var session = _documentStore.QuerySession();
            var query = session.Query<T>().AsQueryable();

            if (desde.HasValue)
            {
                query = query.Where(e => e.Timestamp >= desde.Value);
            }

            var events = query.ToList();
            
            _logger.LogInformation("Retrieved {Count} events of type {EventType}", 
                events.Count, typeof(T).Name);
            
            return await Task.FromResult(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events of type {EventType}", typeof(T).Name);
            throw;
        }
    }

    public async Task<IEnumerable<CreditoEvent>> GetEventsByAggregateIdAsync(string aggregateId)
    {
        try
        {
            using var session = _documentStore.QuerySession();
            var events = await session.Events.FetchStreamAsync(Guid.Parse(aggregateId));
            
            _logger.LogInformation("Retrieved {Count} events for aggregate {AggregateId}", 
                events.Count, aggregateId);
            
            return events.Select(e => (CreditoEvent)e.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for aggregate {AggregateId}", aggregateId);
            throw;
        }
    }
}
