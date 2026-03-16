using ServiceHubCodeAssignment.Core.Readers;
using ServiceHubCodeAssignment.Domain.Models;

namespace ServiceHubCodeAssignment.Core.Services;

public sealed class ProductMonitorService(
    IProductReader reader) : IProductMonitorService
{
    private PeriodicTimer? _periodicTimer;
    private Task? _monitorTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private DateTime _lastWriteTime;

    public event EventHandler<ProductCatalog>? CatalogChanged;
    public event EventHandler<Exception>? ErrorOccurred;
    public event EventHandler<bool>? FileChecked;

    public void Start(string filePath, int intervalMilliseconds = 2000)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMilliseconds));
        _lastWriteTime = File.GetLastWriteTimeUtc(filePath);
        _monitorTask = MonitorAsync(filePath, _cancellationTokenSource.Token);
    }

    private async Task MonitorAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            while (await _periodicTimer!.WaitForNextTickAsync(cancellationToken))
            {
                bool changed = false;
                try
                {
                    DateTime writeTime = File.GetLastWriteTimeUtc(filePath);

                    if (writeTime != _lastWriteTime)
                    {
                        _lastWriteTime = writeTime;
                        changed = true;

                        ProductCatalog catalog = await reader.ReadAsync(filePath, cancellationToken);
                        
                        CatalogChanged?.Invoke(this, catalog);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is not OperationCanceledException)
                    {
                        ErrorOccurred?.Invoke(this, ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    FileChecked?.Invoke(this, changed);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the service is disposed
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
        {
            await _monitorTask;
        }
    }
}
