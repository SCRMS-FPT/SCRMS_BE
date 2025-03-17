using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.Data
{
    public interface IApplicationDbContext
    {
        DbSet<Court> Courts { get; }
        DbSet<CourtSchedule> CourtSchedules { get; }
        DbSet<Sport> Sports { get; }
        DbSet<SportCenter> SportCenters { get; }
        DbSet<Booking> Bookings { get; }
        DbSet<BookingDetail> BookingDetails { get; }
        DbSet<CourtPromotion> CourtPromotions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}