using Microsoft.EntityFrameworkCore;
using Coach.API.Data;

namespace Coach.API.Coaches.GetCoaches
{
    public record GetCoachesQuery : IQuery<IEnumerable<CoachResponse>>;

    public record CoachResponse(
        Guid UserId,
        List<Guid> SportIds,
        string Bio,
        decimal RatePerHour,
        DateTime CreatedAt,
        List<CoachPackageResponse> Packages);

    public record CoachPackageResponse(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        int SessionCount);

    // Handler
    internal class GetCoachesQueryHandler(CoachDbContext context)
    : IQueryHandler<GetCoachesQuery, IEnumerable<CoachResponse>>
    {
        public async Task<IEnumerable<CoachResponse>> Handle(
            GetCoachesQuery request,
            CancellationToken cancellationToken)
        {
            return await context.Coaches
                .Include(c => c.CoachSports)
                .Include(c => c.Packages)
                .Select(c => new CoachResponse(
                    c.UserId,
                    c.CoachSports.Select(s => s.SportId).ToList(),
                    c.Bio,
                    c.RatePerHour,
                    c.CreatedAt,
                    c.Packages.Select(p => new CoachPackageResponse(
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Price,
                        p.SessionCount
                    )).ToList()
                ))
                .ToListAsync(cancellationToken);
        }
    }
}