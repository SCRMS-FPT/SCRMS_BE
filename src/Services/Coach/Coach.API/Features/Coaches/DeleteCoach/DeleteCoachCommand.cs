namespace Coach.API.Features.Coaches.DeleteCoach
{
    public record DeleteCoachCommand(Guid CoachId) : ICommand<bool>;
}