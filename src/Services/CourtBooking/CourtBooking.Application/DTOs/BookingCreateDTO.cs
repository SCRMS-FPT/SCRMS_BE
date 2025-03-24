namespace CourtBooking.Application.DTOs
{
    public record BookingCreateDTO(
        Guid UserId,
        DateTime BookingDate,
        string? Note,
        decimal DepositAmount,
        List<BookingDetailCreateDTO> BookingDetails
    );
}
