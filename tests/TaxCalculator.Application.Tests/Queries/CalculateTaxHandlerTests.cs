using FluentAssertions;
using NSubstitute;
using TaxCalculator.Application.Queries.CalculateTax;
using TaxCalculator.Domain.Entities;
using TaxCalculator.Domain.Interfaces;
using TaxCalculator.Domain.ValueObjects;

namespace TaxCalculator.Application.Tests.Queries;

public class CalculateTaxHandlerTests
{
    private readonly ICountryTaxConfigurationRepository _repository;
    private readonly CalculateTaxHandler _handler;

    public CalculateTaxHandlerTests()
    {
        _repository = Substitute.For<ICountryTaxConfigurationRepository>();
        _handler = new CalculateTaxHandler(_repository);
    }

    [Fact]
    public async Task Handle_CountryNotConfigured_ReturnsFailure()
    {
        _repository.GetByCountryCodeAsync(Arg.Any<CountryCode>(), Arg.Any<CancellationToken>())
            .Returns((CountryTaxConfiguration?)null);

        var query = new CalculateTaxRequest("XX", 50000);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("CountryNotConfigured");
    }

    [Fact]
    public async Task Handle_FullSpecExample_ReturnsCorrectResponse()
    {
        var config = new CountryTaxConfiguration(
            CountryCode.Create("DE"),
            [
                new FixedTaxItem("CommunityTax", 1500),
                new FixedTaxItem("RadioTax", 500),
                new FlatRateTaxItem("PensionTax", 0.20m),
                new ProgressiveTaxItem("IncomeTax",
                [
                    new ProgressiveInterval(10000, 0.00m),
                    new ProgressiveInterval(30000, 0.20m),
                    new ProgressiveInterval(null, 0.40m)
                ])
            ]);

        _repository.GetByCountryCodeAsync(Arg.Any<CountryCode>(), Arg.Any<CancellationToken>())
            .Returns(config);

        var query = new CalculateTaxRequest("DE", 62000);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value!;
        response.GrossSalary.Should().Be(62000);
        response.TaxableBase.Should().Be(60000);
        response.TotalTaxes.Should().Be(30000);
        response.NetSalary.Should().Be(32000);
        response.Breakdown.Should().HaveCount(4);
    }
}
