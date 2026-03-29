# Domain Layer

## Design

1) CountryCode
	- validate format (2-3 letters only)
	- normalise to uppercase to ensure consistency and prevent duplicates (e.g., "de" vs "DE")
2) TaxItem
	- abstract record with 3 subtypes: FixedTaxItem, FlatRateTaxItem, ProgressiveTaxItem
	- supports pattern matching and clear separation of tax types
	- each subtype has specific properties relevant to its tax type (e.g., Rate for FlatRateTaxItem, Intervals for ProgressiveTaxItem)
3) ProgressiveInterval
	- nullable `Threshold` with null meaning no upper bound
	- `ProgressiveTaxItem` sorts intervals by threshold on construction and enforces that the last interval has a null `Threshold`
4) CountryTaxConfiguration (aggregate root)
	- enforces that at least 1 tax item must exist and at most 1 progressive tax item is allowed
	- constructor validates these rules and throws exceptions for invalid configurations
	- entire configuration is meant to be replaced atomically via upsert
5) TaxCalculationService
	- static service that performs tax calculations based on a given `CountryTaxConfiguration`
		- nothing to mock. Pure function that takes configuration and gross salary as input and returns calculation result
	- follows the defined calculation logic in the specified order (fixed taxes, then flat rate taxes, then progressive tax)
	- returns `TaxCalculationResult` with detailed breakdown of each tax item and totals
6) TaxCalculationResult + TaxBreakdownItem
	- immutable records representing the result of a tax calculation
	- domain value objects
7) ICountryTaxConfigurationRepository
	- interface for repository to manage `CountryTaxConfiguration` instances
	- methods for getting configuration by country code and upserting configuration
	- allows for different implementations (e.g., in-memory, database) without coupling domain logic to persistence details