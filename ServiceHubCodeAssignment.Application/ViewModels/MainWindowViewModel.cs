using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using ServiceHubCodeAssignment.Application.Configuration;
using ServiceHubCodeAssignment.Application.Models;
using ServiceHubCodeAssignment.Core.Readers;
using ServiceHubCodeAssignment.Core.Services;
using ServiceHubCodeAssignment.Domian.Models;

namespace ServiceHubCodeAssignment.Application.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IProductReader _reader;
    private readonly IProductMonitorService _monitorService;
    private readonly AppSettings _settings;
    private readonly Dispatcher _dispatcher;

    [ObservableProperty]
    private ObservableCollection<ProductInfo> _products = [];

    [ObservableProperty]
    private ProductInfo? _selectedProduct;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _loading;

    public MainWindowViewModel(
        IProductReader reader,
        IProductMonitorService monitorService,
        IOptions<AppSettings> settings)
    {
        _reader = reader;
        _monitorService = monitorService;
        _settings = settings.Value;
        _dispatcher = System.Windows.Application.Current.Dispatcher;

        _monitorService.CatalogChanged += OnCatalogChanged;
        _monitorService.ErrorOccurred += OnErrorOccured;
    }

    public async Task InitializeAsync()
    {
        await LoadCatalogAsync();

        _monitorService.Start(_settings.ProductsFileName, _settings.IntervalMilliseconds);
    }

    private async Task LoadCatalogAsync()
    {
        try
        {
            Loading = true;

            StatusMessage = "Loading products...";

            var catalog = await _reader.ReadAsync(_settings.ProductsFileName);

            ApplyCatalog(catalog);
        }
        catch (Exception ex)
        {
            _dispatcher.Invoke(() => StatusMessage = $"Error: {ex.Message}");
        }

        Loading = false;
    }

    private void ApplyCatalog(ProductCatalog catalog)
    {
        var viewModels = catalog.Products
            .Select(p => new ProductInfo(p))
            .ToList();

        _dispatcher.Invoke(() =>
        {
            Products = new ObservableCollection<ProductInfo>(viewModels);

            StatusMessage = $"Last updated: {DateTime.Now:HH:mm:ss}";
        });
    }

    private void OnCatalogChanged(object? sender, ProductCatalog catalog)
    {
        ApplyCatalog(catalog);
    }

    private void OnErrorOccured(object? sender, Exception e)
    {
        _dispatcher.Invoke(() => StatusMessage = $"Error: {e.Message}");
    }
}

