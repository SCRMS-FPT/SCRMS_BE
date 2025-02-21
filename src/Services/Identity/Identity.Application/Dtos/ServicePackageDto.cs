namespace Identity.Application.Dtos
{
    public record ServicePackageDto(
        int Id,
        string Name,
        string Description,
        decimal Price,
        int DurationDays,
        DateTime CreatedAt,
        int TotalSubscriptions
    );
}