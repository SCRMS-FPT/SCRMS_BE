using Microsoft.EntityFrameworkCore;
using Notification.API.Data.Model;

namespace Notification.API.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
        }

        public DbSet<MessageNotification> MessageNotifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
