using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AiSupport.Infrastructure.DataBase
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<AppService> AppServices { get; set; } = null!;
        public DbSet<Tenant> Tenants { get; set; } = null!;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //TODO: Add your customizations after calling base.OnModelCreating(builder);
            // Configure AppService entity
            builder.Entity<AppService>(b =>
            {
                b.HasKey(s => s.Id);
                b.Property(s => s.Name).IsRequired();
                b.Property(s => s.Price).HasPrecision(18, 2);
            });

            // Configure many-to-many between AppUser and Tenant with explicit join table AppUserTenant
            builder.Entity<AppUser>(b =>
            {
                b.HasMany(u => u.Tenants)
                 .WithMany(t => t.AppUsers)
                 .UsingEntity<Dictionary<string, object>>(
                    "AppUserTenant",
                    j => j.HasOne<Tenant>().WithMany().HasForeignKey("TenantId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<AppUser>().WithMany().HasForeignKey("AppUserId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("AppUserId", "TenantId");
                        j.ToTable("AppUserTenant");
                    });
            });

            // Configure Tenant entity and one-to-many Tenant -> AppService
            builder.Entity<Tenant>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.Name).IsRequired();
            });

            // Configure AppService Tenant relationship
            builder.Entity<AppService>(b =>
            {
                b.HasOne(s => s.Tenant)
                 .WithMany(t => t.AppServices)
                 .HasForeignKey(s => s.TenantId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
