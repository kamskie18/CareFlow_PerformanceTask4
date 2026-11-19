using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CareFlow.Application.Interfaces;
using CareFlow.Application.ViewModels;
using CareFlow.Infrastructure.Data;
using CareFlow.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AppBookingService = CareFlow.Application.Services.BookingService;

namespace CareFlow.Desktop;

public partial class App : Avalonia.Application
{
    public static ServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        // --- Database ---
        services.AddDbContext<CareFlowDbContext>(options =>
            options.UseNpgsql(
                "Host=localhost;Database=careflow_db;Username=postgres;Password=password"));

        // --- Repository ---
        services.AddScoped<IBookingRepository, BookingRepository>();

        // --- Application Service ---
        services.AddScoped<AppBookingService>();

        // --- ViewModels ---
        services.AddTransient<AvailabilityViewModel>();
        services.AddTransient<CreateBookingViewModel>();
        services.AddTransient<BookingListViewModel>();
        services.AddTransient<MainViewModel>();

        Services = services.BuildServiceProvider();

        // Ensure database is created and migrated
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CareFlowDbContext>();
            db.Database.Migrate();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainVm = Services.GetRequiredService<MainViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
