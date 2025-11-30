# Product API

A .NET Web API with cookie-based authentication, product management, and reservation endpoints, built following Clean Architecture and Domain-Driven Design (DDD) principles.

## Architecture

This project follows Clean Architecture and DDD principles with the following layer organization:

```
├── Domain/                    # Core business logic
│   ├── Entities/             # Domain entities (ProductEntity, UserEntity, ReservationEntity)
│   ├── ValueObjects/         # Value objects (Money, ProductName, Stock, ReservationId)
│   ├── Repositories/         # Repository interfaces
│   ├── Services/             # Domain services (ReservationDomainService)
│   └── Exceptions/           # Domain exceptions
├── Application/              # Application business rules
│   ├── Services/             # Application services (ProductService, AuthService, ReservationService)
│   ├── DTOs/                 # Data Transfer Objects
│   └── Common/               # Shared utilities (Result pattern, Error handling)
├── Infrastructure/           # External concerns
│   ├── Persistence/          # Database context and persistence models
│   │   ├── Models/           # EF Core entity models
│   │   └── AppDbContext.cs   # Database context
│   └── Repositories/         # Repository implementations (including Redis cache)
├── Controllers/              # API Controllers (Presentation layer)
├── Common/                   # Cross-cutting concerns (GlobalExceptionHandler)
├── Extensions/               # Extension methods
└── Resources/                # Localization resources
```

## Features

- Cookie-based authentication with secure settings
- In-memory database (with optional Redis cache for reservations)
- Product CRUD operations with pagination and filtering
- Product reservation system with TTL-based expiration
- Protected endpoints requiring authentication
- Rate limiting to prevent abuse
- Localization support (en-US, es-ES, fr-FR, de-DE)
- Comprehensive security headers
- Response caching for improved performance

## Database Seeds

### Default User
- **Username:** `admin`
- **Password:** `password123`

### Sample Products
1. Laptop - $999.99
2. Mouse - $29.99
3. Keyboard - $89.99

## API Endpoints

### Authentication

#### POST /api/auth/login
Login with credentials (creates authentication cookie)
```json
{
  "username": "admin",
  "password": "password123"
}
```

#### POST /api/auth/logout
Logout and clear authentication cookie

#### GET /api/auth/status
Check authentication status

### Products

#### GET /api/products
List all products with pagination (public access)

**Query Parameters:**
- `page` (optional, default: 1): Page number (1-based)
- `pageSize` (optional, default: 10, max: 100): Items per page

**Response:**
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

#### GET /api/products/filter
Search and filter products with pagination (public access)

**Query Parameters:**
- `searchTerm` (optional): Search by product name or description
- `minPrice` (optional): Minimum price filter
- `maxPrice` (optional): Maximum price filter
- `minStock` (optional): Minimum stock quantity filter
- `maxStock` (optional): Maximum stock quantity filter
- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 10, max: 100): Items per page

#### GET /api/products/{id}
Get a specific product by ID (public access)

#### POST /api/products (Protected)
Create a new product (requires authentication)
```json
{
  "name": "New Product",
  "description": "Product description",
  "price": 99.99,
  "stock": 20
}
```

#### PUT /api/products/{id} (Protected)
Update an existing product (requires authentication)
```json
{
  "name": "Updated Name",
  "price": 149.99,
  "stock": 15
}
```

#### DELETE /api/products/{id} (Protected)
Delete a product (requires authentication)

### Reservations

The reservation system allows users to temporarily hold product stock before completing a purchase. Reservations automatically expire after a configurable TTL (Time-To-Live).

#### POST /api/reservations (Protected)
Create a new product reservation
```json
{
  "productId": 1,
  "quantity": 2,
  "ttlMinutes": 15
}
```
**Response:**
```json
{
  "reservationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "productId": 1,
  "quantity": 2,
  "status": "Active",
  "createdAt": "2025-11-30T15:00:00Z",
  "expiresAt": "2025-11-30T15:15:00Z",
  "completedAt": null
}
```

