using FluentValidation;
using MediatR;
using TaxCalculator.Application.Common;

namespace TaxCalculator.Application.Behaviors;

public class ValidationBehavior<TRequest, T>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, Result<T>>
    where TRequest : IRequest<Result<T>>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<Result<T>> Handle(
        TRequest request,
        RequestHandlerDelegate<Result<T>> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var message = string.Join("; ", failures.Select(f => f.ErrorMessage));
            return Result<T>.Failure(ApplicationErrors.ValidationFailed(message));
        }

        return await next(cancellationToken);
    }
}
