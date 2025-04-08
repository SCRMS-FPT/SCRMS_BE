using BuildingBlocks.CQRS;

namespace CourtBooking.Application.CourtManagement.Command.SoftDeleteSportCenter;

public record SoftDeleteSportCenterCommand(Guid SportCenterId) : ICommand<SoftDeleteSportCenterResult>;

public record SoftDeleteSportCenterResult(bool Success);