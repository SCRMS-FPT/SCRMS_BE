namespace Coach.API.Schedules.UpdateSchedule
{
    public record ScheduleUpdatedEvent(Guid ScheduleId, Guid CoachId) : INotification;
}
