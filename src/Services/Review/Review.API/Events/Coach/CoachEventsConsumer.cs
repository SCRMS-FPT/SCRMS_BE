using MassTransit;
using Reviews.API.Cache;

namespace Reviews.API.Events.Coach;

public class CoachEventsConsumer :
    IConsumer<CoachCreatedEvent>,
    IConsumer<CoachDeletedEvent>
{
    private readonly ISubjectCache _cache;
    private readonly ILogger<CoachEventsConsumer> _logger;

    public CoachEventsConsumer(ISubjectCache cache, ILogger<CoachEventsConsumer> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CoachCreatedEvent> context)
    {
        var key = $"coach_{context.Message.CoachId}";
        _cache.Set(key, true);
        _logger.LogInformation("Updated cache for coach {CoachId}", context.Message.CoachId);
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<CoachDeletedEvent> context)
    {
        var key = $"coach_{context.Message.CoachId}";
        _cache.Remove(key);
        _logger.LogInformation("Removed cache for coach {CoachId}", context.Message.CoachId);
        return Task.CompletedTask;
    }
}