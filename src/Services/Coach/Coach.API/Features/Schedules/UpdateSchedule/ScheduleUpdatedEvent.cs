namespace Coach.API.Features.Schedules.UpdateSchedule
{
    public record ScheduleUpdatedEvent(Guid ScheduleId, Guid CoachId) : INotification;
}
