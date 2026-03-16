using ServiceHubCodeAssignment.Core.Services;
using ServiceHubCodeAssignment.Domain.Models;
using ServiceHubCodeAssignment.IO.Readers;

namespace ServiceHubCodeAssignment.Test;

[TestFixture]
public class ProductMonitorServiceTests
{
    private const int TimeoutSeconds = 5;

    [Test]
    public async Task ProductMonitorTest()
    {
        string filePath = Path.GetFullPath("products.json", AppContext.BaseDirectory);
        string originalContent = await File.ReadAllTextAsync(filePath);

        var firstCheckSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var catalogChangedSource = new TaskCompletionSource<ProductCatalog>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var service = new ProductMonitorService(new ProductFileReader());

        service.FileChecked += (_, _) => firstCheckSource.TrySetResult(true);
        service.CatalogChanged += (_, catalog) => catalogChangedSource.TrySetResult(catalog);

        try
        {
            service.Start(filePath);

            await firstCheckSource.Task.WaitAsync(TimeSpan.FromSeconds(TimeoutSeconds));

            Assert.That(catalogChangedSource.Task.IsCompleted, Is.False, "CatalogChanged should not be raised when the file has not changed.");

            await ModifyProductCatalog(originalContent, filePath);

            var changedCatalog = await catalogChangedSource.Task.WaitAsync(TimeSpan.FromSeconds(TimeoutSeconds));

            Assert.That(changedCatalog.Products, Is.Not.Empty);
            Assert.That(changedCatalog.Products[0].Name, Is.EqualTo("Chainsaws (Updated)"));
        }
        finally
        {
            await File.WriteAllTextAsync(filePath, originalContent);
        }
    }

    private static async Task ModifyProductCatalog(string originalContent, string filePath)
    {
        string modifiedContent = originalContent.Replace(
            "\"name\": \"Chainsaws\"",
            "\"name\": \"Chainsaws (Updated)\"");

        await File.WriteAllTextAsync(filePath, modifiedContent);
    }
}
