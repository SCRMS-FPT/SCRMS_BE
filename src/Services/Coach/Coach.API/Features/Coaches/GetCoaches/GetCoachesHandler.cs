using Microsoft.EntityFrameworkCore;
using Coach.API.Data;
using Microsoft.AspNetCore.Mvc;
using Coach.API.Data.Repositories;

namespace Coach.API.Features.Coaches.GetCoaches
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
    public class GetCoachesQueryHandler : IQueryHandler<GetCoachesQuery, IEnumerable<CoachResponse>>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly ICoachSportRepository _sportRepository;
        private readonly ICoachPackageRepository _packageRepository;

        public GetCoachesQueryHandler(
            ICoachRepository coachRepository,
            ICoachSportRepository sportRepository,
            ICoachPackageRepository packageRepository)
        {
            _coachRepository = coachRepository;
            _sportRepository = sportRepository;
            _packageRepository = packageRepository;
        }

        public async Task<IEnumerable<CoachResponse>> Handle(GetCoachesQuery request, CancellationToken cancellationToken)
        {
            var coaches = await _coachRepository.GetAllCoachesAsync(cancellationToken);
            var responses = new List<CoachResponse>();

            foreach (var coach in coaches)
            {
                var sports = await _sportRepository.GetCoachSportsByCoachIdAsync(coach.UserId, cancellationToken);
                var packages = await _packageRepository.GetCoachPackagesByCoachIdAsync(coach.UserId, cancellationToken);

                var sportIds = sports.Select(s => s.SportId).ToList();
                var packageResponses = packages.Select(p => new CoachPackageResponse(
                    p.Id, p.Name, p.Description, p.Price, p.SessionCount)).ToList();

                responses.Add(new CoachResponse(
                    coach.UserId, sportIds, coach.Bio, coach.RatePerHour, coach.CreatedAt, packageResponses));
            }

            return responses;
        }
    }
}