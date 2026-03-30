using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaxCalculator.Application.Commands.ConfigureTaxRules;
using TaxCalculator.Application.Common;
using TaxCalculator.Application.Queries.CalculateTax;

namespace TaxCalculator.API.Controllers;

[ApiController]
[Route("api/tax")]
public class TaxController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("rules/{countryCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfigureTaxRules(string countryCode, [FromBody] ConfigureTaxRulesRequest request, CancellationToken token)
    {
        var command = new ConfigureTaxRulesCommand(countryCode, request.TaxItems);
        var result = await _mediator.Send(command, token);

        if (result.IsSuccess)
            return Ok();

        return result.Error!.Code switch
        {
            ApplicationErrors.ValidationFailedCode => BadRequest(new { error = result.Error.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error.Message })
        };
    }

    [HttpGet("calculate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CalculateTax([FromQuery] string countryCode, [FromQuery] decimal grossSalary, CancellationToken token)
    {
        var query = new CalculateTaxRequest(countryCode, grossSalary);
        var result = await _mediator.Send(query, token);

        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error!.Code switch
        {
            ApplicationErrors.ValidationFailedCode => BadRequest(new { error = result.Error.Message }),
            ApplicationErrors.CountryNotConfiguredCode => NotFound(new { error = result.Error.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error.Message })
        };
    }
}

// keeping command request models here for simplicity.
// TODO: move to a separate namespace when the project grows.
public record ConfigureTaxRulesRequest(List<TaxItemDto> TaxItems);