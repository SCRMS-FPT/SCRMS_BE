namespace CourtBooking.Application.DTOs
{
    public record BookingCreateDTO(
        DateTime BookingDate,
        string? Note,
        decimal DepositAmount,
        List<BookingDetailCreateDTO> BookingDetails
    );
}
