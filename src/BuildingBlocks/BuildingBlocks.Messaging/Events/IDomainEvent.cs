using System;

namespace BuildingBlocks.Messaging.Events
{
    /// <summary>
    /// Interface đại diện cho một Domain Event
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// ID của sự kiện
        /// </summary>
        Guid Id { get; }
        
        /// <summary>
        /// Thời gian sự kiện xảy ra
        /// </summary>
        DateTime OccurredOn { get; }
    }
} 