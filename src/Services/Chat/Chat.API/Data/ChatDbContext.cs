using Microsoft.EntityFrameworkCore;

namespace Chat.API.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
        }

        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatSession>()
                .HasMany(s => s.Messages)
                .WithOne(m => m.Session)
                .HasForeignKey(m => m.Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessage>()
                .HasIndex(cm => new { cm.ChatSessionId, cm.SentAt });
        }
    }
}