namespace TaxCalculator.Application.Common;

public static class ApplicationErrors
{
    public static Error CountryNotConfigured(string code) =>
        new("CountryNotConfigured", $"No tax configuration found for country '{code}'.");

    public static Error ValidationFailed(string message) =>
        new("ValidationFailed", message);
}
