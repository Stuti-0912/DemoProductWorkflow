using DemoProductworkflow.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DemoProductworkflow.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<EmailSettings> EmailSettings { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
                .HasOne(p => p.CreatedBy)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.CreatedById);

            // Mark EmailSettings as a keyless entity
            builder.Entity<EmailSettings>().HasNoKey();
        }

    }
}
