using CourtBooking.Application.DTOs;
using MediatR;

namespace CourtBooking.Application.CourtManagement.Command.CreateCourtSchedule;

public record CreateCourtScheduleCommand(CourtScheduleDTO CourtSchedule) : IRequest<CreateCourtScheduleResult>;

public record CreateCourtScheduleResult(Guid Id);
