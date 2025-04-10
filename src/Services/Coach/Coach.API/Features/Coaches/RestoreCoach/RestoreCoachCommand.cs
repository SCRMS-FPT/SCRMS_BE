namespace Coach.API.Features.Coaches.RestoreCoach
{
    public record RestoreCoachCommand(Guid CoachId) : ICommand<bool>;
}