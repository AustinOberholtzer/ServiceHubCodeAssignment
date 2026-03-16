namespace ServiceHubCodeAssignment.Domain.Models;

public class Product
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
    public string? ImageUrl { get; init; }
    public IReadOnlyList<Product> Variants { get; init; } = [];
}

