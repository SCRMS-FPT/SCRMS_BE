using CourtBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Command.UpdateCourtPromotion
{
    public record UpdateCourtPromotionCommand(
        Guid PromotionId,
        string Description,
        string DiscountType,
        decimal DiscountValue,
        DateTime ValidFrom,
        DateTime ValidTo,
        Guid UserId) : IRequest<CourtPromotionDTO>;
}