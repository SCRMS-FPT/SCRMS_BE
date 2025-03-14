using CourtBooking.Application.Data.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CourtBooking.Application.SportManagement.Commands.DeleteSport;

public class DeleteSportHandler : IRequestHandler<DeleteSportCommand, DeleteSportResult>
{
    private readonly ISportRepository _sportRepository;

    public DeleteSportHandler(ISportRepository sportRepository)
    {
        _sportRepository = sportRepository;
    }

    public async Task<DeleteSportResult> Handle(DeleteSportCommand request, CancellationToken cancellationToken)
    {
        var sportId = SportId.Of(request.SportId);
        var sport = await _sportRepository.GetSportByIdAsync(sportId, cancellationToken);
        if (sport == null)
        {
            return new DeleteSportResult(false, "Sport not found");
        }

        var isInUse = await _sportRepository.IsSportInUseAsync(sportId, cancellationToken);
        if (isInUse)
        {
            return new DeleteSportResult(false, "Cannot delete sport as it is associated with one or more courts");
        }

        await _sportRepository.DeleteSportAsync(sportId, cancellationToken);
        return new DeleteSportResult(true, "Sport deleted successfully");
    }
}