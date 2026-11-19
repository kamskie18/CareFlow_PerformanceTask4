using System.Collections.ObjectModel;
using CareFlow.Application.DTOs;
using CareFlow.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareFlow.Application.ViewModels;

public partial class BookingListViewModel : ObservableObject
{
    private readonly BookingService _bookingService;

    public BookingListViewModel(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [ObservableProperty]
    private ObservableCollection<BookingSummaryDto> _bookings = new();

    [ObservableProperty]
    private BookingSummaryDto? _selectedBooking;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    public async Task LoadBookingsAsync()
    {
        IsLoading = true;
        try
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            Bookings = new ObservableCollection<BookingSummaryDto>(bookings);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
