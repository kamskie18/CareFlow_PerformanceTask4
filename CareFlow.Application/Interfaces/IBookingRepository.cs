using CareFlow.Domain.Entities;

namespace CareFlow.Application.Interfaces;

public interface IBookingRepository
{
    Task<List<Patient>> GetAllPatientsAsync();
    Task<List<DiagnosticService>> GetAllServicesAsync();
    Task<int> GetBookedCountAsync(int serviceId, DateOnly date);
    Task<Booking> CreateBookingAsync(int patientId, DateOnly date, List<int> serviceIds);
    Task<List<Booking>> GetAllBookingsAsync();
    Task<Booking?> GetBookingByIdAsync(int id);
}
