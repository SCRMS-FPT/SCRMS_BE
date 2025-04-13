using Microsoft.EntityFrameworkCore;
using Coach.API.Data;
using Microsoft.AspNetCore.Mvc;
using Coach.API.Data.Repositories;
using BuildingBlocks.Pagination;
using BuildingBlocks.CQRS;
using MediatR;

namespace Coach.API.Features.Coaches.GetCoaches
{
    // In GetCoachesHandler.cs, update the query record
    public record GetCoachesQuery(
        string? Name = null,
        Guid? SportId = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        int PageIndex = 0,
        int PageSize = 10,
        Guid? CurrentUserId = null
    ) : IQuery<PaginatedResult<CoachResponse>>, IRequest<PaginatedResult<CoachResponse>>;
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
    public class GetCoachesQueryHandler : IQueryHandler<GetCoachesQuery, PaginatedResult<CoachResponse>>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly ICoachSportRepository _sportRepository;
        private readonly ICoachPackageRepository _packageRepository;
        private readonly ICoachScheduleRepository _scheduleRepository;

        public GetCoachesQueryHandler(
            ICoachRepository coachRepository,
            ICoachSportRepository sportRepository,
            ICoachPackageRepository packageRepository,
            ICoachScheduleRepository scheduleRepository)
        {
            _coachRepository = coachRepository;
            _sportRepository = sportRepository;
            _packageRepository = packageRepository;
            _scheduleRepository = scheduleRepository;
        }

        public async Task<PaginatedResult<CoachResponse>> Handle(GetCoachesQuery request, CancellationToken cancellationToken)
        {
            // Get all coaches first (we will filter in memory)
            var coaches = await _coachRepository.GetAllCoachesAsync(cancellationToken);
            // Coaches đã được lọc theo status = "active" trong repository

            // Loại bỏ coach hiện tại nếu người dùng hiện tại là coach
            if (request.CurrentUserId.HasValue)
            {
                coaches = coaches.Where(c => c.UserId != request.CurrentUserId.Value).ToList();
            }

            // Apply name filter if provided
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                coaches = coaches.Where(c => c.FullName.Contains(request.Name, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Apply price range filters if provided
            if (request.MinPrice.HasValue)
            {
                coaches = coaches.Where(c => c.RatePerHour >= request.MinPrice.Value).ToList();
            }

            if (request.MaxPrice.HasValue)
            {
                coaches = coaches.Where(c => c.RatePerHour <= request.MaxPrice.Value).ToList();
            }

            // For sport filtering, we need to fetch coach sports first
            List<Data.Models.Coach> filteredCoaches = coaches;

            if (request.SportId.HasValue)
            {
                var coachesWithSport = await _sportRepository.GetCoachesBySportIdAsync(request.SportId.Value, cancellationToken);
                var coachIdsWithSport = coachesWithSport.Select(cs => cs.CoachId).ToHashSet();
                filteredCoaches = coaches.Where(c => coachIdsWithSport.Contains(c.UserId)).ToList();
            }

            // Get total count for pagination
            int totalCount = filteredCoaches.Count;

            // Apply pagination
            var paginatedCoaches = filteredCoaches
                .Skip(request.PageIndex * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map coaches to response objects
            var responses = new List<CoachResponse>();
            foreach (var coach in paginatedCoaches)
            {
                var sports = await _sportRepository.GetCoachSportsByCoachIdAsync(coach.UserId, cancellationToken);
                var packages = await _packageRepository.GetCoachPackagesByCoachIdAsync(coach.UserId, cancellationToken);
                var schedules = await _scheduleRepository.GetCoachSchedulesByCoachIdAsync(coach.UserId, cancellationToken);

                var sportIds = sports.Select(s => s.SportId).ToList();
                var packageResponses = packages.Select(p => new CoachPackageResponse(
                    p.Id, p.Name, p.Description, p.Price, p.SessionCount)).ToList();

                // Map schedules to weekly schedule response
                var weeklySchedules = schedules.Select(s => new CoachWeeklyScheduleResponse(
                    s.DayOfWeek,
                    GetDayName(s.DayOfWeek),
                    s.StartTime.ToString(@"hh\:mm\:ss"),
                    s.EndTime.ToString(@"hh\:mm\:ss"),
                    s.Id
                )).ToList();

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
                    packageResponses,
                    weeklySchedules));
            }

            // Return paginated result - use the correct property names
            return new PaginatedResult<CoachResponse>(
                request.PageIndex,
                request.PageSize,
                totalCount,
                responses);
        }

        private string GetDayName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                5 => "Friday",
                6 => "Saturday",
                7 => "Sunday",
                _ => "Unknown"
            };
        }
    }
}