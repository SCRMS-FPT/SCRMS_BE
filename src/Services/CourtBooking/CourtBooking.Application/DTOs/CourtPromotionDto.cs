using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record CourtPromotionDto(
    Guid Id,
    Guid CourtId,
    string Description,
    string DiscountType,
    decimal DiscountValue,
    DateTime ValidFrom,
    DateTime ValidTo,
    DateTime CreatedAt,
    DateTime? LastModified);
}