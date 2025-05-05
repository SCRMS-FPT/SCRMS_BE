using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.Exceptions;
using BuildingBlocks.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using CourtBooking.Domain.Enums;

namespace CourtBooking.Application.CourtManagement.Command.DeleteCourt
{
    public class DeleteCourtHandler : ICommandHandler<DeleteCourtCommand, DeleteCourtResult>
    {
        private readonly ICourtRepository _courtRepository;
        private readonly IBookingRepository _bookingRepository;

        public DeleteCourtHandler(ICourtRepository courtRepository, IBookingRepository bookingRepository)
        {
            _courtRepository = courtRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<DeleteCourtResult> Handle(DeleteCourtCommand command, CancellationToken cancellationToken)
        {
            var courtId = CourtId.Of(command.CourtId);
            var court = await _courtRepository.GetCourtByIdAsync(courtId, cancellationToken);
            if (court == null)
            {
                throw new CourtNotFoundException(command.CourtId);
            }

            // Check for active bookings
            var activeBookings = await _bookingRepository.GetActiveBookingsForCourtAsync(
                courtId,
                new[] { BookingStatus.Deposited, BookingStatus.Completed },
                DateTime.UtcNow,
                cancellationToken);

            if (activeBookings.Any())
            {
                throw new ValidationException("Không thể xóa sân vì có lịch đặt sân còn hoạt động.");
            }

            await _courtRepository.DeleteCourtAsync(courtId, cancellationToken);
            return new DeleteCourtResult(true);
        }
    }
}