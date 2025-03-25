using System;

namespace CourtBooking.Application.DTOs
{
    public record CourtPromotionDTO(
        Guid Id,
        Guid CourtId,
        string Description,
        string DiscountType,
        decimal DiscountValue,
        DateTime ValidFrom,
        DateTime ValidTo,
        DateTime CreatedAt,
        DateTime? LastModified
    );
}