using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<HomeownerProfile> HomeownerProfiles => Set<HomeownerProfile>();    
    public DbSet<WorkerProfile> WorkerProfiles => Set<WorkerProfile>();
    public DbSet<WorkerDocument> WorkerDocuments => Set<WorkerDocument>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<PaymentProof> PaymentProofs => Set<PaymentProof>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Notification> Notifications => Set<Notification>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Custom table names
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        // ApplicationUser configuration
        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            b.Property(u => u.PreferredLanguage).HasMaxLength(10);
            b.Property(u => u.ProfileImageUrl).HasMaxLength(500);

            b.HasOne(u => u.HomeownerProfile)
             .WithOne(p => p.User)
             .HasForeignKey<HomeownerProfile>(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(u => u.WorkerProfile)
             .WithOne(p => p.User)
             .HasForeignKey<WorkerProfile>(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(u => u.SentMessages)
             .WithOne(m => m.Sender)
             .HasForeignKey(m => m.SenderId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(u => u.ReceivedMessages)
             .WithOne(m => m.Receiver)
             .HasForeignKey(m => m.ReceiverId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(u => u.ReviewsWritten)
             .WithOne(r => r.Reviewer)
             .HasForeignKey(r => r.ReviewerId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(u => u.ReviewsReceived)
             .WithOne(r => r.Reviewee)
             .HasForeignKey(r => r.RevieweeId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(u => u.BookingsAsHomeowner)
             .WithOne(bo => bo.Homeowner)
             .HasForeignKey(bo => bo.HomeownerId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(u => u.BookingsAsWorker)
             .WithOne(bo => bo.Worker)
             .HasForeignKey(bo => bo.WorkerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // HomeownerProfile configuration
        builder.Entity<HomeownerProfile>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.NationalIdNumber).HasMaxLength(20).IsRequired();
            b.Property(p => p.NationalIdImage).HasMaxLength(500).IsRequired();
            b.Property(p => p.SelfieImage).HasMaxLength(500).IsRequired();
            b.Property(p => p.ProofOfAddressImage).HasMaxLength(500);
            b.Property(p => p.Address).HasMaxLength(500).IsRequired();
            b.Property(p => p.City).HasMaxLength(100).IsRequired();
            b.Property(p => p.District).HasMaxLength(100);
            b.Property(p => p.VerificationNotes).HasMaxLength(1000);
            b.Property(p => p.VerifiedBy).HasMaxLength(450);
        });

        // WorkerProfile configuration
        builder.Entity<WorkerProfile>(b =>
        {
            b.HasKey(p => p.Id);            
            b.Property(p => p.NationalIdNumber).HasMaxLength(20).IsRequired();            
            b.Property(p => p.PassportNumber).HasMaxLength(30);            
            b.Property(p => p.PassportCountry).HasMaxLength(100);
            b.Property(p => p.Bio).HasMaxLength(2000);
            b.Property(p => p.PreviousEmployer).HasMaxLength(200);
            b.Property(p => p.Languages).HasMaxLength(500);                                
            b.Property(p => p.VerifiedBy).HasMaxLength(450);
            b.Property(p => p.MonthlyRate).HasColumnType("decimal(18,2)");
            b.Property(p => p.HourlyRate).HasColumnType("decimal(18,2)");
            b.Property(p => p.AverageRating).HasColumnType("decimal(3,2)");
        });

        // WorkerDocument configuration
        builder.Entity<WorkerDocument>(b =>
        {
            b.HasKey(d => d.Id);            
            b.Property(d => d.DocumentImageUrl).HasMaxLength(500).IsRequired();
            b.Property(d => d.VerifiedBy).HasMaxLength(450);
            b.HasOne(d => d.Worker)
             .WithMany(w => w.Documents)
             .HasForeignKey(d => d.WorkerId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Booking configuration
        builder.Entity<Booking>(b =>
        {
            b.HasKey(bo => bo.Id);
            b.Property(bo => bo.MonthlySalary).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(bo => bo.CommissionAmount).HasColumnType("decimal(18,2)");
            b.Property(bo => bo.PaymentProofImageUrl).HasMaxLength(500);
            b.Property(bo => bo.PaymentConfirmedBy).HasMaxLength(450);
        });

        // Review configuration
        builder.Entity<Review>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Rating).IsRequired();
            b.Property(r => r.Comment).HasMaxLength(2000);
            b.HasOne(r => r.Booking)
             .WithMany(bo => bo.Reviews)
             .HasForeignKey(r => r.BookingId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Message configuration
        builder.Entity<Message>(b =>
        {
            b.HasKey(m => m.Id);
            b.Property(m => m.Content).HasMaxLength(5000).IsRequired();
        });

        // PaymentProof configuration
        builder.Entity<PaymentProof>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(p => p.ProofImageUrl).HasMaxLength(500).IsRequired();
            b.Property(p => p.TransactionReference).HasMaxLength(200);
            b.Property(p => p.ConfirmedBy).HasMaxLength(450);
            b.Property(p => p.RejectionReason).HasMaxLength(1000);
            b.HasOne(p => p.Booking)
             .WithMany()
             .HasForeignKey(p => p.BookingId)
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(p => p.Homeowner)
             .WithMany()
             .HasForeignKey(p => p.HomeownerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Subscription configuration
        builder.Entity<Subscription>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.Amount).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(s => s.PaymentProofImageUrl).HasMaxLength(500);
            b.Property(s => s.PaymentConfirmedBy).HasMaxLength(450);
        });

        // Notification configuration
        builder.Entity<Notification>(b =>
        {
            b.HasKey(n => n.Id);
            b.Property(n => n.Title).HasMaxLength(200).IsRequired();
            b.Property(n => n.Message).HasMaxLength(2000).IsRequired();
            b.Property(n => n.Type).HasMaxLength(50);
        });

        // Seed Admin role
        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                Id = "admin-role-id",
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "System Administrator"
            },
            new ApplicationRole
            {
                Id = "homeowner-role-id",
                Name = "Homeowner",
                NormalizedName = "HOMEOWNER",
                Description = "Home Owner"
            },
            new ApplicationRole
            {
                Id = "worker-role-id",
                Name = "Worker",
                NormalizedName = "WORKER",
                Description = "House Worker"
            }
        );
    }
}
