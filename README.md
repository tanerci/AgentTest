# Product API

A .NET Web API with cookie-based authentication and product management endpoints, built following Clean Architecture principles.

## Architecture

This project follows Clean Architecture principles with the following layer organization:

```
├── Domain/                    # Core business logic
│   ├── Entities/             # Domain entities (ProductEntity, UserEntity)
│   ├── ValueObjects/         # Value objects (Money, ProductName, Stock)
│   ├── Repositories/         # Repository interfaces
│   └── Exceptions/           # Domain exceptions
├── Application/              # Application business rules
│   ├── Services/             # Application services (ProductService, AuthService)
│   └── DTOs/                 # Data Transfer Objects
├── Infrastructure/           # External concerns
│   ├── Persistence/          # Database context and persistence models
│   │   ├── Models/           # EF Core entity models
│   │   └── AppDbContext.cs   # Database context
│   └── Repositories/         # Repository implementations
├── Controllers/              # API Controllers (Presentation layer)
├── Common/                   # Cross-cutting concerns
└── Extensions/               # Extension methods
```

## Features

- Cookie-based authentication
- In-memory database
- Product CRUD operations
- Protected endpoints requiring authentication

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
List all products (public access)

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

## Running the Application

```bash
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`

## Testing with curl

### Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password123"}' \
  -c cookies.txt
```

### List Products
```bash
curl https://localhost:5001/api/products
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

### Logout
```bash
curl -X POST https://localhost:5001/api/auth/logout -b cookies.txt
```

## Notes

- Add and Update endpoints require authentication (will return 401 if not authenticated)
- List products endpoint is publicly accessible
- Authentication cookie expires after 24 hours
- In-memory database resets on application restart
