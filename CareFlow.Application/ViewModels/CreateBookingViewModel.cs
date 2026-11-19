using System.Collections.ObjectModel;
using CareFlow.Application.DTOs;
using CareFlow.Application.Services;
using CareFlow.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareFlow.Application.ViewModels;

public partial class CreateBookingViewModel : ObservableObject
{
    private readonly BookingService _bookingService;

    public CreateBookingViewModel(BookingService bookingService)
    {
        _bookingService = bookingService;
        _selectedDate = DateTimeOffset.Now;
    }

    // ---- Patients ----
    [ObservableProperty] private ObservableCollection<Patient> _patients = new();
    [ObservableProperty] private Patient? _selectedPatient;

    // ---- Date ----
    [ObservableProperty] private DateTimeOffset _selectedDate;

    // ---- Service Availability ----
    [ObservableProperty] private ObservableCollection<ServiceAvailabilityDto> _availableServices = new();

    // ---- Selected Services (checkboxes) ----
    [ObservableProperty] private ObservableCollection<SelectableService> _selectableServices = new();

    // ---- Feedback ----
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isSuccess;
    [ObservableProperty] private bool _isLoading;

    partial void OnSelectedDateChanged(DateTimeOffset value)
    {
        _ = RefreshAvailabilityAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        var patients = await _bookingService.GetPatientsAsync();
        Patients = new ObservableCollection<Patient>(patients);
        await RefreshAvailabilityAsync();
    }

    public async Task RefreshAvailabilityAsync()
    {
        var date = DateOnly.FromDateTime(SelectedDate.DateTime);
        var availability = await _bookingService.GetAvailabilityAsync(date);
        AvailableServices = new ObservableCollection<ServiceAvailabilityDto>(availability);
        SelectableServices = new ObservableCollection<SelectableService>(
            availability.Select(a => new SelectableService
            {
                ServiceId = a.ServiceId,
                ServiceName = a.ServiceName,
                Remaining = a.Remaining,
                IsSelected = false
            }));
    }

    [RelayCommand]
    public async Task SubmitBookingAsync()
    {
        StatusMessage = string.Empty;
        IsLoading = true;

        try
        {
            if (SelectedPatient == null)
            {
                StatusMessage = "Please select a patient.";
                IsSuccess = false;
                return;
            }

            var selectedIds = SelectableServices
                .Where(s => s.IsSelected)
                .Select(s => s.ServiceId)
                .ToList();

            if (selectedIds.Count == 0)
            {
                StatusMessage = "Please select at least one diagnostic service.";
                IsSuccess = false;
                return;
            }

            var date = DateOnly.FromDateTime(SelectedDate.DateTime);
            var result = await _bookingService.CreateBookingAsync(
                SelectedPatient.Id, date, selectedIds);

            StatusMessage = result.Message;
            IsSuccess = result.Success;

            if (result.Success)
            {
                // Reset selections
                foreach (var s in SelectableServices)
                    s.IsSelected = false;

                await RefreshAvailabilityAsync();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class SelectableService : ObservableObject
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Remaining { get; set; }

    [ObservableProperty]
    private bool _isSelected;

    public string DisplayText => $"{ServiceName} ({Remaining} remaining)";
}
