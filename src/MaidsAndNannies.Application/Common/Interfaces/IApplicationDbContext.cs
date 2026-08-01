using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MaidsAndNannies.Application.Common.Interfaces;

/// <summary>
/// Everything in the Application layer (Commands/Queries/Handlers) talks to the
/// database only through this interface - never through the concrete
/// ApplicationDbContext. Infrastructure.Persistence.ApplicationDbContext implements
/// it. This keeps Application testable (fake/in-memory implementation for unit
/// tests) and keeps EF Core / SQL Server swappable without touching business logic.
/// </summary>
public interface IApplicationDbContext
{

    DatabaseFacade Database { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<HomeownerProfile> HomeownerProfiles { get; }
    DbSet<WorkerProfile> WorkerProfiles { get; }
    DbSet<WorkerSpecializationSpec> WorkerSpecializationSpecs { get; }
    DbSet<WorkerDocument> WorkerDocuments { get; }
    DbSet<Currency> Currencies { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Message> Messages { get; }
    DbSet<PaymentProof> PaymentProofs { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<Notification> Notifications { get; }

    DbSet<AppSetting> AppSettings { get; }

    DbSet<Policy> Policies { get; }

    DbSet<Country> Countries { get; }
    DbSet<State> States { get; }
    DbSet<City> Cities { get; }
    DbSet<JobPost> JobPosts { get; }
    DbSet<JobApplication> JobApplications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
