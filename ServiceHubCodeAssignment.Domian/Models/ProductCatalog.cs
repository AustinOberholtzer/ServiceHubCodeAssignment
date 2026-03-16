using ServiceHubCodeAssignment.Domian.Models;

namespace ServiceHubCodeAssignment.Domain.Models;

public class ProductCatalog
{
    public IReadOnlyList<Product> Products { get; init; } = [];
}

