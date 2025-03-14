using Microsoft.EntityFrameworkCore;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Application.Data.Repositories;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtAvailability
{
    public class GetCourtAvailabilityHandler : IQueryHandler<GetCourtAvailabilityQuery, GetCourtAvailabilityResult>
    {
        private readonly ICourtRepository _courtRepository;

        public GetCourtAvailabilityHandler(ICourtRepository courtRepository)
        {
            _courtRepository = courtRepository;
        }

        public async Task<GetCourtAvailabilityResult> Handle(GetCourtAvailabilityQuery query, CancellationToken cancellationToken)
        {
            var courtId = CourtId.Of(query.CourtId);
            var court = await _courtRepository.GetCourtByIdAsync(courtId, cancellationToken);
            if (court == null)
            {
                throw new KeyNotFoundException("Court not found");
            }

            var timeSlots = new List<CourtTimeSlot>();
            var currentTime = query.StartTime;

            while (currentTime <= query.EndTime)
            {
                // Giả sử có phương thức kiểm tra tính khả dụng trong domain model Court
                // var isAvailable = court.IsAvailable(currentTime);
                // timeSlots.Add(new CourtTimeSlot(currentTime, isAvailable));
                // currentTime = currentTime.AddHours(1); // Tăng 1 giờ
            }

            return new GetCourtAvailabilityResult(timeSlots);
        }
    }
}