using System.Security.Cryptography;
using System.Text;
using AiSupport.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiSupport.Infrastructure.DataBase.Seeding
{
    /// <summary>
    /// Fills a development database with realistic, deterministic demo data.
    /// Idempotent: tenant data (services, tenants, subscriptions, customers, payments) is created
    /// only when the marker tenant is missing; seed users are created only when their email is missing
    /// (existing users keep their current tenant).
    /// Everything is written in one transaction, so a failed run leaves no partial data.
    /// </summary>
    public class DevDataSeeder
    {
        public const string AdminEmail = "admin@aisupport.local";
        public const string DemoEmail = "demo@aisupport.local";
        public const string InitechEmail = "initech@aisupport.local";

        // Development-only password for seeded users.
        private const string DevPassword = "Passw0rd!";
        private const string GuidNamespace = "aisupport-dev-seed:";
        private const string MarkerTenantName = "Acme Retail";
        private const int RandomSeed = 20261006;
        private const int PaymentHistoryDays = 60;

        private static readonly UserSeed[] UserSeeds =
        {
            new(AdminEmail, "Alex", "Morgan", "Acme Retail"),
            new(DemoEmail, "Dana", "Reed", "Globex Payments"),
            new(InitechEmail, "Peter", "Gibbons", "Initech")
        };

        private static readonly AppServiceSeed[] AppServiceSeeds =
        {
            new(
                "Payments Monitoring",
                199.00m,
                "Real-time monitoring of payment success rates, decline spikes and provider outages."),
            new(
                "Customer Support Bot",
                149.00m,
                "AI assistant that answers customer payment questions and triages support tickets."),
            new(
                "Fraud Detection",
                299.00m,
                "Risk scoring and anomaly detection for card and bank transfer payments.")
        };

        private static readonly TenantSeed[] TenantSeeds =
        {
            new(
                "Acme Retail",
                "Omnichannel retailer selling home goods online and in 120 stores.",
                420,
                new[]
                {
                    new SubscriptionSeed("Acme Growth Plan", SubscriptionStatus.Active, -120, 245, new[] { "Payments Monitoring", "Customer Support Bot" }),
                    new SubscriptionSeed("Acme Fraud Pilot", SubscriptionStatus.Expired, -200, -20, new[] { "Fraud Detection" })
                },
                new[]
                {
                    new CustomerSeed("Emily Carter", "USD", true),
                    new CustomerSeed("James Whitaker", "USD", true),
                    new CustomerSeed("Sophia Martinez", "USD", false),
                    new CustomerSeed("Daniel Brooks", "USD", true),
                    new CustomerSeed("Olivia Bennett", "USD", false)
                }),
            new(
                "Globex Payments",
                "Payment facilitator serving small and mid-size merchants across Europe.",
                610,
                new[]
                {
                    new SubscriptionSeed("Globex Enterprise", SubscriptionStatus.Active, -300, 65, new[] { "Payments Monitoring", "Fraud Detection", "Customer Support Bot" }),
                    new SubscriptionSeed("Globex Support Add-on", SubscriptionStatus.Cancelled, -90, 275, new[] { "Customer Support Bot" })
                },
                new[]
                {
                    new CustomerSeed("Northwind Coffee Co.", "EUR", true),
                    new CustomerSeed("Bluebird Florist", "EUR", true),
                    new CustomerSeed("Summit Outdoor Gear", "EUR", true),
                    new CustomerSeed("Harbor Dental Clinic", "EUR", false),
                    new CustomerSeed("Lviv Bakery House", "UAH", true),
                    new CustomerSeed("Dnipro Tech Repair", "UAH", false)
                }),
            new(
                "Initech",
                "B2B SaaS company billing customers monthly by card and invoice.",
                250,
                new[]
                {
                    new SubscriptionSeed("Initech Starter", SubscriptionStatus.Active, -45, 320, new[] { "Payments Monitoring" }),
                    new SubscriptionSeed("Initech Fraud Trial", SubscriptionStatus.Cancelled, -60, -30, new[] { "Fraud Detection" })
                },
                new[]
                {
                    new CustomerSeed("Tailspin Toys", "USD", true),
                    new CustomerSeed("Litware Systems", "USD", true),
                    new CustomerSeed("Adventure Works", "EUR", true),
                    new CustomerSeed("Proseware Labs", "USD", false)
                })
        };

        private static readonly string[] FailureCodes =
        {
            "card_declined",
            "card_declined",
            "insufficient_funds",
            "insufficient_funds",
            "expired_card",
            "processing_error",
            "do_not_honor",
            "incorrect_cvc"
        };

        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<DevDataSeeder> _logger;

        public DevDataSeeder(
            ApplicationDbContext db,
            UserManager<AppUser> userManager,
            ILogger<DevDataSeeder> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var markerTenantId = CreateDeterministicGuid($"tenant:{MarkerTenantName}");

            var tenantDataSeeded = await _db.Tenants.AnyAsync(
                t => t.Id == markerTenantId,
                cancellationToken);

            if (tenantDataSeeded)
            {
                _logger.LogInformation("Development tenant data already present. Skipping tenant data.");
            }
            else
            {
                await SeedTenantDataAsync(now, cancellationToken);
            }

            var createdUserCount = 0;

            foreach (var userSeed in UserSeeds)
            {
                var created = await EnsureUserAsync(userSeed, now, cancellationToken);

                if (created)
                {
                    createdUserCount++;
                }
            }

            await transaction.CommitAsync(cancellationToken);

            if (createdUserCount > 0)
            {
                _logger.LogInformation(
                    "Development users created: {CreatedUserCount}.",
                    createdUserCount);
            }
        }

        private async Task SeedTenantDataAsync(DateTime now, CancellationToken cancellationToken)
        {
            var random = new Random(RandomSeed);
            var usedTransactionIds = new HashSet<string>();

            var services = AppServiceSeeds
                .Select(CreateAppService)
                .ToDictionary(s => s.Name);

            _db.AppServices.AddRange(services.Values);

            var customerCount = 0;
            var paymentAttemptCount = 0;

            foreach (var tenantSeed in TenantSeeds)
            {
                var tenant = CreateTenant(tenantSeed, now, random);

                foreach (var subscriptionSeed in tenantSeed.Subscriptions)
                {
                    var subscription = CreateSubscription(tenant, subscriptionSeed, services, now);
                    tenant.Subscriptions.Add(subscription);
                }

                foreach (var customerSeed in tenantSeed.Customers)
                {
                    var customer = CreateCustomer(tenant, customerSeed, now, random);

                    var attempts = CreatePaymentAttempts(
                        customer,
                        customerSeed.Currency,
                        now,
                        random,
                        usedTransactionIds);

                    foreach (var attempt in attempts)
                    {
                        customer.PaymentAttempts.Add(attempt);
                    }

                    tenant.Customers.Add(customer);
                    customerCount++;
                    paymentAttemptCount += attempts.Count;
                }

                _db.Tenants.Add(tenant);
            }

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Development tenant data created: {TenantCount} tenants, {ServiceCount} app services, {CustomerCount} customers, {PaymentAttemptCount} payment attempts.",
                TenantSeeds.Length,
                services.Count,
                customerCount,
                paymentAttemptCount);
        }

        /// <summary>
        /// Creates the seed user in its tenant when no user with that email exists.
        /// Returns true when a user was created.
        /// </summary>
        private async Task<bool> EnsureUserAsync(
            UserSeed seed,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var existing = await _userManager.FindByEmailAsync(seed.Email);

            if (existing != null)
            {
                return false;
            }

            var tenantId = CreateDeterministicGuid($"tenant:{seed.TenantName}");

            var tenantExists = await _db.Tenants.AnyAsync(
                t => t.Id == tenantId,
                cancellationToken);

            if (!tenantExists)
            {
                _logger.LogWarning(
                    "Skipping development user {Email}: tenant {TenantName} not found.",
                    seed.Email,
                    seed.TenantName);

                return false;
            }

            var user = new AppUser
            {
                Id = CreateDeterministicGuid($"user:{seed.Email}"),
                UserName = seed.Email,
                Email = seed.Email,
                EmailConfirmed = true,
                Name = seed.Email,
                FirstName = seed.FirstName,
                LastName = seed.LastName,
                TenantId = tenantId,
                CreatedDateTime = now,
                UpdatedDateTime = now
            };

            var result = await _userManager.CreateAsync(user, DevPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => e.Description);

                throw new InvalidOperationException(
                    $"Failed to create development user '{seed.Email}': {string.Join("; ", errors)}");
            }

            return true;
        }
        private static AppService CreateAppService(AppServiceSeed seed)
        {
            return new AppService
            {
                Id = CreateDeterministicGuid($"service:{seed.Name}"),
                Name = seed.Name,
                Price = seed.Price,
                Description = seed.Description
            };
        }

        private static AppTenant CreateTenant(TenantSeed seed, DateTime now, Random random)
        {
            return new AppTenant
            {
                Id = CreateDeterministicGuid($"tenant:{seed.Name}"),
                Name = seed.Name,
                Description = seed.Description,
                CreatedDateTime = now.AddDays(-seed.AgeInDays),
                UpdatedDateTime = now.AddDays(-random.Next(0, 30))
            };
        }

        private static Subscription CreateSubscription(
            AppTenant tenant,
            SubscriptionSeed seed,
            IReadOnlyDictionary<string, AppService> services,
            DateTime now)
        {
            var today = now.Date;
            var startDate = today.AddDays(seed.StartOffsetDays);

            var subscription = new Subscription
            {
                Id = CreateDeterministicGuid($"subscription:{tenant.Name}:{seed.Name}"),
                TenantId = tenant.Id,
                Name = seed.Name,
                Status = seed.Status,
                StartDate = startDate,
                EndDate = today.AddDays(seed.EndOffsetDays),
                CreatedDateTime = startDate.AddDays(-3),
                UpdatedDateTime = now
            };

            foreach (var serviceName in seed.ServiceNames)
            {
                subscription.AppServices.Add(services[serviceName]);
            }

            return subscription;
        }

        private static Customer CreateCustomer(
            AppTenant tenant,
            CustomerSeed seed,
            DateTime now,
            Random random)
        {
            var createdDateTime = now.AddDays(-(PaymentHistoryDays + random.Next(10, 240)));

            return new Customer
            {
                Id = CreateDeterministicGuid($"customer:{tenant.Name}:{seed.Name}"),
                TenantId = tenant.Id,
                Name = seed.Name,
                ExternalId = CreateExternalId(seed, random),
                CreatedDateTime = createdDateTime,
                UpdatedDateTime = now.AddDays(-random.Next(0, 30))
            };
        }

        private static List<PaymentAttempt> CreatePaymentAttempts(
            Customer customer,
            string currency,
            DateTime now,
            Random random,
            HashSet<string> usedTransactionIds)
        {
            var count = random.Next(5, 16);
            var attempts = new List<PaymentAttempt>(count);

            for (var i = 0; i < count; i++)
            {
                var status = PickStatus(random);

                var attempt = new PaymentAttempt
                {
                    Id = CreateDeterministicGuid($"payment:{customer.Id}:{i}"),
                    CustomerId = customer.Id,
                    ProviderTransactionId = CreateTransactionId(random, usedTransactionIds),
                    Amount = PickAmount(currency, random),
                    Currency = currency,
                    Status = status,
                    ErrorCode = PickErrorCode(status, random),
                    CreatedDateTime = PickCreatedDateTime(status, now, random)
                };

                attempts.Add(attempt);
            }

            return attempts;
        }

        private static PaymentStatus PickStatus(Random random)
        {
            var roll = random.Next(100);

            if (roll < 65)
            {
                return PaymentStatus.Completed;
            }

            if (roll < 87)
            {
                return PaymentStatus.Failed;
            }

            if (roll < 95)
            {
                return PaymentStatus.Pending;
            }

            return PaymentStatus.Unknown;
        }

        private static string? PickErrorCode(PaymentStatus status, Random random)
        {
            if (status != PaymentStatus.Failed)
            {
                return null;
            }

            return FailureCodes[random.Next(FailureCodes.Length)];
        }

        private static DateTime PickCreatedDateTime(PaymentStatus status, DateTime now, Random random)
        {
            // Pending payments are only realistic for the last couple of days.
            if (status == PaymentStatus.Pending)
            {
                return now.AddMinutes(-random.Next(5, 48 * 60));
            }

            return now.AddMinutes(-random.Next(60, PaymentHistoryDays * 24 * 60));
        }

        private static decimal PickAmount(string currency, Random random)
        {
            var (min, max) = currency switch
            {
                "UAH" => (150.0, 25000.0),
                "EUR" => (10.0, 1200.0),
                _ => (12.0, 900.0)
            };

            var amount = min + (random.NextDouble() * (max - min));

            return Math.Round((decimal)amount, 2);
        }

        private static string? CreateExternalId(CustomerSeed seed, Random random)
        {
            if (!seed.HasExternalId)
            {
                return null;
            }

            return "cus_" + CreateRandomToken(random, 14);
        }

        private static string CreateTransactionId(Random random, HashSet<string> usedTransactionIds)
        {
            while (true)
            {
                var transactionId = "txn_" + CreateRandomToken(random, 24);

                if (usedTransactionIds.Add(transactionId))
                {
                    return transactionId;
                }
            }
        }

        private static string CreateRandomToken(Random random, int length)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";

            var chars = new char[length];

            for (var i = 0; i < length; i++)
            {
                chars[i] = alphabet[random.Next(alphabet.Length)];
            }

            return new string(chars);
        }

        private static Guid CreateDeterministicGuid(string key)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(GuidNamespace + key));

            return new Guid(hash.AsSpan(0, 16));
        }

        private sealed record UserSeed(string Email, string FirstName, string LastName, string TenantName);

        private sealed record AppServiceSeed(string Name, decimal Price, string Description);

        private sealed record SubscriptionSeed(
            string Name,
            SubscriptionStatus Status,
            int StartOffsetDays,
            int EndOffsetDays,
            string[] ServiceNames);

        private sealed record CustomerSeed(string Name, string Currency, bool HasExternalId);

        private sealed record TenantSeed(
            string Name,
            string Description,
            int AgeInDays,
            SubscriptionSeed[] Subscriptions,
            CustomerSeed[] Customers);
    }
}
