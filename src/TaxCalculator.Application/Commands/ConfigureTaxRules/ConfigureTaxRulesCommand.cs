using MediatR;
using TaxCalculator.Application.Common;

namespace TaxCalculator.Application.Commands.ConfigureTaxRules;

public record ConfigureTaxRulesCommand(
    string CountryCode,
    List<TaxItemDto> TaxItems) : IRequest<Result<Unit>>;
