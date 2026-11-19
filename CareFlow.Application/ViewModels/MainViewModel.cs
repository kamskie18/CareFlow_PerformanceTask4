using CommunityToolkit.Mvvm.ComponentModel;

namespace CareFlow.Application.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public AvailabilityViewModel Availability { get; }
    public CreateBookingViewModel CreateBooking { get; }
    public BookingListViewModel BookingList { get; }

    [ObservableProperty]
    private int _selectedTabIndex;

    public MainViewModel(
        AvailabilityViewModel availability,
        CreateBookingViewModel createBooking,
        BookingListViewModel bookingList)
    {
        Availability = availability;
        CreateBooking = createBooking;
        BookingList = bookingList;
    }
}
