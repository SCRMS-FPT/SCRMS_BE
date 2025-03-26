using CourtBooking.Application.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.BookingManagement.Queries.GetBookingById
{
    public class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, BookingDto?>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ISportCenterRepository _sportCenterRepository;
        private readonly IApplicationDbContext _context;

        public GetBookingByIdHandler(IApplicationDbContext context, IBookingRepository bookingRepository, ISportCenterRepository sportCenterRepository)
        {
            _context = context;
            _bookingRepository = bookingRepository;
            _sportCenterRepository = sportCenterRepository;
        }

        public async Task<BookingDto?> Handle(GetBookingByIdQuery query, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(BookingId.Of(query.BookingId), cancellationToken);
            if (booking == null) return null;

            // Lấy danh sách CourtId từ BookingDetails
            var courtIds = booking.BookingDetails.Select(d => d.CourtId).ToList();
            var courts = await _context.Courts.Where(c => courtIds.Contains(c.Id)).ToListAsync(cancellationToken);
            var sportCenterIds = courts.Select(c => c.SportCenterId).Distinct().ToList();
            var sportCenters = await _context.SportCenters
                .Where(sc => sportCenterIds.Contains(sc.Id))
                .ToDictionaryAsync(sc => sc.Id.Value, sc => sc.Name, cancellationToken);

            var courtDetails = courts.ToDictionary(
                c => c.Id.Value,
                c => new { CourtName = c.CourtName.Value, SportCenterName = sportCenters[c.SportCenterId.Value] }
            );

            bool hasPermission = false;
            if (query.Role == "Admin")
            {
                hasPermission = true;
            }
            else if (query.Role == "CourtOwner")
            {
                var ownedSportsCenters = await _sportCenterRepository.GetSportCentersByOwnerIdAsync(query.UserId, cancellationToken);
                var ownedSportsCenterIds = ownedSportsCenters.Select(sc => sc.Id).ToHashSet();

                // Check if booking is at an owned center OR if it's the court owner's own booking
                hasPermission = booking.BookingDetails.Any(d =>
                    ownedSportsCenterIds.Contains(courts.First(c => c.Id == d.CourtId).SportCenterId))
                    || booking.UserId.Value == query.UserId;
            }
            else // User
            {
                hasPermission = booking.UserId.Value == query.UserId;
            }

            if (!hasPermission) return null;

            var bookingDto = new BookingDto(
                booking.Id.Value,
                booking.UserId.Value,
                booking.TotalTime,
                booking.TotalPrice,
                booking.RemainingBalance,
                booking.InitialDeposit,
                booking.Status.ToString(),
                booking.BookingDate,
                booking.Note,
                booking.CreatedAt,
                booking.LastModified,
                booking.BookingDetails.Select(d => new BookingDetailDto(
                    d.Id.Value,
                    d.CourtId.Value,
                    courtDetails[d.CourtId.Value].CourtName,
                    courtDetails[d.CourtId.Value].SportCenterName,
                    d.StartTime.ToString(@"hh\:mm\:ss"),
                    d.EndTime.ToString(@"hh\:mm\:ss"),
                    d.TotalPrice
                )).ToList()
            );

            return bookingDto;
        }
    }
}