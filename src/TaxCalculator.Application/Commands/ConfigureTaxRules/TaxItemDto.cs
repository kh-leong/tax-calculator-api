using System.Text.Json.Serialization;
using TaxCalculator.Domain.Constants;

namespace TaxCalculator.Application.Commands.ConfigureTaxRules;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(FixedTaxItemDto), TaxTypeConstants.Fixed)]
[JsonDerivedType(typeof(FlatRateTaxItemDto), TaxTypeConstants.FlatRate)]
[JsonDerivedType(typeof(ProgressiveTaxItemDto), TaxTypeConstants.Progressive)]
public abstract record TaxItemDto(string Name);

public record FixedTaxItemDto(string Name, decimal Amount) : TaxItemDto(Name);

public record FlatRateTaxItemDto(string Name, decimal Rate) : TaxItemDto(Name);

public record ProgressiveTaxItemDto(
    string Name,
    List<ProgressiveIntervalDto> Intervals) : TaxItemDto(Name);

public record ProgressiveIntervalDto(decimal? Threshold, decimal Rate);
