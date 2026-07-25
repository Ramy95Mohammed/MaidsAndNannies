using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options), IApplicationDbContext
{
    public DbSet<HomeownerProfile> HomeownerProfiles => Set<HomeownerProfile>();
    public DbSet<WorkerProfile> WorkerProfiles => Set<WorkerProfile>();
    public DbSet<WorkerSpecializationSpec> WorkerSpecializationSpecs => Set<WorkerSpecializationSpec>();
    public DbSet<WorkerDocument> WorkerDocuments => Set<WorkerDocument>();
    public DbSet<Currency>  Currencies => Set<Currency>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<PaymentProof> PaymentProofs => Set<PaymentProof>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Custom table names
        builder.Entity<ApplicationUser>().ToTable("Users");        
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
            b.Property(p => p.WhatsAppNumber).HasMaxLength(20);
            b.Property(p => p.Bio).HasMaxLength(2000);
            b.Property(p => p.PreviousEmployer).HasMaxLength(200);
            b.Property(p => p.Languages).HasMaxLength(500);
            b.Property(p => p.VerifiedBy).HasMaxLength(450);
            b.Property(p => p.DailyRate).HasColumnType("decimal(18,2)");
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


        // Currency configuration
        builder.Entity<Currency>(b =>
        {            
            b.Property(c => c.Code).HasMaxLength(10).IsRequired();
            b.Property(c => c.Symbol).HasMaxLength(10).IsRequired();
            b.Property(c => c.NameAr).HasMaxLength(100).IsRequired();
            b.Property(c => c.NameEn).HasMaxLength(100).IsRequired();
            b.Property(c => c.RateToEgp).HasColumnType("decimal(18,6)").IsRequired();
        });

        // Seed currencies
        builder.Entity<Currency>().HasData(
            new Currency { Id = 1, Code = "EGP", Symbol = "E£", NameAr = "جنيه مصري", NameEn = "Egyptian Pound", RateToEgp = 1m, IsActive = true },
            new Currency { Id = 2, Code = "USD", Symbol = "$", NameAr = "دولار أمريكي", NameEn = "US Dollar", RateToEgp = 48.5m, IsActive = true },
            new Currency { Id = 3, Code = "SAR", Symbol = "﷼", NameAr = "ريال سعودي", NameEn = "Saudi Riyal", RateToEgp = 12.9m, IsActive = true }
        );
     

        // Booking configuration
        builder.Entity<Booking>(b =>
        {
            b.HasKey(bo => bo.Id);
            b.Property(bo => bo.MonthlySalary).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(bo => bo.TotalAmount).HasColumnType("decimal(18,2)");
            b.Property(bo => bo.CommissionAmount).HasColumnType("decimal(18,2)");
            b.Property(bo => bo.PaymentProofImageUrl).HasMaxLength(500);
            b.Property(bo => bo.PaymentConfirmedBy).HasMaxLength(450);
            b.Property(bo => bo.AdminNotes).HasMaxLength(2000);
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

        // AppSetting configuration
        builder.Entity<AppSetting>(b =>
        {
            b.HasKey(s => s.Key);
            b.Property(s => s.Key).HasMaxLength(100).IsRequired();
            b.Property(s => s.Value).HasMaxLength(500).IsRequired();
            b.Property(s => s.Description).HasMaxLength(500);
        });

        // Seed settings
        builder.Entity<AppSetting>().HasData(
            new AppSetting { Key = "MaxReplacementCount", Value = "2", Description = "الحد الأقصى لعدد مرات الاستبدال لكل حجز" },
            new AppSetting { Key = "CommissionDailyPercent", Value = "10", Description = "نسبة العمولة للحجوزات اليومية (%)" },
            new AppSetting { Key = "CommissionHourlyPercent", Value = "10", Description = "نسبة العمولة للحجوزات بالساعة (%)" },
            new AppSetting { Key = "CommissionMonthlyOneTimePercent", Value = "10", Description = "نسبة العمولة للحجوزات الشهرية (مرة واحدة)" },
            new AppSetting { Key = "CommissionMonthlySubscriptionPercent", Value = "10", Description = "نسبة العمولة للحجوزات الشهرية (اشتراك شهري)" },
            new AppSetting { Key = "AutoCancelPendingBookingHours", Value = "48", Description = "إلغاء الحجوزات المعلقة تلقائياً بعد (ساعة)" },
            new AppSetting { Key = "MaxActiveBookingsPerHomeowner", Value = "5", Description = "الحد الأقصى للحجوزات النشطة لكل صاحبة منزل" }
        );


        builder.Entity<Country>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Id).ValueGeneratedNever();
            b.Property(c => c.Name_ar).HasMaxLength(200);
            b.Property(c => c.Name_en).HasMaxLength(200);
            b.Property(c => c.Iso2).HasMaxLength(2);
            b.Property(c => c.Iso3).HasMaxLength(3);
            b.Property(c => c.Phone_code).HasMaxLength(100);
            b.Property(c => c.Nationality_ar).HasMaxLength(200);
            b.Property(c => c.Nationality_en).HasMaxLength(200);
            b.Property(c => c.Region).HasMaxLength(200);
            b.HasIndex(c => c.Iso2).IsUnique();
        });

        builder.Entity<State>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).ValueGeneratedNever();
            b.Property(s => s.Name_ar).HasMaxLength(200);
            b.Property(s => s.Name_en).HasMaxLength(200);
            b.Property(s => s.State_code).HasMaxLength(100);
            b.HasOne(s => s.Country)
             .WithMany()
             .HasForeignKey(s => s.Country_id)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<City>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Id).ValueGeneratedNever();
            b.Property(c => c.Name_ar).HasMaxLength(200);
            b.Property(c => c.Name_en).HasMaxLength(200);
            b.HasOne(c => c.Country)
            .WithMany()
            .HasForeignKey(c => c.Country_id)
            .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(c => c.State)
             .WithMany()
             .HasForeignKey(c => c.State_id)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed Admin role
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole()
            {
                Id = "admin-role-id",
                Name = "Admin",
                NormalizedName = "ADMIN",                
            },
            new IdentityRole
            {
                Id = "homeowner-role-id",
                Name = "Homeowner",
                NormalizedName = "HOMEOWNER",                
            },
            new IdentityRole
            {
                Id = "worker-role-id",
                Name = "Worker",
                NormalizedName = "WORKER",                
            }
        );
    }
}