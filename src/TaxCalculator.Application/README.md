# Application Layer

## Design

1) CQRS pattern with MediatR
	- Commands for modifying data (e.g., upserting tax configuration)
	- Queries for retrieving data (e.g., getting tax configuration, calculating tax)
	- Clear separation of concerns and single responsibility for each handler
	- MediatR pipeline allows for easy addition of cross-cutting concerns (e.g. logging, validation) without cluttering handlers
2) Validation
	- FluentValidation used for validating command and query inputs
	- Ensures that invalid data is caught early and provides clear error messages
	- Validation logic is kept separate from handlers, adhering to single responsibility principle
	- Runs before handlers are executed, preventing unnecessary processing of invalid requests
3) Dependency Injection
	- Handlers depend on abstractions (e.g. `ICountryTaxConfigurationRepository`) rather than concrete implementations
	- Allows for easy swapping of implementations (e.g. in-memory vs database) without affecting handler logic
	- Promotes testability by allowing for mocking of dependencies in unit tests
4) TaxItemDto
	- Uses System.Text.Json `[JsonPolymorphic]` deserialisation using `type` property to determine the specific tax item type
	- Keeps serialisation concerns in the Application layer, allowing domain models to remain clean and focused on business logic
	- Explicit mapping to domain models without using an automapper. The project is currently simple enough to handle it with a simple function
		- Consider introducing a mapping library when project complexity grows
5) Result<T>
	- Custom result type to encapsulate success/failure of operations along with error messages
	- Controller maps error codes to HTTP status codes (e.g., 400 for validation errors, 404 for not found)
