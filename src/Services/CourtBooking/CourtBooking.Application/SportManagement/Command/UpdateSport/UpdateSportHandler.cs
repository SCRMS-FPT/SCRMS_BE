using CourtBooking.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CourtBooking.Application.SportManagement.Commands.UpdateSport;

public class UpdateSportHandler(IApplicationDbContext _context)
    : IRequestHandler<UpdateSportCommand, UpdateSportResult>
{
    public async Task<UpdateSportResult> Handle(UpdateSportCommand request, CancellationToken cancellationToken)
    {
        var sportId = SportId.Of(request.Id);
        var sport = await _context.Sports.FindAsync( sportId, cancellationToken);
        if (sport == null)
        {
            throw new KeyNotFoundException("Sport not found");
        }

        sport.Update(request.Name, request.Description, request.Icon);

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateSportResult(true);
    }
}
