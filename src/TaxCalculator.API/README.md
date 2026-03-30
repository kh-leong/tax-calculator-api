# API Layer

## Design

1) TaxController
	- Exposes API endpoints for managing tax configurations and performing tax calculations
	- Deserialises requests, uses MediatR to send commands and queries to the Application layer
	- Maps application results to appropriate HTTP responses (e.g. 200 OK for success, 400 Bad Request for validation errors, 404 Not Found for missing configurations)
	- Keeps controller logic minimal by delegating business logic to the Application layer
	- `ConfigureTaxRulesRequest` is a separate record from the command because the route parameter (country code) is not part of the request body
		- It cannot be included in the same record as the command without coupling the API layer to the Application layer's command structure
2) Scalar.AspNetCore
	- Replaces Swashbuckle for API documentation and testing