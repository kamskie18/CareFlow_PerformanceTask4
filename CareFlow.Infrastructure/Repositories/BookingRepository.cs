using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CareFlow.Infrastructure.Repositories;

/// <summary>
/// Repository that handles all database operations for bookings.
/// BR-07: CreateBookingAsync uses an explicit transaction so that
/// the Booking row and all BookingItem rows are committed atomically.
/// BR-08: Capacity is re-checked inside the same transaction.
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly CareFlowDbContext _context;

    public BookingRepository(CareFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<Patient>> GetAllPatientsAsync()
    {
        return await _context.Patients
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<DiagnosticService>> GetAllServicesAsync()
    {
        return await _context.DiagnosticServices
            .OrderBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetBookedCountAsync(int serviceId, DateOnly date)
    {
        return await _context.BookingItems
            .CountAsync(bi => bi.DiagnosticServiceId == serviceId
                           && bi.Booking.BookingDate == date);
    }

    /// <summary>
    /// Creates a booking atomically within a database transaction.
    /// If any step fails, the entire transaction is rolled back (BR-07).
    /// Capacity is re-validated inside the transaction (BR-08).
    /// </summary>
    public async Task<Booking> CreateBookingAsync(int patientId, DateOnly date, List<int> serviceIds)
    {
        IDbContextTransaction? transaction = null;
        if (_context.Database.IsRelational())
        {
            transaction = await _context.Database.BeginTransactionAsync();
        }

        try
        {
            // BR-08: Re-check capacity inside the transaction
            foreach (var serviceId in serviceIds)
            {
                var service = await _context.DiagnosticServices.FindAsync(serviceId)
                    ?? throw new InvalidOperationException($"Service {serviceId} not found.");

                var currentCount = await _context.BookingItems
                    .CountAsync(bi => bi.DiagnosticServiceId == serviceId
                                   && bi.Booking.BookingDate == date);

                if (currentCount >= service.DailyCapacity)
                {
                    throw new InvalidOperationException(
                        $"'{service.Name}' has reached its daily capacity. Booking rejected.");
                }
            }

            // Create the booking record
            var booking = new Booking
            {
                PatientId = patientId,
                BookingDate = date,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Create all BookingItem records
            foreach (var serviceId in serviceIds)
            {
                _context.BookingItems.Add(new BookingItem
                {
                    BookingId = booking.Id,
                    DiagnosticServiceId = serviceId
                });
            }

            await _context.SaveChangesAsync();

            // BR-07: Commit — all or nothing
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            // Reload with navigation properties
            return await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.DiagnosticService)
                .FirstAsync(b => b.Id == booking.Id);
        }
        catch
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
            throw;
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    public async Task<List<Booking>> GetAllBookingsAsync()
    {
        return await _context.Bookings
            .Include(b => b.Patient)
            .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.DiagnosticService)
            .OrderByDescending(b => b.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.Patient)
            .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.DiagnosticService)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
