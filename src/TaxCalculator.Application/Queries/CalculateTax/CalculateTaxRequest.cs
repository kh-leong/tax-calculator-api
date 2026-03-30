using MediatR;
using TaxCalculator.Application.Common;

namespace TaxCalculator.Application.Queries.CalculateTax;

public record CalculateTaxRequest(
    string CountryCode,
    decimal GrossSalary) : IRequest<Result<CalculateTaxResponse>>;
