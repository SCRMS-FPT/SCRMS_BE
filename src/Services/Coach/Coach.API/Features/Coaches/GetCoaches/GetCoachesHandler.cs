using Microsoft.EntityFrameworkCore;
using Coach.API.Data;
using Microsoft.AspNetCore.Mvc;
using Coach.API.Data.Repositories;

namespace Coach.API.Features.Coaches.GetCoaches
{
    public record GetCoachesQuery : IQuery<IEnumerable<CoachResponse>>;

    public record CoachWeeklyScheduleResponse(
    int DayOfWeek,         // 1=Sunday to 7=Saturday
    string DayName,        // "Sunday", "Monday", etc.
    string StartTime,      // "08:00:00" format
    string EndTime,        // "17:00:00" format
    Guid ScheduleId);      // The ID of the schedule record

    // Update existing CoachResponse to include schedules
    public record CoachResponse(
        Guid Id,
        string FullName,
        string Email,
        string Phone,
        string Avatar,
        List<string> ImageUrls,
        List<Guid> SportIds,
        string Bio,
        decimal RatePerHour,
        DateTime CreatedAt,
        List<CoachPackageResponse> Packages,
        List<CoachWeeklyScheduleResponse>? WeeklySchedule = null); // New field

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
                    coach.UserId,
                    coach.FullName,
                    coach.Email,
                    coach.Phone,
                    coach.Avatar,
                    coach.GetImageUrlsList(),
                    sportIds,
                    coach.Bio,
                    coach.RatePerHour,
                    coach.CreatedAt,
                    packageResponses));
            }

            return responses;
        }
    }
}