using Microsoft.EntityFrameworkCore;

namespace Company_representative_tasks.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<UserLog> UserLogs { get; set; }
    }
}
