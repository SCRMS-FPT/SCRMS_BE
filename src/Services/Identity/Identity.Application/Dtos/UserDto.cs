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
    DateTime CreatedAt);
}