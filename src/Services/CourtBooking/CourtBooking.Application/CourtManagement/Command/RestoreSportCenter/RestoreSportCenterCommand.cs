using BuildingBlocks.CQRS;

namespace CourtBooking.Application.CourtManagement.Command.RestoreSportCenter;

public record RestoreSportCenterCommand(Guid SportCenterId) : ICommand<RestoreSportCenterResult>;

public record RestoreSportCenterResult(bool Success, string Message);
