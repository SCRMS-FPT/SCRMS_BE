namespace Coach.API.Exceptions
{
    public class BookingConflictException : Exception
    {
        public BookingConflictException(string message) : base(message)
        {
        }

        public BookingConflictException(string message, string details) : base(message)
        {
            Details = details;
        }

        public string? Details { get; }
    }
}