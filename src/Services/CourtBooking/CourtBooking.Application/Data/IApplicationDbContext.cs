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
        DbSet<CourtOperatingHour> CourtOperatingHours { get; }
        DbSet<Sport> Sports { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
