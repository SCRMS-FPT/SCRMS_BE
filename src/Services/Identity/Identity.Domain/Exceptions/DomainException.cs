using System;

namespace Identity.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public int ErrorCode { get; set; } // Optional: error code can be useful
        public string? Detail { get; set; } // Optional: provide additional details for the exception

        // Default constructor
        public DomainException(string message) : base(message)
        {
        }

        // Constructor with inner exception
        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        // Constructor with error code and optional additional details
        public DomainException(string message, int errorCode, string? detail = null)
            : base(message)
        {
            ErrorCode = errorCode;
            Detail = detail;
        }

        // You can override ToString to include additional details if you wish
        public override string ToString()
        {
            return $"{base.ToString()}, ErrorCode: {ErrorCode}, Detail: {Detail}";
        }
    }
}
