using MediatR;
using Microsoft.EntityFrameworkCore;
using CourtBooking.Application.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Application.Data.Repositories;

namespace CourtBooking.Application.SportManagement.Queries.GetSports;

public class GetSportsHandler : IRequestHandler<GetSportsQuery, GetSportsResult>
{
    private readonly ISportRepository _sportRepository;

    public GetSportsHandler(ISportRepository sportRepository)
    {
        _sportRepository = sportRepository;
    }

    public async Task<GetSportsResult> Handle(GetSportsQuery request, CancellationToken cancellationToken)
    {
        var sports = await _sportRepository.GetAllSportsAsync(cancellationToken);
        var sportDtos = sports.Select(s => new SportDTO(
            Id: s.Id.Value,
            Name: s.Name,
            Description: s.Description,
            Icon: s.Icon
        )).ToList();

        return new GetSportsResult(sportDtos);
    }
}