using BuildingBlocks.Pagination;
using Coach.API.Data;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Bookings.GetUserBookings
{
    public record GetUserBookingsQuery(
        Guid UserId,
        int PageIndex,
        int PageSize,
        string? Status,
        DateOnly? StartDate,
        DateOnly? EndDate,
        Guid? SportId,
        Guid? CoachId,
        Guid? PackageId
    ) : IQuery<PaginatedResult<UserBookingHistoryResult>>;

    public record UserBookingHistoryResult(
        Guid Id,
        Guid CoachId,
        string CoachName,
        DateOnly BookingDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string Status,
        decimal TotalPrice,
        Guid SportId,
        string? PackageName
    );

    public class GetUserBookingsQueryValidator : AbstractValidator<GetUserBookingsQuery>
    {
        public GetUserBookingsQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
            RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0).WithMessage("PageIndex must be non-negative.");
            RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("PageSize must be greater than 0.");
        }
    }

    public class GetUserBookingsQueryHandler : IQueryHandler<GetUserBookingsQuery, PaginatedResult<UserBookingHistoryResult>>
    {
        private readonly ICoachBookingRepository _bookingRepository;
        private readonly ICoachRepository _coachRepository;
        private readonly ICoachPackageRepository _packageRepository;
        private readonly CoachDbContext _dbContext;

        public GetUserBookingsQueryHandler(
            ICoachBookingRepository bookingRepository,
            ICoachRepository coachRepository,
            ICoachPackageRepository packageRepository,
            CoachDbContext dbContext)
        {
            _bookingRepository = bookingRepository;
            _coachRepository = coachRepository;
            _packageRepository = packageRepository;
            _dbContext = dbContext;
        }

        public async Task<PaginatedResult<UserBookingHistoryResult>> Handle(GetUserBookingsQuery query, CancellationToken cancellationToken)
        {
            // Get bookings for the user
            var bookingsQuery = _bookingRepository.GetCoachBookingsByUserIdQueryable(query.UserId);

            // Filter by Status
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                bookingsQuery = bookingsQuery.Where(b => b.Status == query.Status);
            }

            // Filter by StartDate and EndDate
            if (query.StartDate.HasValue && query.EndDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b =>
                    b.BookingDate >= query.StartDate.Value &&
                    b.BookingDate <= query.EndDate.Value);
            }

            // Filter by SportId
            if (query.SportId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.SportId == query.SportId.Value);
            }

            // Filter by CoachId
            if (query.CoachId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.CoachId == query.CoachId.Value);
            }

            // Filter by PackageId
            if (query.PackageId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.PackageId == query.PackageId.Value);
            }

            // Get total count
            var totalCount = await bookingsQuery.CountAsync(cancellationToken);

            // Get paginated bookings
            var bookings = await bookingsQuery
                .OrderByDescending(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .Skip(query.PageIndex * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            // Create a dictionary to store coach names
            var coachNames = new Dictionary<Guid, string>();
            var packageNames = new Dictionary<Guid, string>();

            // Get coach names and package names individually
            foreach (var booking in bookings)
            {
                // Get coach name if not already in dictionary
                if (!coachNames.ContainsKey(booking.CoachId))
                {
                    var coach = await _coachRepository.GetCoachByIdAsync(booking.CoachId, cancellationToken);
                    coachNames[booking.CoachId] = coach?.FullName ?? "Unknown Coach";
                }

                // Get package name if applicable and not already in dictionary
                if (booking.PackageId.HasValue && !packageNames.ContainsKey(booking.PackageId.Value))
                {
                    var package = await _packageRepository.GetCoachPackageByIdAsync(booking.PackageId.Value, cancellationToken);
                    packageNames[booking.PackageId.Value] = package?.Name ?? "Unknown Package";
                }
            }

            // Map to result
            var results = bookings.Select(b => new UserBookingHistoryResult(
                b.Id,
                b.CoachId,
                coachNames.GetValueOrDefault(b.CoachId, "Unknown Coach"),
                b.BookingDate,
                b.StartTime,
                b.EndTime,
                b.Status,
                b.TotalPrice,
                b.SportId,
                b.PackageId.HasValue ? packageNames.GetValueOrDefault(b.PackageId.Value) : null
            )).ToList();

            return new PaginatedResult<UserBookingHistoryResult>(
                query.PageIndex,
                query.PageSize,
                totalCount,
                results
            );
        }
    }
}