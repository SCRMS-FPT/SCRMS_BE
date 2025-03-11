using CourtBooking.Application.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options) { }

        public DbSet<Court> Courts { get; set; }

        public DbSet<CourtSchedule> CourtSlots { get; set; }

        public DbSet<Sport> Sports { get; set; }
        public DbSet<SportCenter> SportCenters { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingPrice> BookingPrices { get; set; }
        public DbSet<CourtPromotion> CourtPromotions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
            
        }
    }
}
