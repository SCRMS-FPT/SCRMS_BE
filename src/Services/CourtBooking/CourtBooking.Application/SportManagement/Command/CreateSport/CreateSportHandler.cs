using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Application.Data.Repositories;

namespace CourtBooking.Application.SportManagement.Commands.CreateSport;

public class CreateSportHandler : IRequestHandler<CreateSportCommand, CreateSportResult>
{
    private readonly ISportRepository _sportRepository;

    public CreateSportHandler(ISportRepository sportRepository)
    {
        _sportRepository = sportRepository;
    }

    public async Task<CreateSportResult> Handle(CreateSportCommand request, CancellationToken cancellationToken)
    {
        var isExist = await _sportRepository.GetByName(request.Name, cancellationToken);
        if (isExist != null)
        {
            throw new ApplicationException("Duplicate was found.");
        }

        var newSportId = SportId.Of(Guid.NewGuid());
        var sport = Sport.Create(newSportId, request.Name, request.Description, request.Icon);

        await _sportRepository.AddSportAsync(sport, cancellationToken);
        return new CreateSportResult(sport.Id.Value);
    }
}