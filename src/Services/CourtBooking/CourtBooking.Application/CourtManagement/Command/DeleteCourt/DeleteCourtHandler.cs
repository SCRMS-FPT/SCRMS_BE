using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.Exceptions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Command.DeleteCourt
{
    public class DeleteCourtHandler : ICommandHandler<DeleteCourtCommand, DeleteCourtResult>
    {
        private readonly ICourtRepository _courtRepository;

        public DeleteCourtHandler(ICourtRepository courtRepository)
        {
            _courtRepository = courtRepository;
        }

        public async Task<DeleteCourtResult> Handle(DeleteCourtCommand command, CancellationToken cancellationToken)
        {
            var courtId = CourtId.Of(command.CourtId);
            var court = await _courtRepository.GetCourtByIdAsync(courtId, cancellationToken);
            if (court == null)
            {
                throw new CourtNotFoundException(command.CourtId);
            }

            await _courtRepository.DeleteCourtAsync(courtId, cancellationToken);
            return new DeleteCourtResult(true);
        }
    }
}