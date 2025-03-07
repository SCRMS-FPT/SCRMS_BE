﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.Data
{
    public interface IApplicationDbContext
    {
        DbSet<Court> Courts { get; }
        DbSet<CourtSchedule> CourtSlots { get; }
        DbSet<Sport> Sports { get; }
        DbSet<SportCenter> SportCenters { get; }
        DbSet<Booking> Bookings { get; }
        DbSet<BookingPrice> BookingPrices { get; }
        DbSet<CourtPromotion> CourtPromotions { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
