namespace CareFlow.Domain.Entities;

public class DiagnosticService
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DailyCapacity { get; set; }

    public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
}
