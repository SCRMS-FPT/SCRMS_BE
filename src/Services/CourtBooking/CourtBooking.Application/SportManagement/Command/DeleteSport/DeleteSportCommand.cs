using MediatR;

namespace CourtBooking.Application.SportManagement.Commands.DeleteSport;

public record DeleteSportCommand(Guid SportId) : IRequest<DeleteSportResult>;

public record DeleteSportResult(bool IsSuccess, string Message);
