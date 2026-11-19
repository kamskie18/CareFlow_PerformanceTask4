using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareFlow.Application.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public AvailabilityViewModel Availability { get; }
    public CreateBookingViewModel CreateBooking { get; }
    public BookingListViewModel BookingList { get; }

    [ObservableProperty]
    private int _selectedTabIndex;

    [RelayCommand]
    public void SelectTab(int index)
    {
        SelectedTabIndex = index;
    }

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
