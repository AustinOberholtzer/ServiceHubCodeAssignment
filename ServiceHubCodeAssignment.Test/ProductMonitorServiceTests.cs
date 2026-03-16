using Microsoft.Extensions.Time.Testing;
using ServiceHubCodeAssignment.Core.Services;
using ServiceHubCodeAssignment.Domain.Models;
using ServiceHubCodeAssignment.IO.Readers;

namespace ServiceHubCodeAssignment.Test;

[TestFixture]
public class ProductMonitorServiceTests
{
    [Test]
    public async Task Monitor_DetectsNoChangeOnFirstTick_ThenDetectsChangeOnSecondTick()
    {
        string filePath = Path.GetFullPath("products.json", AppContext.BaseDirectory);
        string originalContent = await File.ReadAllTextAsync(filePath);

        var timeProvider = new FakeTimeProvider();
        var interval = TimeSpan.FromSeconds(2);

        var taskCompletionSource = new TaskCompletionSource<ProductCatalog>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var service = new ProductMonitorService(new ProductFileReader(), timeProvider);

        service.CatalogChanged += (_, catalog) => taskCompletionSource.TrySetResult(catalog);

        try
        {
            service.Start(filePath);

            timeProvider.Advance(interval);

            await Task.Yield();

            Assert.That(taskCompletionSource.Task.IsCompleted, Is.False, "CatalogChanged should not be raised when the file has not changed.");

            await ModifyProductCatalog(originalContent, filePath);

            timeProvider.Advance(interval);

            var changedCatalog = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5));

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
