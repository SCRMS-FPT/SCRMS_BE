using BuildingBlocks.Pagination;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Dashboard.GetUserDashboard
{
    public record GetUserDashboardQuery(Guid UserId) : IQuery<UserDashboardResponse>;

    public record UserDashboardResponse(
        int TotalSessions,
        List<UpcomingCoachSessionDto> UpcomingSessions
    );

    public record UpcomingCoachSessionDto(
        Guid BookingId,
        Guid CoachId,
        string CoachName,
        string CoachAvatar,
        DateOnly BookingDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string Status,
        Guid SportId,
        string? PackageName
    );

    public class GetUserDashboardQueryHandler : IQueryHandler<GetUserDashboardQuery, UserDashboardResponse>
    {
        private readonly ICoachBookingRepository _bookingRepository;
        private readonly ICoachRepository _coachRepository;
        private readonly ICoachPackageRepository _packageRepository;

        public GetUserDashboardQueryHandler(
            ICoachBookingRepository bookingRepository,
            ICoachRepository coachRepository,
            ICoachPackageRepository packageRepository)
        {
            _bookingRepository = bookingRepository;
            _coachRepository = coachRepository;
            _packageRepository = packageRepository;
        }

        public async Task<UserDashboardResponse> Handle(GetUserDashboardQuery query, CancellationToken cancellationToken)
        {
            // Get all bookings for the user
            var allBookings = await _bookingRepository.GetCoachBookingsByUserIdAsync(query.UserId, cancellationToken);

            // Calculate total sessions (only completed sessions)
            int totalSessions = allBookings.Count(b => b.Status == "completed");

            // Get upcoming sessions (pending sessions with date >= today)
            var today = DateOnly.FromDateTime(DateTime.Today);
            var upcomingBookings = allBookings
                .Where(b => b.Status == "pending" && b.BookingDate >= today)
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .Take(5) // Limit to 5 upcoming sessions
                .ToList();

            // Get coach details for upcoming sessions
            var coachIds = upcomingBookings.Select(b => b.CoachId).Distinct().ToList();
            var coaches = new Dictionary<Guid, Data.Models.Coach>();
            foreach (var coachId in coachIds)
            {
                var coach = await _coachRepository.GetCoachByIdAsync(coachId, cancellationToken);
                if (coach != null)
                {
                    coaches.Add(coachId, coach);
                }
            }

            // Get package details for upcoming sessions
            var packageIds = upcomingBookings
                .Where(b => b.PackageId.HasValue)
                .Select(b => b.PackageId!.Value)
                .Distinct()
                .ToList();

            var packages = new Dictionary<Guid, CoachPackage>();
            foreach (var packageId in packageIds)
            {
                var packageItem = await _packageRepository.GetCoachPackageByIdAsync(packageId, cancellationToken);
                if (packageItem != null)
                {
                    packages.Add(packageId, packageItem);
                }
            }

            // Map upcoming bookings to DTOs
            var upcomingSessions = upcomingBookings.Select(b => new UpcomingCoachSessionDto(
                b.Id,
                b.CoachId,
                coaches.TryGetValue(b.CoachId, out var coach) ? coach.FullName : "Unknown Coach",
                coaches.TryGetValue(b.CoachId, out var coachWithAvatar) ? coachWithAvatar.Avatar : null,
                b.BookingDate,
                b.StartTime,
                b.EndTime,
                b.Status,
                b.SportId,
                b.PackageId.HasValue && packages.TryGetValue(b.PackageId.Value, out var packageData)
                    ? packageData.Name
                    : null
            )).ToList();

            return new UserDashboardResponse(
                totalSessions,
                upcomingSessions
            );
        }
    }
}