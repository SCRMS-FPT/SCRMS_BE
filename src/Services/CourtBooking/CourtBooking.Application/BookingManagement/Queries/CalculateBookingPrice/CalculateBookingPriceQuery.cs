using CourtBooking.Application.DTOs;
using MediatR;

namespace CourtBooking.Application.BookingManagement.Queries.CalculateBookingPrice
{
    public record CalculateBookingPriceQuery(Guid UserId, BookingCreateDTO Booking) : IRequest<CalculateBookingPriceResult>;

    public record CalculateBookingPriceResult(
        List<CourtPriceDetailDTO> CourtPrices,
        decimal TotalPrice,
        decimal MinimumDeposit);

    public record CourtPriceDetailDTO(
        Guid CourtId,
        string CourtName,
        TimeSpan StartTime,
        TimeSpan EndTime,
        decimal OriginalPrice,
        decimal DiscountedPrice,
        string? PromotionName,
        string? DiscountType,
        decimal? DiscountValue);
}