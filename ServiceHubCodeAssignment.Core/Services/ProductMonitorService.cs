using ServiceHubCodeAssignment.Core.Readers;
using ServiceHubCodeAssignment.Domain.Models;

namespace ServiceHubCodeAssignment.Core.Services;

public sealed class ProductMonitorService(IProductReader reader) : IProductMonitorService
{
    private PeriodicTimer? _periodicTimer;
    private Task? _monitorTask;
    private CancellationTokenSource? _cenCancellationTokenSource;
    private DateTime _lastWriteTime;

    public event EventHandler<ProductCatalog>? CatalogChanged;

    public void Start(string filePath, int intervalMilliseconds = 1000)
    {
        _cenCancellationTokenSource = new CancellationTokenSource();
        _periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMilliseconds));
        _lastWriteTime = File.GetLastWriteTimeUtc(filePath);

        _monitorTask = MonitorAsync(filePath, _cenCancellationTokenSource.Token);
    }

    private async Task MonitorAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            while (await _periodicTimer!.WaitForNextTickAsync(cancellationToken))
            {
                DateTime writeTime = File.GetLastWriteTimeUtc(filePath);

                if (writeTime == _lastWriteTime)
                    continue;

                _lastWriteTime = writeTime;

                ProductCatalog catalog = await reader.ReadAsync(filePath, cancellationToken);
                CatalogChanged?.Invoke(this, catalog);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when shutting down.
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_cenCancellationTokenSource is not null)
        {
            await _cenCancellationTokenSource.CancelAsync();
            _cenCancellationTokenSource.Dispose();
        }

        _periodicTimer?.Dispose();

        if (_monitorTask is not null)
        {
            await _monitorTask;
        }
    }
}

