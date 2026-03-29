using TaxCalculator.Domain.Entities;
using TaxCalculator.Domain.ValueObjects;

namespace TaxCalculator.Domain.Interfaces;

public interface ICountryTaxConfigurationRepository
{
    Task<CountryTaxConfiguration?> GetByCountryCodeAsync(CountryCode code, CancellationToken token = default);
    Task UpsertAsync(CountryTaxConfiguration configuration, CancellationToken token = default);
}
