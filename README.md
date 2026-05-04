# Tax Calculator

A simplified tax calculation web API that allows configuring tax rules by country and calculates taxes based on those rules.

## Requirements

### In scope
- Configure tax rules per country
- Calculate tax based on configured rules
- Extensibility and clean architecture
- Core domain logic must be verifiable and testable
- Swagger/OpenAPI for endpoint testing
- Docker support

### Out of scope
- UI
- Real storage (in-memory only)
- Real external integrations

## Endpoints

### Configure Tax Rules

```
POST /api/tax/rules/{countryCode}
```

Following REST convention, the URL identifies which resource is being created or replaced. The body carries what to store.

Request body:
```json
{
  "taxItems": [
    { "type": "fixed",       "name": "Community Tax", "amount": 1500 },
    { "type": "fixed",       "name": "Radio Tax", "amount": 500 },
    { "type": "flatRate",    "name": "Pension Tax",   "rate": 0.20 },
    { "type": "progressive", "name": "Income Tax",
      "intervals": [
        { "threshold": 10000, "rate": 0.00 },
        { "threshold": 30000, "rate": 0.20 },
        { "threshold": null,  "rate": 0.40 }
      ]
    }
  ]
}
```

**Rules:**
- Exactly one configuration per country (upsert: replaces if it already exists)
- At least one tax item must be provided
- At most one progressive tax item is allowed

**Responses:** `200 OK`, `400 Bad Request`

### Calculate Tax

```
GET /api/tax/{countryCode}?grossSalary=62000
```

We use GET as the operation is idempotent and doesn't change anything in the server. The country code is part of the URL path following REST convention, and the gross salary is passed as a query parameter.

**Response `200 OK`:**
```json
{
  "grossSalary": 62000,
  "taxableBase": 60000,
  "totalTaxes": 30000,
  "netSalary": 32000,
  "breakdown": [
    { "name": "Community Tax", "type": "fixed",       "amount": 1500  },
    { "name": "Radio Tax",     "type": "fixed",       "amount": 500   },
    { "name": "Pension Tax",   "type": "flatRate",    "amount": 12000 },
    { "name": "Income Tax",    "type": "progressive", "amount": 16000 }
  ]
}
```

**Response `404 Not Found`:** returned when no configuration exists for the given country code.

**Response `400 Bad Request`:** returned for invalid input (empty country code, negative gross salary).

## Tax Calculation Logic

The calculation follows four steps in order:

**Step 1) Fixed taxes and taxable base**

- Fixed taxes are flat amounts deducted directly from gross salary.
- Multiple fixed taxes are allowed.
- They reduce the taxable base for all other tax types.

```
TaxableBase = max(0, GrossSalary - sum(all fixed tax amounts))
```

**Step 2) Flat rate taxes**

- Each flat rate tax is a percentage applied to the taxable base.
- Multiple flat rate taxes are allowed.

```
FlatRateTax = TaxableBase * Rate
```

**Step 3) Progressive tax**

- Only one progressive tax is allowed per country.
- It consists of intervals defined by an upper bound threshold and a rate. A null threshold = no limit.
- Applied to the taxable base incrementally.

```
For each interval [previousThreshold, threshold]:
    taxable slice = min(TaxableBase, threshold) - previousThreshold
    interval tax   = max(0, taxable slice) * rate
ProgressiveTax = sum(all interval taxes)
```

Assumption: The final interval must always have a null threshold.

**Step 4) Totals**
```
TotalTaxes = sum(Fixed) + sum(FlatRate) + ProgressiveTax
NetSalary  = GrossSalary - TotalTaxes
```

### Example

```
Gross = 62,000

Fixed taxes:
  CommunityTax = 1,500
  RadioTax     = 500
FixedTotal   = 1,500 + 500 = 2,000

TaxableBase = 62,000 - 2,000 = 60,000

FlatRate:
  PensionTax = 60,000 * 20% = 12,000

Progressive (intervals: 0-10k @ 0%, 10k-30k @ 20%, 30k+ @ 40%):
  10,000 * 0%  =      0
  20,000 * 20% =  4,000
  30,000 * 40% = 12,000
ProgressiveTotal = 0 + 4,000 + 12,000 = 16,000

TotalTaxes = 2,000 + 12,000 + 16,000 = 30,000
NetSalary  = 62,000 - 30,000 = 32,000
```
## Getting Started

### Prerequisites

- [Visual Studio 2022+](https://visualstudio.microsoft.com/downloads/) with .NET workload
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) 
- [Docker](https://www.docker.com/get-started) for running in a container
- [Dev Certificates](https://learn.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-10.0) set up for HTTPS support in Docker

### With Visual Studio

1) Open `TaxCalculator.slnx` in Visual Studio

2) Set `TaxCalculator.API` as the startup project

3) Select a launch profile from the toolbar:
   - http: runs on `http://localhost:5000`
   - https: runs on `https://localhost:5001`
   - Container (Dockerfile): runs in a Docker container on `http://localhost:8080` or `https://localhost:8081`

4) Press `F5` (or `Ctrl+F5` to run without the debugger)

5) Access the Scalar API documentation at the corresponding URL:
   - http: `http://localhost:5000/scalar/v1`
   - https: `https://localhost:5001/scalar/v1`
   - Container (Dockerfile): `http://localhost:8080/scalar/v1` or `https://localhost:8081/scalar/v1`

### With Docker

1) Navigate to the solution root directory (where the Dockerfile is located)

2) Build the Docker image:
```
docker build -t tax-calculator-api .
```

3) Run the container:

   - **HTTP only:**
     ```
     docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development tax-calculator-api
     ```

   - **HTTPS (with a dev certificate already set up):**
     ```powershell
     docker run -p 8080:8080 -p 8081:8081 -e ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_HTTPS_PORTS=8081 -e ASPNETCORE_Kestrel__Certificates__Default__Password=<your-password> -e ASPNETCORE_Kestrel__Certificates__Default__Path=<pfx-path> -v "$HOME/.aspnet/https:/https/" tax-calculator-api
     ```

4) Access the Scalar API documentation at [http://localhost:8080/scalar/v1](http://localhost:8080/scalar/v1) or [https://localhost:8081/scalar/v1](https://localhost:8081/scalar/v1)


### Without Docker (dotnet CLI)

1) Navigate to the solution root directory

2) Restore dependencies:
```
dotnet restore
```

3) Run the API project:
```
dotnet run --project src/TaxCalculator.API
```

4) Access the Scalar API documentation at [http://localhost:5000/scalar/v1](http://localhost:5000/scalar/v1)

## Future Extensions

### Tax Credits

The system will later subtract tax credits from Total Taxes after all tax items are calculated. Credits can come from an external source.

To implement:
- we can add an interface `ITaxCreditRepository` and inject it into `CalculateTaxHandler`
- after the tax is calculate, the handler can query the repository for any applicable credits and subtract them from the total before returning the result
- existing calculation logic in `TaxCalculationService` will remain unchanged

### External Rule Providers

If a manual rule for a country is not configured, the system may fall back to external providers.

To implement:
- we can implement the logic to call external providers in the implementation of `GetByCountryCodeAsync` for `ICountryTaxConfigurationRepository`
    - it can be an external HTTP request or a query to another database. This is purely an infrastructure concern
- the handler continues calling `GetByCountryCodeAsync` without knowing where the result comes from