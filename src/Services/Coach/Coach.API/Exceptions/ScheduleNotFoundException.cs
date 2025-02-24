namespace Coach.API.Exceptions
{
    public class ScheduleNotFoundException : Exception
    {
        public ScheduleNotFoundException(Guid scheduleId)
            : base($"Schedule with ID {scheduleId} not found.") { }
    }
}
