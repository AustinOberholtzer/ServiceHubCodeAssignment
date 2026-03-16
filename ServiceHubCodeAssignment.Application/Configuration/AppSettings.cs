namespace ServiceHubCodeAssignment.Application.Configuration;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public string ProductsFilePath { get; init; } = "products.json";
}

