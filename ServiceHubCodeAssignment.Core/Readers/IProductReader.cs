using ServiceHubCodeAssignment.Domain.Models;

namespace ServiceHubCodeAssignment.Core.Readers;

public interface IProductReader
{
    Task<ProductCatalog> ReadAsync(string fileName, CancellationToken cancellationToken = default);
}