#### GET /api/reservations/{id} (Protected)
Get a specific reservation by ID

#### POST /api/reservations/checkout (Protected)
Complete a reservation and finalize the order
```json
{
  "reservationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

#### DELETE /api/reservations/{id} (Protected)
Cancel an active reservation and return stock to inventory

## Testing with curl

### Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password123"}' \
  -c cookies.txt
```

### List Products (with pagination)
```bash
# Get first page with 10 items
curl https://localhost:5001/api/products

# Get second page with 5 items
curl "https://localhost:5001/api/products?page=2&pageSize=5"
```

### Filter Products
```bash
# Search by name or description
curl "https://localhost:5001/api/products/filter?searchTerm=mouse"

# Filter by price range
curl "https://localhost:5001/api/products/filter?minPrice=50&maxPrice=200"

# Filter by stock availability
curl "https://localhost:5001/api/products/filter?minStock=10"

# Combine multiple filters
curl "https://localhost:5001/api/products/filter?searchTerm=laptop&minPrice=500&page=1&pageSize=20"
```

### Create Product (with authentication)
```bash
curl -X POST https://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"name":"Headphones","description":"Wireless headphones","price":149.99,"stock":30}'
```

### Update Product (with authentication)
```bash
curl -X PUT https://localhost:5001/api/products/1 \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"price":899.99}'
```

### Reserve Product (with authentication)
```bash
# Create a reservation for 2 units with 15 minute TTL
curl -X POST https://localhost:5001/api/reservations \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"productId":1,"quantity":2,"ttlMinutes":15}'
```

### Get Reservation Status
```bash
curl https://localhost:5001/api/reservations/{reservationId} \
  -b cookies.txt
```

### Checkout Reservation
```bash
curl -X POST https://localhost:5001/api/reservations/checkout \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"reservationId":"your-reservation-id"}'
```

### Cancel Reservation
```bash
curl -X DELETE https://localhost:5001/api/reservations/{reservationId} \
  -b cookies.txt
```

### Logout
```bash
curl -X POST https://localhost:5001/api/auth/logout -b cookies.txt
```

## Notes

- Add and Update endpoints require authentication (will return 401 if not authenticated)
- List products endpoint is publicly accessible
- Authentication cookie expires after 24 hours
- In-memory database resets on application restart
- Reservations use an in-memory Redis-like cache for fast stock management
- Reservation TTL defaults to 15 minutes if not specified

## Localization

The API supports multiple languages for error messages and responses:
- English (en-US) - Default
- Spanish (es-ES)
- French (fr-FR)
- German (de-DE)

Set the `Accept-Language` header to request localized responses:
```bash
curl -H "Accept-Language: es-ES" https://localhost:5001/api/products
```

## Configuration

### Environment Variables

The application uses `appsettings.json` and `appsettings.Development.json` for configuration:

| Setting | Description | Default |
|---------|-------------|---------|
| `AllowedHosts` | Allowed hostnames | `localhost:*` |
| `AllowedOrigins` | CORS allowed origins | `[]` (must be configured in production) |

### Rate Limiting

- **Login endpoint**: 5 attempts per minute per IP
- **General endpoints**: 100 requests per minute per IP

## Response Caching

The following endpoints use response caching for improved performance:

| Endpoint | Cache Duration | Varies By |
|----------|---------------|-----------|
| `GET /api/products` | 60 seconds | `page`, `pageSize` |
| `GET /api/products/filter` | 30 seconds | All query parameters |
| `GET /api/products/{id}` | 60 seconds | - |

## Development

### Prerequisites
- .NET 9.0 SDK or later

### Build
```bash
dotnet build AgentTest.sln
```

### Run Tests
```bash
dotnet test AgentTest.sln
```

### Run the Application
```bash
dotnet run --project ProductApi.csproj
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`

### Swagger UI

In development mode, Swagger UI is available at the root URL (`/`) for API exploration and testing.
