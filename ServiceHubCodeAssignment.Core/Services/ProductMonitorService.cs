using ServiceHubCodeAssignment.Core.Readers;
using ServiceHubCodeAssignment.Domain.Models;

namespace ServiceHubCodeAssignment.Core.Services;

public sealed class ProductMonitorService(
    IProductReader reader,
    Func<string, DateTime>? getLastWriteTimeUtc = null) : IProductMonitorService
{
    private readonly Func<string, DateTime> _getLastWriteTimeUtc =
        getLastWriteTimeUtc ?? File.GetLastWriteTimeUtc;

    private PeriodicTimer? _periodicTimer;
    private Task? _monitorTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private DateTime _lastWriteTime;

    public event EventHandler<ProductCatalog>? CatalogChanged;
    public event EventHandler<Exception>? ErrorOccurred;

    public void Start(string filePath, int intervalMilliseconds = 1000)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        _cancellationTokenSource = new CancellationTokenSource();
        _periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMilliseconds));
        _lastWriteTime = File.GetLastWriteTimeUtc(filePath);
        _monitorTask = MonitorAsync(filePath, _cancellationTokenSource.Token);
    }

    private async Task MonitorAsync(string filePath, CancellationToken cancellationToken)
    {
        while (await _periodicTimer!.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                DateTime writeTime = _getLastWriteTimeUtc(filePath);

                if (writeTime == _lastWriteTime)
                    continue;

                _lastWriteTime = writeTime;

                ProductCatalog catalog = await reader.ReadAsync(filePath, cancellationToken);
                CatalogChanged?.Invoke(this, catalog);
            }
            catch (OperationCanceledException)
            {
                // Expected when shutting down.
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Dispose();
        }

        _periodicTimer?.Dispose();

        if (_monitorTask is not null)
            await _monitorTask;
    }
}
