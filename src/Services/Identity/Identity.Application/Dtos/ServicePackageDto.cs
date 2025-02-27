namespace Identity.Application.Dtos
{
    public record ServicePackageDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int DurationDays,
    string AssociatedRole,
    DateTime CreatedAt,
    string Status
    );
}