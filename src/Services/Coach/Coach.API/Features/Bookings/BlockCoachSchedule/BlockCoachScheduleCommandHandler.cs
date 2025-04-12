using Coach.API.Data;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Bookings.BlockCoachSchedule
{
    public class BlockCoachScheduleCommandHandler : ICommandHandler<BlockCoachScheduleCommand, BlockCoachScheduleResult>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly ICoachBookingRepository _bookingRepository;
        private readonly CoachDbContext _context;

        public BlockCoachScheduleCommandHandler(
            ICoachRepository coachRepository,
            ICoachBookingRepository bookingRepository,
            CoachDbContext context)
        {
            _coachRepository = coachRepository;
            _bookingRepository = bookingRepository;
            _context = context;
        }

        public async Task<BlockCoachScheduleResult> Handle(BlockCoachScheduleCommand command, CancellationToken cancellationToken)
        {
            // Verify coach exists
            var coach = await _coachRepository.GetCoachByIdAsync(command.CoachId, cancellationToken);
            if (coach == null)
                throw new Exception("Coach not found");

            // Check for overlapping bookings
            var hasOverlap = await _bookingRepository.HasOverlappingCoachBookingAsync(
                command.CoachId,
                command.BookingDate,
                command.StartTime,
                command.EndTime,
                cancellationToken);

            if (hasOverlap)
                throw new Exception("The selected time slot is already booked");

            // Calculate duration and price (will be zero as it's a self-booking)
            var duration = (command.EndTime - command.StartTime).TotalHours;

            // Create the booking
            var booking = new CoachBooking
            {
                Id = Guid.NewGuid(),
                UserId = command.CoachId, // Coach is booking themselves
                CoachId = command.CoachId,
                SportId = command.SportId,
                BookingDate = command.BookingDate,
                StartTime = command.StartTime,
                EndTime = command.EndTime,
                Status = "completed", // Mark as completed
                TotalPrice = 0, // No charge for self-booking
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddCoachBookingAsync(booking, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new BlockCoachScheduleResult(booking.Id);
        }
    }
}