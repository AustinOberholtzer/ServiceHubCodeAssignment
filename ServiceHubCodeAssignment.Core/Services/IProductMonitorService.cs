using ServiceHubCodeAssignment.Domain.Models;

namespace ServiceHubCodeAssignment.Core.Services;

public interface IProductMonitorService : IAsyncDisposable
{
    event EventHandler<ProductCatalog>? CatalogChanged;

    void Start(string filePath);
}

