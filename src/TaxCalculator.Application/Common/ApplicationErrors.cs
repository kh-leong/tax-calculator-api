namespace TaxCalculator.Application.Common;

public static class ApplicationErrors
{
    public const string CountryNotConfiguredCode = "CountryNotConfigured";
    public const string ValidationFailedCode = "ValidationFailed";

    public static Error CountryNotConfigured(string code) =>
        new(CountryNotConfiguredCode, $"No tax configuration found for country '{code}'.");

    public static Error ValidationFailed(string message) =>
        new(ValidationFailedCode, message);
}
