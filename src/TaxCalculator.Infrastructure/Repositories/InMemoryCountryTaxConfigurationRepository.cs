using System.Collections.Concurrent;
using TaxCalculator.Domain.Entities;
using TaxCalculator.Domain.Interfaces;
using TaxCalculator.Domain.ValueObjects;

namespace TaxCalculator.Infrastructure.Repositories;

public class InMemoryCountryTaxConfigurationRepository : ICountryTaxConfigurationRepository
{
    private readonly ConcurrentDictionary<string, CountryTaxConfiguration> _store = new();

    public Task<CountryTaxConfiguration?> GetByCountryCodeAsync(CountryCode code, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(code);
        token.ThrowIfCancellationRequested();
        _store.TryGetValue(code.Value, out var config);
        // TODO: this is where we could add future external providers. For now, we just return the in-memory store.

        // TODO: return a copy of the configuration to prevent external modifications. For simplicity, we return the stored instance here.
        return Task.FromResult(config);
    }

    public Task UpsertAsync(CountryTaxConfiguration configuration, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        token.ThrowIfCancellationRequested();
        _store.AddOrUpdate(
            configuration.Code.Value,
            configuration,
            (_, _) => configuration);
        return Task.CompletedTask;
    }
}
