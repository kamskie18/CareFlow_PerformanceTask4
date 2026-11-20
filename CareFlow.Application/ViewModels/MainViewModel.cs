using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareFlow.Application.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public AvailabilityViewModel Availability { get; }
    public CreateBookingViewModel CreateBooking { get; }
    public BookingListViewModel BookingList { get; }

    [ObservableProperty]
    private int _selectedViewIndex = 0;

    public MainViewModel(
        AvailabilityViewModel availability,
        CreateBookingViewModel createBooking,
        BookingListViewModel bookingList)
    {
        Availability = availability;
        CreateBooking = createBooking;
        BookingList = bookingList;

        _ = Availability.LoadAvailabilityAsync();
    }

    [RelayCommand]
    public async Task ShowAvailabilityAsync()
    {
        SelectedViewIndex = 0;
        await Availability.LoadAvailabilityAsync();
    }

    [RelayCommand]
    public async Task ShowCreateBookingAsync()
    {
        SelectedViewIndex = 1;
        await CreateBooking.LoadDataAsync();
    }

    [RelayCommand]
    public async Task ShowBookingListAsync()
    {
        SelectedViewIndex = 2;
        await BookingList.LoadBookingsAsync();
    }
}
