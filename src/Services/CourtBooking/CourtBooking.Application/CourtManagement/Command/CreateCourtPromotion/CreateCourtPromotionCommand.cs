using CourtBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Command.CreateCourtPromotion
{
    public record CreateCourtPromotionCommand(
           Guid CourtId,
           string Description,
           string DiscountType,
           decimal DiscountValue,
           DateTime ValidFrom,
           DateTime ValidTo,
           Guid UserId) : IRequest<CourtPromotionDto>;
}