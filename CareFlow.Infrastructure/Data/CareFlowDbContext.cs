using CareFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Data;

public class CareFlowDbContext : DbContext
{
    public CareFlowDbContext(DbContextOptions<CareFlowDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<DiagnosticService> DiagnosticServices => Set<DiagnosticService>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- Patient ----------
        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            e.Property(p => p.LastName).IsRequired().HasMaxLength(100);
        });

        // ---------- DiagnosticService ----------
        modelBuilder.Entity<DiagnosticService>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(150);
            e.Property(s => s.DailyCapacity).IsRequired();
            e.HasIndex(s => s.Name).IsUnique();
        });

        // ---------- Booking ----------
        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.BookingDate).IsRequired();
            e.Property(b => b.CreatedAt).IsRequired();
            e.HasOne(b => b.Patient)
             .WithMany(p => p.Bookings)
             .HasForeignKey(b => b.PatientId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- BookingItem (join table) ----------
        modelBuilder.Entity<BookingItem>(e =>
        {
            e.HasKey(bi => new { bi.BookingId, bi.DiagnosticServiceId });

            e.HasOne(bi => bi.Booking)
             .WithMany(b => b.BookingItems)
             .HasForeignKey(bi => bi.BookingId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(bi => bi.DiagnosticService)
             .WithMany(ds => ds.BookingItems)
             .HasForeignKey(bi => bi.DiagnosticServiceId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== SEED DATA ==========
        modelBuilder.Entity<Patient>().HasData(
            new Patient { Id = 1, FirstName = "Maria", LastName = "Santos", DateOfBirth = new DateOnly(1990, 3, 15) },
            new Patient { Id = 2, FirstName = "Juan", LastName = "Dela Cruz", DateOfBirth = new DateOnly(1985, 7, 22) },
            new Patient { Id = 3, FirstName = "Ana", LastName = "Reyes", DateOfBirth = new DateOnly(2000, 1, 10) },
            new Patient { Id = 4, FirstName = "Carlos", LastName = "Garcia", DateOfBirth = new DateOnly(1978, 11, 5) },
            new Patient { Id = 5, FirstName = "Elena", LastName = "Mendoza", DateOfBirth = new DateOnly(1995, 6, 28) },
            new Patient { Id = 6, FirstName = "Stephen", LastName = "Curry", DateOfBirth = new DateOnly(1988, 3, 14) },
            new Patient { Id = 7, FirstName = "Rubin", LastName = "Castro", DateOfBirth = new DateOnly(1992, 8, 20) },
            new Patient { Id = 8, FirstName = "Lance", LastName = "Balubar", DateOfBirth = new DateOnly(2001, 5, 11) },
            new Patient { Id = 9, FirstName = "Scudge", LastName = "Gil", DateOfBirth = new DateOnly(1999, 11, 23) },
            new Patient { Id = 10, FirstName = "Juan", LastName = "Pablo", DateOfBirth = new DateOnly(1994, 4, 18) },
            new Patient { Id = 11, FirstName = "Juan", LastName = "Tamad", DateOfBirth = new DateOnly(1996, 9, 2) },
            new Patient { Id = 12, FirstName = "Sarah", LastName = "David", DateOfBirth = new DateOnly(1998, 12, 15) }
        );

        modelBuilder.Entity<DiagnosticService>().HasData(
            new DiagnosticService { Id = 1, Name = "Complete Blood Count", Description = "CBC - Full blood panel analysis", DailyCapacity = 10 },
            new DiagnosticService { Id = 2, Name = "Urinalysis", Description = "Standard urine examination", DailyCapacity = 8 },
            new DiagnosticService { Id = 3, Name = "Chest X-Ray", Description = "PA and lateral chest radiograph", DailyCapacity = 5 },
            new DiagnosticService { Id = 4, Name = "ECG", Description = "12-lead electrocardiogram", DailyCapacity = 6 },
            new DiagnosticService { Id = 5, Name = "Fasting Blood Sugar", Description = "FBS - Glucose level after 8-hour fast", DailyCapacity = 10 },
            new DiagnosticService { Id = 6, Name = "Lipid Panel", Description = "Total cholesterol, HDL, LDL, triglycerides", DailyCapacity = 8 }
        );
    }
}
