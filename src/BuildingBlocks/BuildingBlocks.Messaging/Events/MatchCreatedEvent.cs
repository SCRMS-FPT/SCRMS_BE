using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events
{
    public record MatchCreatedEvent(
        Guid UserId1,
        Guid UserId2,
        DateTime MatchedAt) : IntegrationEvent;

    public record SwipeActionEvent(
        Guid SwiperId,
        Guid SwipeeId,
        bool IsLike,
        DateTime SwipeTime) : IntegrationEvent;
}