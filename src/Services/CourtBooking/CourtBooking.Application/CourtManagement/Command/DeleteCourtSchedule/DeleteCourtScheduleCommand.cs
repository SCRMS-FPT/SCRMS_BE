using MediatR;

namespace CourtBooking.Application.CourtManagement.Command.DeleteCourtSchedule;

public record DeleteCourtScheduleCommand(Guid CourtScheduleId) : IRequest<DeleteCourtScheduleResult>;

public record DeleteCourtScheduleResult(bool IsSuccess);
