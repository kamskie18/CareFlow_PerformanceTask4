namespace CareFlow.Domain.Entities;

public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }

    public string FullName => $"{LastName}, {FirstName}";

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
