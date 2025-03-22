using CourtBooking.Application.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CourtBooking.Application.BookingManagement.Queries.GetBookings;

public class GetBookingsHandler : IRequestHandler<GetBookingsQuery, GetBookingsResult>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ISportCenterRepository _sportCenterRepository;
    private readonly IApplicationDbContext _context;

    public GetBookingsHandler(IApplicationDbContext context, IBookingRepository bookingRepository, ISportCenterRepository sportCenterRepository)
    {
        _context = context;
        _bookingRepository = bookingRepository;
        _sportCenterRepository = sportCenterRepository;
    }

    public async Task<GetBookingsResult> Handle(GetBookingsQuery query, CancellationToken cancellationToken)
    {
        var bookingsQuery = _context.Bookings
            .Include(b => b.BookingDetails)
            .AsQueryable();

        // **Logic lọc theo vai trò**
        if (query.Role == "Admin")
        {
            if (query.FilterUserId.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.UserId.Value == query.FilterUserId.Value);
            if (query.CourtId.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.BookingDetails.Any(d => d.CourtId.Value == query.CourtId.Value));
            if (query.SportsCenterId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.BookingDetails.Any(d =>
                    _context.Courts.Any(c => c.Id.Value == d.CourtId.Value && c.SportCenterId.Value == query.SportsCenterId.Value)));
            }
        }
        else if (query.Role == "CourtOwner")
        {
            var ownedSportsCenters = await _sportCenterRepository.GetSportCentersByOwnerIdAsync(query.UserId, cancellationToken);
            var ownedSportsCenterIds = ownedSportsCenters.Select(sc => sc.Id).ToList();
            bookingsQuery = bookingsQuery.Where(b => b.BookingDetails.Any(d =>
                _context.Courts.Any(c => c.Id == d.CourtId && ownedSportsCenterIds.Contains(c.SportCenterId))));

            if (query.FilterUserId.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.UserId.Value == query.FilterUserId);
            if (query.CourtId.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.BookingDetails.Any(d => d.CourtId.Value == query.CourtId));
            if (query.SportsCenterId.HasValue)
            {
                if (!ownedSportsCenterIds.Contains(SportCenterId.Of(query.SportsCenterId.Value)))
                    throw new UnauthorizedAccessException("You do not own this sports center");
                bookingsQuery = bookingsQuery.Where(b => b.BookingDetails.Any(d =>
                    _context.Courts.Any(c => c.Id == d.CourtId && c.SportCenterId == SportCenterId.Of(query.SportsCenterId.Value))));
            }
        }
        else // User
        {
            bookingsQuery = bookingsQuery.Where(b => b.UserId.Value == query.UserId);
        }

        // **Lọc bổ sung**
        if (query.Status.HasValue)
            bookingsQuery = bookingsQuery.Where(b => b.Status == query.Status.Value);
        if (query.StartDate.HasValue)
            bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= query.StartDate.Value);
        if (query.EndDate.HasValue)
            bookingsQuery = bookingsQuery.Where(b => b.BookingDate <= query.EndDate.Value);

        // **Lấy tổng số lượng và danh sách bookings**
        var totalCount = await bookingsQuery.CountAsync(cancellationToken);
        var bookings = await bookingsQuery
            .OrderBy(b => b.BookingDate)
            .Skip(query.Page * query.Limit)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        // **Lấy thông tin Courts và SportCenters**
        var allCourtIds = bookings.SelectMany(b => b.BookingDetails.Select(d => d.CourtId)).Distinct().ToList();
        var courts = await _context.Courts
            .Where(c => allCourtIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c, cancellationToken);

        var sportCenterIds = courts.Values.Select(c => c.SportCenterId).Distinct().ToList();
        var sportCenters = await _context.SportCenters
            .Where(sc => sportCenterIds.Contains(sc.Id))
            .ToDictionaryAsync(sc => sc.Id, sc => sc.Name, cancellationToken);

        // **Ánh xạ sang DTO**
        var bookingDtos = bookings.Select(b => new BookingDto(
            b.Id.Value,
            b.UserId.Value,
            b.TotalTime,
            b.TotalPrice,
            b.RemainingBalance,
            b.InitialDeposit,
            b.Status.ToString(),
            b.BookingDate,
            b.Note,
            b.CreatedAt,
            b.LastModified,
            b.BookingDetails.Select(d =>
            {
                var court = courts[d.CourtId];
                var sportCenterName = sportCenters[court.SportCenterId];
                return new BookingDetailDto(
                    d.Id.Value,
                    d.CourtId.Value,
                    court.CourtName.Value,
                    sportCenterName,
                    d.StartTime.ToString(@"hh\:mm\:ss"),
                    d.EndTime.ToString(@"hh\:mm\:ss"),
                    d.TotalPrice
                );
            }).ToList()
        )).ToList();

        return new GetBookingsResult(bookingDtos, totalCount);
    }
}