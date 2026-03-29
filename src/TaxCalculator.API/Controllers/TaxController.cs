using Microsoft.AspNetCore.Mvc;

namespace TaxCalculator.API.Controllers;

[ApiController]
[Route("api/tax")]
public class TaxController : ControllerBase
{
    [HttpPost("rules/{countryCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfigureTaxRules(string countryCode, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    [HttpGet("calculate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CalculateTax([FromQuery] string countryCode, [FromQuery] decimal grossSalary, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}
