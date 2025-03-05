using CourtBooking.Application.Exceptions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Command.DeleteCourt
{
    public class DeleteCourtHandler(IApplicationDbContext context)
        : ICommandHandler<DeleteCourtCommand, DeleteCourtResult>
    {
        public async Task<DeleteCourtResult> Handle(DeleteCourtCommand command, CancellationToken cancellationToken)
        {
           var courtId = CourtId.Of(command.CourtId);
            var court = await context.Courts
               .FirstOrDefaultAsync(o => o.Id == courtId, cancellationToken);

            if (court == null)
            {
                throw new CourtNotFoundException(command.CourtId);
            }
            context.Courts.Remove(court);
            await context.SaveChangesAsync(cancellationToken);

            return new DeleteCourtResult(true);
        }
    }
}
