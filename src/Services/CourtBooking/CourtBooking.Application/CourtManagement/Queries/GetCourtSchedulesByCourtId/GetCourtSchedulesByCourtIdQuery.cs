using CourtBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtSchedulesByCourtId
{
        public record GetCourtSchedulesByCourtIdQuery(Guid CourtId) : IRequest<GetCourtSchedulesByCourtIdResult>;

        public record GetCourtSchedulesByCourtIdResult(List<CourtScheduleDTO> CourtSchedules);
}
