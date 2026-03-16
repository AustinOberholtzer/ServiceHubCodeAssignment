using System.Text.Json;
using ServiceHubCodeAssignment.Core.Readers;
using ServiceHubCodeAssignment.Domain.Models;

namespace ServiceHubCodeAssignment.IO.Readers;

public sealed class ProductFileReader : IProductReader
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ProductCatalog> ReadAsync(string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        fileName = Path.GetFullPath(fileName, AppContext.BaseDirectory);

        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException("The product catalog file was not found.", fileName);
        }

        try
        {
            await using FileStream stream = File.OpenRead(fileName);

            return await JsonSerializer.DeserializeAsync<ProductCatalog>(stream, s_jsonOptions, cancellationToken)
                   ?? new ProductCatalog();
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("The product catalog contains invalid JSON and could not be deserialized.", ex);
        }
    }
}

