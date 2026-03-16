using ServiceHubCodeAssignment.Domain.Models;

namespace ServiceHubCodeAssignment.Core.Services;

public interface IProductMonitorService : IAsyncDisposable
{
    event EventHandler<ProductCatalog>? CatalogChanged;
    event EventHandler<Exception>? ErrorOccurred;

    void Start(string filePath, int intervalMilliseconds);
}

