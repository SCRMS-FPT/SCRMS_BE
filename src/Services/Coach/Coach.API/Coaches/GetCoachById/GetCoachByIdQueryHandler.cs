using Coach.API.Coaches.GetCoaches;
using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Coaches.GetCoachById
{
    public record GetCoachByIdQuery(Guid Id) : IQuery<CoachResponse>;

    internal class GetCoachByIdQueryHandler(CoachDbContext context)
        : IQueryHandler<GetCoachByIdQuery, CoachResponse>
    {
        public async Task<CoachResponse> Handle(GetCoachByIdQuery query, CancellationToken cancellationToken)
        {
            var coach = await context.Coaches
                .Include(c => c.CoachSports)
                .Include(c => c.Packages)
                .FirstOrDefaultAsync(c => c.UserId == query.Id, cancellationToken);

            if (coach is null)
                throw new CoachNotFoundException(query.Id);

            var sportIds = coach.CoachSports.Select(cs => cs.SportId).ToList();

            var packages = coach.Packages.Select(p => new CoachPackageResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.SessionCount
            )).ToList();

            return new CoachResponse(
                coach.UserId,
                sportIds,
                coach.Bio,
                coach.RatePerHour,
                coach.CreatedAt,
                packages);
        }
    }
}