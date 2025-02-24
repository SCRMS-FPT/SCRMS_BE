namespace Coach.API.Exceptions
{
    public class ScheduleConflictException : Exception
    {
        public ScheduleConflictException(string message) : base(message) { }
    }
}
