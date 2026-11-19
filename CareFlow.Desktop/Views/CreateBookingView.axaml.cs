using Avalonia.Controls;

namespace CareFlow.Desktop.Views;

public partial class CreateBookingView : UserControl
{
    public CreateBookingView()
    {
        InitializeComponent();
    }

    protected override async void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataContext is CareFlow.Application.ViewModels.CreateBookingViewModel vm)
        {
            await vm.LoadDataCommand.ExecuteAsync(null);
        }
    }
}
