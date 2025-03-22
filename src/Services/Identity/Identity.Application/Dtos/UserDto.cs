namespace Identity.Application.Dtos
{
    public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime BirthDate,
    string Gender,
    string SelfIntroduction,
    DateTime CreatedAt,
    List<string>? Roles = null);

    public record UserProfileDto(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string Phone,
        DateTime BirthDate,
        string Gender,
        string SelfIntroduction);
}