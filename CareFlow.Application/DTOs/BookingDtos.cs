namespace CareFlow.Application.DTOs;

public record ServiceAvailabilityDto(
    int ServiceId,
    string ServiceName,
    int DailyCapacity,
    int BookedCount,
    int Remaining);

public record BookingResultDto(bool Success, string Message, int? BookingId = null);

public record BookingSummaryDto(
    int BookingId,
    string PatientName,
    DateOnly BookingDate,
    DateTime CreatedAt,
    List<string> Services);
