using TaxCalculator.Domain.Entities;
using TaxCalculator.Domain.ValueObjects;
using TaxCalculator.Domain.Constants;

namespace TaxCalculator.Domain.Services;

public static class TaxCalculationService
{
    public static TaxCalculationResult Calculate(CountryTaxConfiguration configuration, decimal grossSalary)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (grossSalary < 0)
            throw new ArgumentOutOfRangeException(nameof(grossSalary), grossSalary, "Gross salary cannot be negative.");

        var breakdown = new List<TaxBreakdownItem>();

        // Step 1: fixed taxes reduce the taxable base
        var fixedItems = configuration.Items.OfType<FixedTaxItem>().ToList();
        var fixedTotal = fixedItems.Sum(f => f.Amount);
        foreach (var item in fixedItems)
        {
            breakdown.Add(new TaxBreakdownItem(item.Name, TaxTypeConstants.Fixed, item.Amount));
        }

        var taxableBase = Math.Max(0, grossSalary - fixedTotal);

        // Step 2: flat rate taxes on taxable base
        var flatRateItems = configuration.Items.OfType<FlatRateTaxItem>().ToList();
        var flatRateTotal = 0m;
        foreach (var item in flatRateItems)
        {
            var amount = taxableBase * item.Rate;
            flatRateTotal += amount;
            breakdown.Add(new TaxBreakdownItem(item.Name, TaxTypeConstants.FlatRate, amount));
        }

        // Step 3: progressive tax on taxable base
        var progressiveItem = configuration.Items.OfType<ProgressiveTaxItem>().SingleOrDefault();
        var progressiveTotal = 0m;
        if (progressiveItem is not null)
        {
            progressiveTotal = CalculateProgressiveTax(progressiveItem, taxableBase);
            breakdown.Add(new TaxBreakdownItem(progressiveItem.Name, TaxTypeConstants.Progressive, progressiveTotal));
        }

        // Step 4: Totals
        var totalTaxes = fixedTotal + flatRateTotal + progressiveTotal;
        var netSalary = grossSalary - totalTaxes;

        return new TaxCalculationResult(grossSalary, taxableBase, totalTaxes, netSalary, breakdown.AsReadOnly());
    }

    private static decimal CalculateProgressiveTax(ProgressiveTaxItem item, decimal taxableBase)
    {
        var total = 0m;
        var previousThreshold = 0m;

        foreach (var interval in item.Intervals)
        {
            var upperBound = interval.Threshold ?? decimal.MaxValue;
            var taxableSlice = Math.Min(taxableBase, upperBound) - previousThreshold;

            if (taxableSlice > 0)
                total += taxableSlice * interval.Rate;

            previousThreshold = upperBound;

            if (taxableBase <= upperBound)
                break;
        }

        return total;
    }
}
