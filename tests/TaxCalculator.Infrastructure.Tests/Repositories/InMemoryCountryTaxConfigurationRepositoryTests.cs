using TaxCalculator.Domain.Entities;
using TaxCalculator.Domain.ValueObjects;
using TaxCalculator.Infrastructure.Repositories;

namespace TaxCalculator.Infrastructure.Tests.Repositories;

public class InMemoryCountryTaxConfigurationRepositoryTests
{
    private readonly InMemoryCountryTaxConfigurationRepository _repository = new();

    private static CountryTaxConfiguration CreateConfiguration(string countryCode = "DE")
    {
        var code = CountryCode.Create(countryCode);
        var items = new List<TaxItem> { new FixedTaxItem("Tax", 100m) };
        return new CountryTaxConfiguration(code, items);
    }

    [Fact]
    public async Task GetByCountryCodeAsync_NonExistentCode_ReturnsNull()
    {
        var code = CountryCode.Create("DE");

        var result = await _repository.GetByCountryCodeAsync(code, TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpsertAsync_NewConfiguration_CanBeRetrieved()
    {
        var config = CreateConfiguration("DE");

        await _repository.UpsertAsync(config, TestContext.Current.CancellationToken);
        var result = await _repository.GetByCountryCodeAsync(config.Code, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(config.Code, result.Code);
        Assert.Equal(config.Items.Count, result.Items.Count);
    }

    [Fact]
    public async Task UpsertAsync_ExistingConfiguration_UpdatesEntry()
    {
        var code = CountryCode.Create("DE");
        var original = new CountryTaxConfiguration(code, [new FixedTaxItem("Tax", 100m)]);
        var updated = new CountryTaxConfiguration(code, [new FixedTaxItem("Tax", 200m)]);

        await _repository.UpsertAsync(original, TestContext.Current.CancellationToken);
        await _repository.UpsertAsync(updated, TestContext.Current.CancellationToken);
        var result = await _repository.GetByCountryCodeAsync(code, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        var fixedItem = Assert.IsType<FixedTaxItem>(result.Items[0]);
        Assert.Equal(200m, fixedItem.Amount);
    }

    [Fact]
    public async Task UpsertAsync_MultipleCountries_StoresIndependently()
    {
        var configDE = CreateConfiguration("DE");
        var configUS = CreateConfiguration("US");

        await _repository.UpsertAsync(configDE, TestContext.Current.CancellationToken);
        await _repository.UpsertAsync(configUS, TestContext.Current.CancellationToken);

        var resultDE = await _repository.GetByCountryCodeAsync(configDE.Code, TestContext.Current.CancellationToken);
        var resultUS = await _repository.GetByCountryCodeAsync(configUS.Code, TestContext.Current.CancellationToken);

        Assert.NotNull(resultDE);
        Assert.NotNull(resultUS);
        Assert.Equal("DE", resultDE.Code.Value);
        Assert.Equal("US", resultUS.Code.Value);
    }

    [Fact]
    public async Task GetByCountryCodeAsync_AfterUpsert_ReturnsSameInstance()
    {
        var config = CreateConfiguration("DE");

        await _repository.UpsertAsync(config, TestContext.Current.CancellationToken);
        var result = await _repository.GetByCountryCodeAsync(config.Code, TestContext.Current.CancellationToken);

        Assert.Same(config, result);
    }

    [Fact]
    public async Task UpsertAsync_NullConfiguration_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _repository.UpsertAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetByCountryCodeAsync_NullCode_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _repository.GetByCountryCodeAsync(null!, TestContext.Current.CancellationToken));
    }
}
