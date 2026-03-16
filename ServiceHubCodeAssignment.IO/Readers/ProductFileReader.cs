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

    public async Task<ProductCatalog> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The product catalog at {filePath} was not found.");
        }

        try
        {
            await using FileStream stream = File.OpenRead(filePath);

            return await JsonSerializer.DeserializeAsync<ProductCatalog>(stream, s_jsonOptions, cancellationToken)
                   ?? new ProductCatalog();
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"The product catalog at '{filePath}' contains invalid JSON and could not be deserialized.", ex);
        }
        catch (IOException ex)
        {
            throw new IOException($"The product catalog at '{filePath}' could not be read.", ex);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"An unexpected error occurred when reading the product catalog at '{filePath}'.", ex);
        }
    }
}

