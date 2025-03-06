using BuildingBlocks.Messaging.Events;

namespace Reviews.API.Events.Court;
public record CourtCreatedEvent(Guid CourtId, DateTime CreatedAt) : IntegrationEvent;

public record CourtDeletedEvent(Guid CourtId, DateTime DeletedAt) : IntegrationEvent;