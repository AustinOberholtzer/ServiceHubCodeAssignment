namespace ServiceHubCodeAssignment.Application.Configuration;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public string ProductsFileName { get; init; } = "products.json";
    public int IntervalMilliseconds { get; init; } = 1000;
}

