namespace Coach.API.Coaches.UpdateCoach
{
    public record UpdateCoachRequest(Guid SportId, string Bio, decimal RatePerHour);
}
