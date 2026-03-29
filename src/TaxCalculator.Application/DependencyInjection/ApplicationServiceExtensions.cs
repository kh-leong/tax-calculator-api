using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaxCalculator.Application.Behaviors;
using TaxCalculator.Application.Common;

namespace TaxCalculator.Application.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceExtensions).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
