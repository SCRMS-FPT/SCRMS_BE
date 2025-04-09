using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Payment.API.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.ConfigureOutbox();

            modelBuilder.Entity<WalletTransaction>()
                .HasIndex(t => new { t.UserId, t.CreatedAt })
                .IncludeProperties(t => t.Amount)
                .HasDatabaseName("IX_Transactions_User_CreatedAt");

            modelBuilder.Entity<UserWallet>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<WithdrawalRequest>()
                .HasIndex(w => w.UserId)
                .HasDatabaseName("IX_WithdrawalRequests_UserId");

            modelBuilder.Entity<WithdrawalRequest>()
                .HasIndex(w => w.Status)
                .HasDatabaseName("IX_WithdrawalRequests_Status");
        }
    }
}