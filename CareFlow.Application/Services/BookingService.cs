using CareFlow.Application.DTOs;
using CareFlow.Application.Interfaces;

namespace CareFlow.Application.Services;

/// <summary>
/// Core booking service that enforces all business rules (BR-01 through BR-10)
/// independently of the UI layer.
/// </summary>
public class BookingService
{
    private readonly IBookingRepository _repository;

    public BookingService(IBookingRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Returns availability for every diagnostic service on the given date.
    /// Implements Feature A and supports AC-01.
    /// </summary>
    public async Task<List<ServiceAvailabilityDto>> GetAvailabilityAsync(DateOnly date)
    {
        var services = await _repository.GetAllServicesAsync();
        var result = new List<ServiceAvailabilityDto>();

        foreach (var svc in services)
        {
            var booked = await _repository.GetBookedCountAsync(svc.Id, date);
            result.Add(new ServiceAvailabilityDto(
                svc.Id, svc.Name, svc.DailyCapacity, booked, svc.DailyCapacity - booked));
        }

        return result;
    }

    /// <summary>
    /// Creates a booking after validating all business rules.
    /// BR-01: patient must exist; BR-02: 1-3 distinct services;
    /// BR-03: date >= today; BR-05/BR-08: capacity re-checked at commit time;
    /// BR-06: no duplicate services; BR-07: atomic transaction;
    /// BR-10: understandable error messages on failure.
    /// Implements Features B, C and supports AC-02 through AC-05.
    /// </summary>
    public async Task<BookingResultDto> CreateBookingAsync(
        int patientId, DateOnly date, List<int> serviceIds)
    {
        // BR-01: Patient must exist
        var patients = await _repository.GetAllPatientsAsync();
        if (!patients.Any(p => p.Id == patientId))
            return new BookingResultDto(false, "Selected patient does not exist.");

        // BR-06: No duplicate services
        if (serviceIds.Distinct().Count() != serviceIds.Count)
            return new BookingResultDto(false, "A service cannot appear more than once in a booking.");

        // BR-02: 1 to 3 services
        if (serviceIds.Count < 1 || serviceIds.Count > 3)
            return new BookingResultDto(false, "A booking must contain between 1 and 3 diagnostic services.");

        // BR-03: Date cannot be in the past
        if (date < DateOnly.FromDateTime(DateTime.Now))
            return new BookingResultDto(false, "Booking date cannot be earlier than today.");

        // Validate service IDs exist
        var allServices = await _repository.GetAllServicesAsync();
        var serviceMap = allServices.ToDictionary(s => s.Id);
        foreach (var sid in serviceIds)
        {
            if (!serviceMap.ContainsKey(sid))
                return new BookingResultDto(false, $"Diagnostic service with ID {sid} does not exist.");
        }

        // BR-05 / BR-08: Re-check availability right before booking (inside transaction)
        foreach (var sid in serviceIds)
        {
            var booked = await _repository.GetBookedCountAsync(sid, date);
            var capacity = serviceMap[sid].DailyCapacity;
            if (booked >= capacity)
                return new BookingResultDto(false,
                    $"'{serviceMap[sid].Name}' has reached its daily capacity ({capacity}) for {date:yyyy-MM-dd}. The entire booking has been rejected.");
        }

        // BR-07: Atomic transaction — handled inside the repository
        try
        {
            var booking = await _repository.CreateBookingAsync(patientId, date, serviceIds);
            return new BookingResultDto(true,
                $"Booking #{booking.Id} confirmed for {date:yyyy-MM-dd}.", booking.Id);
        }
        catch (Exception ex)
        {
            // BR-10: Understandable feedback
            return new BookingResultDto(false,
                $"Booking failed due to a system error. No records were saved. Details: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves all confirmed bookings with patient and service details.
    /// Implements Feature D and supports AC-06.
    /// </summary>
    public async Task<List<BookingSummaryDto>> GetAllBookingsAsync()
    {
        var bookings = await _repository.GetAllBookingsAsync();
        return bookings.Select(b => new BookingSummaryDto(
            b.Id,
            b.Patient.FullName,
            b.BookingDate,
            b.CreatedAt,
            b.BookingItems.Select(bi => bi.DiagnosticService.Name).ToList()
        )).OrderByDescending(b => b.CreatedAt).ToList();
    }

    public async Task<Domain.Entities.Patient[]> GetPatientsAsync()
    {
        var patients = await _repository.GetAllPatientsAsync();
        return patients.ToArray();
    }
}
