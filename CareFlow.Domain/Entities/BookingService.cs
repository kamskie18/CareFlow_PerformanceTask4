namespace CareFlow.Domain.Entities;

/// <summary>
/// Join entity linking a Booking to a DiagnosticService.
/// Named BookingItem to avoid ambiguity with the application-layer BookingService class.
/// </summary>
public class BookingItem
{
    public int BookingId { get; set; }
    public int DiagnosticServiceId { get; set; }

    public Booking Booking { get; set; } = null!;
    public DiagnosticService DiagnosticService { get; set; } = null!;
}
