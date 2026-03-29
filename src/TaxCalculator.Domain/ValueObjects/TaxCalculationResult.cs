namespace TaxCalculator.Domain.ValueObjects;

public record TaxCalculationResult(
    decimal GrossSalary,
    decimal TaxableBase,
    decimal TotalTaxes,
    decimal NetSalary,
    IReadOnlyList<TaxBreakdownItem> Breakdown);

public record TaxBreakdownItem(string Name, string Type, decimal Amount);
