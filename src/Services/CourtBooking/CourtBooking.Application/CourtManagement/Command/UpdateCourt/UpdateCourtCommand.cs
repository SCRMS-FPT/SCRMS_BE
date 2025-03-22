using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.CourtManagement.Command.UpdateCourt;

public record UpdateCourtCommand(Guid Id, CourtUpdateDTO Court) : IRequest<UpdateCourtResult>;

public record UpdateCourtResult(bool IsSuccess);