namespace Identity.Application.Dtos
{
    public record ServicePackagePromotionDto(
        Guid Id,
        Guid ServicePackageId,
        string Description,
        string Type,
        decimal Value,
        DateTime ValidFrom,
        DateTime ValidTo,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
