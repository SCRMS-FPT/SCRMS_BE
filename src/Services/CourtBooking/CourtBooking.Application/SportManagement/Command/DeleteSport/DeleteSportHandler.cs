using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CourtBooking.Application.SportManagement.Commands.DeleteSport;

public class DeleteSportHandler(IApplicationDbContext _context)
    : IRequestHandler<DeleteSportCommand, DeleteSportResult>
{

    public async Task<DeleteSportResult> Handle(DeleteSportCommand request, CancellationToken cancellationToken)
    {
        var sportId = SportId.Of(request.SportId);
        var sport = await _context.Sports.FindAsync(new object[] { sportId }, cancellationToken);
        if (sport == null)
        {
            return new DeleteSportResult(false, "Sport not found");
        }

        var isSportInUse = await _context.Courts.AnyAsync(c => c.SportId == sportId, cancellationToken);
        if (isSportInUse)
        {
            return new DeleteSportResult(false, "Cannot delete sport as it is associated with one or more courts");
        }

        _context.Sports.Remove(sport);
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteSportResult(true, "Sport deleted successfully");
    }
}
