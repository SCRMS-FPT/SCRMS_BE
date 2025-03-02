using Microsoft.EntityFrameworkCore;
using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtAvailability
{
    public class GetCourtAvailabilityHandler(IApplicationDbContext _context) 
        : IQueryHandler<GetCourtAvailabilityQuery, GetCourtAvailabilityResult>
    {
        public async Task<GetCourtAvailabilityResult> Handle(GetCourtAvailabilityQuery query, CancellationToken cancellationToken)
        {
            var courtId = CourtId.Of(query.CourtId);
            var court = await _context.Courts
                //.Include(c => c.OperatingHours)
                .FirstOrDefaultAsync(c => c.Id == courtId, cancellationToken);

            if (court == null)
            {
                throw new KeyNotFoundException("Court not found");
            }

            var timeSlots = new List<CourtTimeSlot>();
            var currentTime = query.StartTime;

            while (currentTime <= query.EndTime)
            {
                //var isAvailable = court.IsAvailable(currentTime);
                //timeSlots.Add(new CourtTimeSlot(currentTime, isAvailable));
                //currentTime = currentTime.AddHours(1); // Add 1-hour intervals
            }

            return new GetCourtAvailabilityResult(timeSlots);
        }
    }
}