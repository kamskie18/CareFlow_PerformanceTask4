using CareFlow.Application.Interfaces;
using CareFlow.Application.Services;
using CareFlow.Domain.Entities;
using CareFlow.Infrastructure.Data;
using CareFlow.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareFlow.Tests;

public class AcceptanceTests
{
    private CareFlowDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CareFlowDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new CareFlowDbContext(options);

        // Seed basic test data
        if (!context.Patients.Any())
        {
            context.Patients.AddRange(
                new Patient { Id = 1, FirstName = "Juan", LastName = "Dela Cruz", DateOfBirth = new DateOnly(1990, 1, 1) },
                new Patient { Id = 2, FirstName = "Maria", LastName = "Clara", DateOfBirth = new DateOnly(1995, 5, 12) }
            );

            context.DiagnosticServices.AddRange(
                new DiagnosticService { Id = 1, Name = "CBC", DailyCapacity = 2 },
                new DiagnosticService { Id = 2, Name = "Urinalysis", DailyCapacity = 3 },
                new DiagnosticService { Id = 3, Name = "Chest X-Ray", DailyCapacity = 1 }
            );

            context.SaveChanges();
        }

        return context;
    }

    [Fact]
    public async Task AC01_ViewDiagnosticServices_CalculatesRemainingCapacityCorrectly()
    {
        var db = CreateInMemoryDbContext(nameof(AC01_ViewDiagnosticServices_CalculatesRemainingCapacityCorrectly));
        var repo = new BookingRepository(db);
        var service = new BookingService(repo);
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Pre-book 1 CBC
        var b = new Booking { Id = 101, PatientId = 1, BookingDate = today, CreatedAt = DateTime.UtcNow };
        db.Bookings.Add(b);
        db.BookingItems.Add(new BookingItem { BookingId = 101, DiagnosticServiceId = 1 });
        await db.SaveChangesAsync();

        var availability = await service.GetAvailabilityAsync(today);

        var cbc = availability.First(s => s.ServiceId == 1);
        Assert.Equal(2, cbc.DailyCapacity);
        Assert.Equal(1, cbc.BookedCount);
        Assert.Equal(1, cbc.Remaining);

        var urinalysis = availability.First(s => s.ServiceId == 2);
        Assert.Equal(3, urinalysis.DailyCapacity);
        Assert.Equal(0, urinalysis.BookedCount);
        Assert.Equal(3, urinalysis.Remaining);
    }

    [Fact]
    public async Task AC02_CreateBooking_WithMultipleAvailableServices_Succeeds()
    {
        var db = CreateInMemoryDbContext(nameof(AC02_CreateBooking_WithMultipleAvailableServices_Succeeds));
        var repo = new BookingRepository(db);
        var service = new BookingService(repo);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var result = await service.CreateBookingAsync(1, today, new List<int> { 1, 2 });

        Assert.True(result.Success);
        Assert.NotNull(result.BookingId);

        var booking = await repo.GetBookingByIdAsync(result.BookingId.Value);
        Assert.NotNull(booking);
        Assert.Equal(2, booking.BookingItems.Count);
    }

    [Fact]
    public async Task AC03_RequestFullyBookedService_OrInvalidInfo_Rejected()
    {
        var db = CreateInMemoryDbContext(nameof(AC03_RequestFullyBookedService_OrInvalidInfo_Rejected));
        var repo = new BookingRepository(db);
        var service = new BookingService(repo);
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Capacity of Chest X-Ray (Id=3) is 1
        var res1 = await service.CreateBookingAsync(1, today, new List<int> { 3 });
        Assert.True(res1.Success);

        // Try booking Chest X-Ray again for same date
        var res2 = await service.CreateBookingAsync(2, today, new List<int> { 3 });
        Assert.False(res2.Success);
        Assert.Contains("capacity", res2.Message, StringComparison.OrdinalIgnoreCase);

        // Invalid: Duplicate services in single booking (BR-06)
        var resDup = await service.CreateBookingAsync(1, today, new List<int> { 1, 1 });
        Assert.False(resDup.Success);

        // Invalid: Zero or >3 services (BR-02)
        var resTooMany = await service.CreateBookingAsync(1, today, new List<int> { 1, 2, 3, 4 });
        Assert.False(resTooMany.Success);

        // Invalid: Date in the past (BR-03)
        var pastDate = today.AddDays(-1);
        var resPast = await service.CreateBookingAsync(1, pastDate, new List<int> { 1 });
        Assert.False(resPast.Success);
    }

    [Fact]
    public async Task AC04_AttemptBookingAfterCapacityBecomesUnavailable_RevalidatesAndRejects()
    {
        var db = CreateInMemoryDbContext(nameof(AC04_AttemptBookingAfterCapacityBecomesUnavailable_RevalidatesAndRejects));
        var repo = new BookingRepository(db);
        var service = new BookingService(repo);
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Suppose client displayed capacity when Chest X-Ray (Id=3) had 1 spot left.
        // Another staff/client confirms a booking in the meantime:
        await service.CreateBookingAsync(1, today, new List<int> { 3 });

        // Second booking attempts to book with stale awareness:
        var secondAttempt = await service.CreateBookingAsync(2, today, new List<int> { 3 });

        Assert.False(secondAttempt.Success);
        Assert.Contains("capacity", secondAttempt.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AC05_OperationFailure_RollsBackTransaction_NoPartialRecords()
    {
        var db = CreateInMemoryDbContext(nameof(AC05_OperationFailure_RollsBackTransaction_NoPartialRecords));
        var repo = new BookingRepository(db);
        var service = new BookingService(repo);
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Exhaust service 3
        await service.CreateBookingAsync(1, today, new List<int> { 3 });

        var initialBookingsCount = await db.Bookings.CountAsync();
        var initialBookingItemsCount = await db.BookingItems.CountAsync();

        // Multi-service booking requesting service 1 (available) and service 3 (unavailable)
        var result = await service.CreateBookingAsync(2, today, new List<int> { 1, 3 });

        Assert.False(result.Success);
        // Ensure no partial records were written
        Assert.Equal(initialBookingsCount, await db.Bookings.CountAsync());
        Assert.Equal(initialBookingItemsCount, await db.BookingItems.CountAsync());
    }

    [Fact]
    public async Task AC06_PersistedBookings_AreRetrievable()
    {
        var db = CreateInMemoryDbContext(nameof(AC06_PersistedBookings_AreRetrievable));
        var repo = new BookingRepository(db);
        var service = new BookingService(repo);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var res = await service.CreateBookingAsync(1, today, new List<int> { 1, 2 });
        Assert.True(res.Success);

        var allBookings = await service.GetAllBookingsAsync();
        Assert.NotEmpty(allBookings);

        var first = allBookings.First(b => b.BookingId == res.BookingId);
        Assert.Equal("Dela Cruz, Juan", first.PatientName);
        Assert.Contains("CBC", first.Services);
        Assert.Contains("Urinalysis", first.Services);
    }
}
