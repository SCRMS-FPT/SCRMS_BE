using CourtBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.SportManagement.Queries.GetSportById
{
    public record GetSportByIdQuery(Guid SportId) : IRequest<SportDTO>;
}