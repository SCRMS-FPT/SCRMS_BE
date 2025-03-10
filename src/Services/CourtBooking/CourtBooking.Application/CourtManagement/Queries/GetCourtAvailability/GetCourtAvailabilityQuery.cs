public record GetCourtAvailabilityQuery(Guid CourtId, DateTime StartTime, DateTime EndTime) : IQuery<GetCourtAvailabilityResult>;

public record CourtTimeSlot(DateTime DateTime, bool IsAvailable);
public record GetCourtAvailabilityResult(List<CourtTimeSlot> TimeSlots);