using FluentAssertions;
using MediatR;
using NSubstitute;
using TaxCalculator.Application.Commands.ConfigureTaxRules;
using TaxCalculator.Domain.Entities;
using TaxCalculator.Domain.Interfaces;
using TaxCalculator.Domain.ValueObjects;

namespace TaxCalculator.Application.Tests.Commands;

public class ConfigureTaxRulesHandlerTests
{
    private readonly ICountryTaxConfigurationRepository _repository;
    private readonly ConfigureTaxRulesHandler _handler;

    public ConfigureTaxRulesHandlerTests()
    {
        _repository = Substitute.For<ICountryTaxConfigurationRepository>();
        _handler = new ConfigureTaxRulesHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_StoresConfigAndReturnsSuccess()
    {
        var command = new ConfigureTaxRulesCommand("DE",
        [
            new FixedTaxItemDto("Fee", 1000),
            new FlatRateTaxItemDto("Tax", 0.10m)
        ]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).UpsertAsync(
            Arg.Is<CountryTaxConfiguration>(c =>
                c.Code.Value == "DE" &&
                c.Items.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProgressiveItems_MapsCorrectly()
    {
        var command = new ConfigureTaxRulesCommand("US",
        [
            new ProgressiveTaxItemDto("Income",
            [
                new ProgressiveIntervalDto(10000, 0.10m),
                new ProgressiveIntervalDto(null, 0.30m)
            ])
        ]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).UpsertAsync(
            Arg.Is<CountryTaxConfiguration>(c =>
                c.Items.OfType<ProgressiveTaxItem>().Single().Intervals.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SecondCallSameCountry_CallsUpsert()
    {
        var command1 = new ConfigureTaxRulesCommand("DE", [new FixedTaxItemDto("Fee", 500)]);
        var command2 = new ConfigureTaxRulesCommand("DE", [new FixedTaxItemDto("NewFee", 1000)]);

        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);

        await _repository.Received(2).UpsertAsync(
            Arg.Any<CountryTaxConfiguration>(),
            Arg.Any<CancellationToken>());
    }
}
