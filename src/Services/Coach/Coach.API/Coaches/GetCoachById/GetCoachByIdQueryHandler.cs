using Coach.API.Coaches.GetCoaches;
using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Coaches.GetCoachById
{
    public record GetCoachByIdQuery(Guid Id) : IQuery<CoachResponse>;

    // Handler
    internal class GetCoachByIdQueryHandler(CoachDbContext context)
        : IQueryHandler<GetCoachByIdQuery, CoachResponse>
    {
        public async Task<CoachResponse> Handle(GetCoachByIdQuery query, CancellationToken cancellationToken)
        {
            var coach = await context.Coaches
                .FirstOrDefaultAsync(c => c.UserId == query.Id, cancellationToken);

            return coach is null
                ? throw new CoachNotFoundException(query.Id)
                : new CoachResponse(
                    coach.UserId,
                    coach.SportId,
                    coach.Bio,
                    coach.RatePerHour,
                    coach.CreatedAt);
        }
    }
}