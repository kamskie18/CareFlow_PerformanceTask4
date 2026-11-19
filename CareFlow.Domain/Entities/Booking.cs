namespace CareFlow.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateOnly BookingDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient Patient { get; set; } = null!;
    public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
}
