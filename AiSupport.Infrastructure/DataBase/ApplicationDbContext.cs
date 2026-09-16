using AiSupport.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AiSupport.Infrastructure.DataBase
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<AppService> AppServices { get; set; } = null!;
        public DbSet<AppTenant> Tenants { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<PaymentAttempt> PaymentAttempts { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppService>(b =>
            {
                b.HasKey(s => s.Id);
                b.Property(s => s.Name).IsRequired();
                b.Property(s => s.Price).HasPrecision(18, 2);
            });

            builder.Entity<AppUser>(b =>
            {
                b.HasMany(u => u.Tenants)
                 .WithMany(t => t.AppUsers)
                 .UsingEntity<Dictionary<string, object>>(
                    "AppUserTenant",
                    j => j.HasOne<AppTenant>().WithMany().HasForeignKey("TenantId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<AppUser>().WithMany().HasForeignKey("AppUserId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("AppUserId", "TenantId");
                        j.ToTable("AppUserTenant");
                    });
            });

            builder.Entity<AppTenant>(b =>
            {
                b.ToTable("Tenants");
                b.HasKey(t => t.Id);
                b.Property(t => t.Name).IsRequired();

                b.HasMany(t => t.Subscriptions)
                 .WithOne(s => s.Tenant)
                 .HasForeignKey(s => s.TenantId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(t => t.Customers)
                 .WithOne(c => c.Tenant)
                 .HasForeignKey(c => c.TenantId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Subscription>(b =>
            {
                b.HasKey(s => s.Id);
                b.Property(s => s.Name).IsRequired();
                b.Property(s => s.Status)
                    .HasConversion<string>()
                    .IsRequired();

                b.HasMany(s => s.AppServices)
                 .WithMany(a => a.Subscriptions)
                 .UsingEntity<Dictionary<string, object>>(
                    "SubscriptionAppService",
                    j => j.HasOne<AppService>().WithMany().HasForeignKey("AppServiceId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<Subscription>().WithMany().HasForeignKey("SubscriptionId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("SubscriptionId", "AppServiceId");
                        j.ToTable("SubscriptionAppService");
                    });
            });

            builder.Entity<Customer>(b =>
            {
                b.ToTable("Customers");
                b.HasKey(c => c.Id);
                b.Property(c => c.Name).IsRequired();
                b.Property(c => c.ExternalId).HasMaxLength(256);
                b.HasIndex(c => c.ExternalId)
                    .HasDatabaseName("IX_Customers_ExternalId");
            });

            builder.Entity<PaymentAttempt>(b =>
            {
                b.ToTable("PaymentAttempts");
                b.HasKey(p => p.Id);
                b.Property(p => p.ProviderTransactionId).IsRequired().HasMaxLength(256);
                b.HasIndex(p => p.ProviderTransactionId)
                    .HasDatabaseName("IX_PaymentAttempts_ProviderTransactionId");
                b.Property(p => p.Currency).IsRequired().HasMaxLength(16);
                b.Property(p => p.Amount).HasPrecision(18, 2);
                b.Property(p => p.Status)
                    .HasConversion<string>()
                    .IsRequired();
                b.Property(p => p.ErrorCode).HasMaxLength(128);

                b.HasOne(p => p.Customer)
                 .WithMany(c => c.PaymentAttempts)
                 .HasForeignKey(p => p.CustomerId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}


