using System.Collections.ObjectModel;
using CareFlow.Application.DTOs;
using CareFlow.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareFlow.Application.ViewModels;

public partial class AvailabilityViewModel : ObservableObject
{
    private readonly BookingService _bookingService;

    public AvailabilityViewModel(BookingService bookingService)
    {
        _bookingService = bookingService;
        _selectedDate = DateTimeOffset.Now;
    }

    [ObservableProperty]
    private DateTimeOffset _selectedDate;

    [ObservableProperty]
    private ObservableCollection<ServiceAvailabilityDto> _services = new();

    [ObservableProperty]
    private bool _isLoading;

    partial void OnSelectedDateChanged(DateTimeOffset value)
    {
        _ = LoadAvailabilityAsync();
    }

    [RelayCommand]
    public async Task LoadAvailabilityAsync()
    {
        IsLoading = true;
        try
        {
            var date = DateOnly.FromDateTime(SelectedDate.DateTime);
            var availability = await _bookingService.GetAvailabilityAsync(date);
            Services = new ObservableCollection<ServiceAvailabilityDto>(availability);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
