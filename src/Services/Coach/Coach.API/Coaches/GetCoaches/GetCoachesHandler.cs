using Microsoft.EntityFrameworkCore;
using Coach.API.Data;

namespace Coach.API.Coaches.GetCoaches
{
    public record GetCoachesQuery : IQuery<IEnumerable<CoachResponse>>;

    public record CoachResponse(
        Guid UserId,
        Guid SportId,
        string Bio,
        decimal RatePerHour,
        DateTime CreatedAt);

    // Handler
    internal class GetCoachesQueryHandler(CoachDbContext context)
        : IQueryHandler<GetCoachesQuery, IEnumerable<CoachResponse>>
    {
        public async Task<IEnumerable<CoachResponse>> Handle(GetCoachesQuery request, CancellationToken cancellationToken)
        {
            return await context.Coaches
                .Select(c => new CoachResponse(
                    c.UserId,
                    c.SportId,
                    c.Bio,
                    c.RatePerHour,
                    c.CreatedAt))
                .ToListAsync(cancellationToken);
        }
    }
}