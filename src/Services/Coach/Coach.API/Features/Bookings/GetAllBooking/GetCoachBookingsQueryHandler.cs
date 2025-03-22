using BuildingBlocks.Pagination;
using Coach.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Bookings.GetAllBooking
{
    public record GetCoachBookingsQuery(
        Guid CoachUserId,
        int PageIndex,
        int PageSize,
        string? Status,
        DateOnly? StartDate,
        DateOnly? EndDate,
        Guid? SportId,
        Guid? PackageId
    ) : IQuery<PaginatedResult<BookingHistoryResult>>;
    public record BookingHistoryResult(
        Guid Id,
        Guid UserId,
        DateOnly BookingDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string Status,
        decimal TotalPrice
    );

    public class GetCoachBookingsQueryValidator : AbstractValidator<GetCoachBookingsQuery>
    {
        public GetCoachBookingsQueryValidator()
        {
            RuleFor(x => x.CoachUserId).NotEmpty().WithMessage("CoachId is required.");
            RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0).WithMessage("PageIndex must be non-negative.");
            RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("PageSize must be greater than 0.");
        }
    }

    internal class GetCoachBookingsQueryHandler : IQueryHandler<GetCoachBookingsQuery, PaginatedResult<BookingHistoryResult>>
    {
        private readonly ICoachBookingRepository _bookingRepository;

        public GetCoachBookingsQueryHandler(ICoachBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<PaginatedResult<BookingHistoryResult>> Handle(GetCoachBookingsQuery query, CancellationToken cancellationToken)
        {
            var bookingsQuery = _bookingRepository.GetCoachBookingsByCoachIdQueryable(query.CoachUserId);

            // Filter theo Status
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                bookingsQuery = bookingsQuery.Where(b => b.Status == query.Status);
            }

            // Filter theo StartDate và EndDate
            if (query.StartDate.HasValue && query.EndDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= query.StartDate.Value && b.BookingDate <= query.EndDate.Value);
            }

            // Filter theo SportId
            if (query.SportId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.SportId == query.SportId.Value);
            }

            // Filter theo PackageId
            if (query.PackageId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.PackageId == query.PackageId.Value);
            }

            // Tính tổng số bản ghi
            var totalCount = await bookingsQuery.CountAsync(cancellationToken);

            // Phân trang và ánh xạ sang DTO
            var bookings = await bookingsQuery
                .OrderBy(b => b.BookingDate) // Có thể thay đổi cách sắp xếp
                .Skip(query.PageIndex * query.PageSize)
                .Take(query.PageSize)
                .Select(b => new BookingHistoryResult(
                    b.Id,
                    b.UserId,
                    b.BookingDate,
                    b.StartTime,
                    b.EndTime,
                    b.Status,
                    b.TotalPrice
                ))
                .ToListAsync(cancellationToken);

            return new PaginatedResult<BookingHistoryResult>(
                query.PageIndex,
                query.PageSize,
                totalCount,
                bookings
            );
        }
    }
}