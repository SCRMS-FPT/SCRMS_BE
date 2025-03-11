using MediatR;

namespace CourtBooking.Application.SportManagement.Commands.UpdateSport;

public record UpdateSportCommand(Guid Id, string Name, string Description, string Icon) : IRequest<UpdateSportResult>;

public record UpdateSportResult(bool IsSuccess);
