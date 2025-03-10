using CourtBooking.Application.DTOs;
using FluentValidation;
using MediatR;

namespace CourtBooking.Application.SportCenterManagement.Commands.CreateSportCenter;
public record CreateSportCenterCommand(
    Guid Id,
    Guid OwnerId,
    string Name,
    string Description,
    LocationDTO Location,
    string PhoneNumber,
    GeoLocation LocationPoint,
    SportCenterImages Images,
    IEnumerable<CourtDTO> Courts
) : ICommand<CreateSportCenterResult>;

public record CreateSportCenterResult(Guid Id);

public class CreateSportCenterCommandValidator : AbstractValidator<CreateSportCenterCommand>
{
    public CreateSportCenterCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required");

        //RuleFor(x => x.Name)
        //    .NotEmpty().WithMessage("Name is required")
        //    .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        //RuleFor(x => x.Description)
        //    .NotEmpty().WithMessage("Description is required")
        //    .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        //RuleFor(x => x.Location)
        //    .NotNull().WithMessage("Location is required");

        //When(x => x.Location != null, () =>
        //{
        //    RuleFor(x => x.Location.AddressLine)
        //        .NotEmpty().WithMessage("Address is required")
        //        .MaximumLength(200).WithMessage("Address cannot exceed 200 characters");

        //    RuleFor(x => x.Location.Commune)
        //        .NotEmpty().WithMessage("Commune is required")
        //        .MaximumLength(100).WithMessage("Commune cannot exceed 100 characters");

        //    RuleFor(x => x.Location.District)
        //        .NotEmpty().WithMessage("District is required")
        //        .MaximumLength(100).WithMessage("District cannot exceed 100 characters");
        //    RuleFor(x => x.Location.City)
        //        .NotEmpty().WithMessage("City is required")
        //        .MaximumLength(100).WithMessage("City cannot exceed 100 characters");
        //});

        //RuleFor(x => x.PhoneNumber)
        //    .NotEmpty().WithMessage("Phone number is required")
        //    .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Phone number format is invalid");

        //RuleFor(x => x.LocationPoint)
        //    .NotNull().WithMessage("Location point is required");

        //When(x => x.LocationPoint != null, () =>
        //{
        //    RuleFor(x => x.LocationPoint.Latitude)
        //        .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

        //    RuleFor(x => x.LocationPoint.Longitude)
        //        .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");
        //});

        //RuleFor(x => x.Images)
        //    .NotNull().WithMessage("Images are required");

        //// Note: There's a duplicate description field (lowercase) in the command
        //// Consider removing it or adding validation if it's needed

        //RuleFor(x => x.Courts)
        //    .NotNull().WithMessage("Courts collection cannot be null")
        //    .Must(courts => courts.Any()).WithMessage("At least one court must be added");

        ////RuleForEach(x => x.Courts).SetValidator(new CourtDTOValidator());
    }
}

