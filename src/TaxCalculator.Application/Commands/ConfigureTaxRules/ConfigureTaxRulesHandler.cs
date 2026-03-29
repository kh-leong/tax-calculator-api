using MediatR;
using TaxCalculator.Application.Common;
using TaxCalculator.Domain.Entities;
using TaxCalculator.Domain.Interfaces;
using TaxCalculator.Domain.ValueObjects;

namespace TaxCalculator.Application.Commands.ConfigureTaxRules;

public class ConfigureTaxRulesHandler(ICountryTaxConfigurationRepository repository) : IRequestHandler<ConfigureTaxRulesCommand, Result<Unit>>
{
    private readonly ICountryTaxConfigurationRepository _repository = repository;

    public async Task<Result<Unit>> Handle(ConfigureTaxRulesCommand request, CancellationToken cancellationToken)
    {
        var countryCode = CountryCode.Create(request.CountryCode);

        var taxItems = request.TaxItems.Select(MapToDomainTaxItem).ToList();

        var configuration = new CountryTaxConfiguration(countryCode, taxItems);

        await _repository.UpsertAsync(configuration, cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }

    private static TaxItem MapToDomainTaxItem(TaxItemDto dto) => dto switch
    {
        FixedTaxItemDto f => new FixedTaxItem(f.Name, f.Amount),
        FlatRateTaxItemDto f => new FlatRateTaxItem(f.Name, f.Rate),
        ProgressiveTaxItemDto p => new ProgressiveTaxItem(
            p.Name,
            [.. p.Intervals.Select(b => new ProgressiveInterval(b.Threshold, b.Rate))]),
        _ => throw new InvalidOperationException($"Unknown tax item type: {dto.GetType().Name}")
    };
}
