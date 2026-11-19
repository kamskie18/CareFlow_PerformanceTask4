using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareFlow.Application.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public AvailabilityViewModel Availability { get; }
    public CreateBookingViewModel CreateBooking { get; }
    public BookingListViewModel BookingList { get; }

    [ObservableProperty]
    private ObservableObject _currentView;

    public MainViewModel(
        AvailabilityViewModel availability,
        CreateBookingViewModel createBooking,
        BookingListViewModel bookingList)
    {
        Availability = availability;
        CreateBooking = createBooking;
        BookingList = bookingList;

        _currentView = Availability;
        _ = Availability.LoadAvailabilityAsync();
    }

    [RelayCommand]
    public async Task ShowAvailabilityAsync()
    {
        CurrentView = Availability;
        await Availability.LoadAvailabilityAsync();
    }

    [RelayCommand]
    public async Task ShowCreateBookingAsync()
    {
        CurrentView = CreateBooking;
        await CreateBooking.LoadDataAsync();
    }

    [RelayCommand]
    public async Task ShowBookingListAsync()
    {
        CurrentView = BookingList;
        await BookingList.LoadBookingsAsync();
    }
}
