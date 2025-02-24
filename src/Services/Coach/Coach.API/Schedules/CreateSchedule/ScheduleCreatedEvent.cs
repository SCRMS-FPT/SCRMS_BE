namespace Coach.API.Schedules.CreateSchedule
{
    public record ScheduleCreatedEvent(Guid ScheduleId, Guid CoachId) : INotification;

}
