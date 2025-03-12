namespace CourtBooking.Application.DTOs
{
    public record BookingCreateDTO(
        Guid UserId,
        //Guid CourtId,
        DateTime BookingDate,
        decimal TotalPrice,
        string? Note,
        Guid? PromotionId,
        //list booking details
        List<BookingDetailCreateDTO> BookingDetails
    );
}
