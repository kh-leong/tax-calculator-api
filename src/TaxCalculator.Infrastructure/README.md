# Infrastructure Layer

## Design

1) InMemoryCountryTaxConfigurationRepository
	- simple in-memory implementation of `ICountryTaxConfigurationRepository`
	- uses a thread-safe dictionary for concurrent API requests
	- JSON file storage was considered but rejected due to issues with concurrent access and complexity of file locking
	- suitable for testing and development purposes, not for production use
	- clean architecture makes it trivial to swap for a real database without affecting domain logic