using CourtBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtPromotions
{
    public record GetCourtPromotionsQuery(Guid CourtId, Guid UserId, string Role) : IRequest<List<CourtPromotionDTO>>;
}