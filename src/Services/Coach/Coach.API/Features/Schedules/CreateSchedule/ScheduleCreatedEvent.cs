namespace Coach.API.Features.Schedules.CreateSchedule
{
    public record ScheduleCreatedEvent(Guid ScheduleId, Guid CoachId) : INotification;

}
