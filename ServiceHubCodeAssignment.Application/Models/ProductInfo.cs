using ServiceHubCodeAssignment.Domain.Models;

namespace ServiceHubCodeAssignment.Application.Models;

public sealed class ProductInfo
{
    public string Id { get; }
    public string Name { get; }
    public string? Description { get; }
    public string? Category { get; }
    public string? ImageUrl { get; }
    public IReadOnlyList<ProductInfo> Variants { get; }

    public ProductInfo(Product product)
    {
        Id = product.Id;
        Name = product.Name;
        Description = product.Description;
        Category = product.Category;
        ImageUrl = product.ImageUrl;
        Variants = product.Variants.Select(v => new ProductInfo(v)).ToList();
    }
}

