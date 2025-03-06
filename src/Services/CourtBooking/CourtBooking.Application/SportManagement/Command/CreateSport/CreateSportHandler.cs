using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CourtBooking.Application.SportManagement.Commands.CreateSport;

public class CreateSportHandler(IApplicationDbContext _context)
    : IRequestHandler<CreateSportCommand, CreateSportResult>
{

    public async Task<CreateSportResult> Handle(CreateSportCommand request, CancellationToken cancellationToken)
    {
        var newSportId = SportId.Of(Guid.NewGuid());
        var sport = Sport.Create(newSportId, request.Name, request.Description, request.Icon);

        _context.Sports.Add(sport);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateSportResult(sport.Id.Value);
    }
}
