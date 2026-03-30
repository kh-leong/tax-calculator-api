using Microsoft.Extensions.DependencyInjection;
using TaxCalculator.Domain.Interfaces;
using TaxCalculator.Infrastructure.Repositories;

namespace TaxCalculator.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ICountryTaxConfigurationRepository,
            InMemoryCountryTaxConfigurationRepository>();

        return services;
    }
}
