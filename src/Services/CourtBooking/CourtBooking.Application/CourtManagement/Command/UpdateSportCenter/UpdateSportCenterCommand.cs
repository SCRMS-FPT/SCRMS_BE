using CourtBooking.Application.DTOs;
using FluentValidation;

namespace CourtBooking.Application.CourtManagement.Command.UpdateSportCenter;

public record UpdateSportCenterCommand(
    Guid SportCenterId,
    string Name,
    string PhoneNumber,
    string AddressLine,
    string City,
    string District,
    string Commune,
    double Latitude,
    double Longitude,
    string Avatar,
    List<string> ImageUrls,
    string Description
) : ICommand<UpdateSportCenterResult>;

public record UpdateSportCenterResult(SportCenterDetailDTO SportCenter);

public class UpdateSportCenterCommandValidator : AbstractValidator<UpdateSportCenterCommand>
{
    public UpdateSportCenterCommandValidator()
    {
        RuleFor(x => x.SportCenterId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^\+?[0-9\s\-\(\)]+$");
        RuleFor(x => x.AddressLine).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Commune).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.Avatar).NotEmpty();
        RuleFor(x => x.ImageUrls).NotNull();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}