using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.SportManagement.Queries.GetSportById
{
    public class GetSportByIdHandler : IRequestHandler<GetSportByIdQuery, SportDTO>
    {
        private readonly ISportRepository _sportRepository;

        public GetSportByIdHandler(ISportRepository sportRepository)
        {
            _sportRepository = sportRepository;
        }

        public async Task<SportDTO> Handle(GetSportByIdQuery request, CancellationToken cancellationToken)
        {
            var sport = await _sportRepository.GetByIdAsync(SportId.Of(request.SportId), cancellationToken);
            if (sport == null)
                return null;
            return new SportDTO(
                sport.Id.Value,
                sport.Name,
                sport.Description,
                sport.Icon);
        }
    }
}