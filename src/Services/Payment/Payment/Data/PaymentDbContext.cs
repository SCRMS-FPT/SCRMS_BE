using Microsoft.EntityFrameworkCore;

namespace Payment.API.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WalletTransaction>()
                .HasIndex(t => new { t.UserId, t.CreatedAt })
                .IncludeProperties(t => t.Amount)
                .HasDatabaseName("IX_Transactions_User_CreatedAt");

            modelBuilder.Entity<UserWallet>()
                .HasKey(u => u.UserId);
        }
    }
}