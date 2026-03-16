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
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException(nameof(fileName), "The product catalog file name must be provided.");
        }

        fileName = Path.GetFullPath(fileName, AppContext.BaseDirectory);

        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException("The product catalog was not found.");
        }

        try
        {
            await using FileStream stream = File.OpenRead(fileName);

            return await JsonSerializer.DeserializeAsync<ProductCatalog>(stream, s_jsonOptions, cancellationToken)
                   ?? new ProductCatalog();
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"The product catalog contains invalid JSON and could not be deserialized.", ex);
        }
        catch (IOException ex)
        {
            throw new IOException("The product catalog could not be read.", ex);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred when reading the product catalog.", ex);
        }
    }
}

