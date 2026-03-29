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
    { "type": "fixed",       "name": "CommunityTax", "amount": 1500 },
    { "type": "flatRate",    "name": "PensionTax",   "rate": 0.20 },
    { "type": "progressive", "name": "IncomeTax",
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
GET /api/tax/calculate?countryCode=DE&grossSalary=62000
```

We use GET as the operation is idempotent and doesn't change anything in the server. No URL length concerns with the query parameters.

**Response `200 OK`:**
```json
{
  "grossSalary": 62000,
  "taxableBase": 60000,
  "totalTaxes": 30000,
  "netSalary": 32000,
  "breakdown": [
    { "name": "CommunityTax", "type": "fixed",       "amount": 1500  },
    { "name": "RadioTax",     "type": "fixed",       "amount": 500   },
    { "name": "PensionTax",   "type": "flatRate",    "amount": 12000 },
    { "name": "IncomeTax",    "type": "progressive", "amount": 16000 }
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
