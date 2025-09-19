using Microsoft.EntityFrameworkCore;

namespace Argent_Company.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<UserLog> UserLogs { get; set; }
        public DbSet<AgentDetails> AgentDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent multiple cascade paths
        }
    }
}
