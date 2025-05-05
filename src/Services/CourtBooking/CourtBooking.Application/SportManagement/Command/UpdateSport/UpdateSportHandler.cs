using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CourtBooking.Application.SportManagement.Commands.UpdateSport;

public class UpdateSportHandler : IRequestHandler<UpdateSportCommand, UpdateSportResult>
{
    private readonly ISportRepository _sportRepository;

    public UpdateSportHandler(ISportRepository sportRepository)
    {
        _sportRepository = sportRepository;
    }

    public async Task<UpdateSportResult> Handle(UpdateSportCommand request, CancellationToken cancellationToken)
    {
        var sportId = SportId.Of(request.Id);
        var sport = await _sportRepository.GetSportByIdAsync(sportId, cancellationToken);
        if (sport == null)
        {
            throw new KeyNotFoundException("Sport not found");
        }
        var isExist = await _sportRepository.GetByName(request.Name, cancellationToken);
        if (isExist != null)
        {
            throw new ApplicationException("Duplicate was found.");
        }

        sport.Update(request.Name, request.Description, request.Icon);
        await _sportRepository.UpdateSportAsync(sport, cancellationToken);
        return new UpdateSportResult(true);
    }
}