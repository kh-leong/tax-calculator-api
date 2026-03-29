namespace TaxCalculator.Application.Queries.CalculateTax;

public record CalculateTaxResponse(
    decimal GrossSalary,
    decimal TaxableBase,
    decimal TotalTaxes,
    decimal NetSalary,
    List<TaxBreakdownItemResponse> Breakdown);

public record TaxBreakdownItemResponse(string Name, string Type, decimal Amount);
