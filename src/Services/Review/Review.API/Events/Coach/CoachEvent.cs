using BuildingBlocks.Messaging.Events;

namespace Reviews.API.Events.Coach;
public record CoachCreatedEvent(Guid CoachId, DateTime CreatedAt) : IntegrationEvent;

public record CoachDeletedEvent(Guid CoachId, DateTime DeletedAt) : IntegrationEvent;