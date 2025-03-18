using CourtBooking.Application.DTOs;
using MediatR;

namespace CourtBooking.Application.CourtManagement.Command.UpdateCourtSchedule;

public record UpdateCourtScheduleCommand(CourtScheduleUpdateDTO CourtSchedule) : IRequest<UpdateCourtScheduleResult>;

public record UpdateCourtScheduleResult(bool IsSuccess);