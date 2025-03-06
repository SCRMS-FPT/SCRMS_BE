
namespace CourtBooking.Application.SportManagement.Commands.CreateSport;

public record CreateSportCommand(string Name, string Description, string Icon) : IRequest<CreateSportResult>;

public record CreateSportResult(Guid Id);
