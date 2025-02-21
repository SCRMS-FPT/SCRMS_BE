namespace Coach.API.Exceptions
{
    public class InvalidBookingTimeException : Exception
    {
        public InvalidBookingTimeException(string message) : base(message)
        {
        }

        public InvalidBookingTimeException(string message, string details) : base(message)
        {
            Details = details;
        }

        public string? Details { get; }
    }
}