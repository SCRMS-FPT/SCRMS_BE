using BuildingBlocks.CQRS;

namespace CourtBooking.Application.CourtManagement.Command.DeleteSportCenter;

public record DeleteSportCenterCommand(Guid SportCenterId) : ICommand<DeleteSportCenterResult>;

public record DeleteSportCenterResult(bool Success);