using Avalonia.Controls;

namespace CareFlow.Desktop.Views;

public partial class BookingListView : UserControl
{
    public BookingListView()
    {
        InitializeComponent();
    }

    protected override async void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataContext is CareFlow.Application.ViewModels.BookingListViewModel vm)
        {
            await vm.LoadBookingsCommand.ExecuteAsync(null);
        }
    }
}
