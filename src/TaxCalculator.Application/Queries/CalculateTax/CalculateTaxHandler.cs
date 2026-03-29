using MediatR;
using TaxCalculator.Application.Common;
using TaxCalculator.Domain.Interfaces;
using TaxCalculator.Domain.Services;
using TaxCalculator.Domain.ValueObjects;

namespace TaxCalculator.Application.Queries.CalculateTax;

public class CalculateTaxHandler(ICountryTaxConfigurationRepository repository) : IRequestHandler<CalculateTaxRequest, Result<CalculateTaxResponse>>
{
    private readonly ICountryTaxConfigurationRepository _repository = repository;

    public async Task<Result<CalculateTaxResponse>> Handle(CalculateTaxRequest request, CancellationToken cancellationToken)
    {
        var countryCode = CountryCode.Create(request.CountryCode);

        var config = await _repository.GetByCountryCodeAsync(countryCode, cancellationToken);
        if (config is null)
            return Result<CalculateTaxResponse>.Failure(
                ApplicationErrors.CountryNotConfigured(request.CountryCode));

        var result = TaxCalculationService.Calculate(config, request.GrossSalary);

        var response = new CalculateTaxResponse(
            result.GrossSalary,
            result.TaxableBase,
            result.TotalTaxes,
            result.NetSalary,
            [.. result.Breakdown.Select(b => new TaxBreakdownItemResponse(b.Name, b.Type, b.Amount))]);

        return Result<CalculateTaxResponse>.Success(response);
    }
}
