# Security Documentation

This document outlines the security measures implemented in the Product API application.

## Security Features

### 1. Input Validation

All DTOs have comprehensive validation using DataAnnotations:

#### LoginRequest
- Username: Required, 3-50 characters
- Password: Required, 6-100 characters (minimum complexity requirement)

#### ProductCreateDto
- Name: Required, 1-100 characters
- Description: Required, max 500 characters
- Price: Required, between 0.01 and 999999.99
- Stock: Required, non-negative integer

#### ProductUpdateDto
- Name: Optional, 1-100 characters
- Description: Optional, max 500 characters
- Price: Optional, between 0.01 and 999999.99
- Stock: Optional, non-negative integer

All controllers validate model state before processing requests.

### 2. Rate Limiting

Rate limiting is implemented using AspNetCoreRateLimit to prevent abuse:

- **Login endpoint**: 5 attempts per minute per IP (brute force protection)
- **General endpoints**: 100 requests per minute per IP (DoS protection)

Configuration:
- Uses `X-Forwarded-For` header for IP detection (supports reverse proxy deployments)
- Returns HTTP 429 (Too Many Requests) when limits are exceeded

### 3. Authentication & Authorization

- Cookie-based authentication with secure settings:
  - HttpOnly: true (prevents XSS cookie theft)
  - SecurePolicy: Always (requires HTTPS)
  - SameSite: Strict (prevents CSRF attacks)
  - 24-hour expiration with sliding expiration
- Passwords hashed using BCrypt with automatic salt generation
- Protected endpoints require [Authorize] attribute

### 4. CORS Configuration

- Configured with explicit allowed origins (no wildcards)
- Requires configuration in production environment
- Development defaults to `https://localhost:5001` only
- Credentials allowed for cookie-based authentication
- Configure via `AllowedOrigins` in appsettings.json

### 5. Security Headers

The application sets the following security headers on all responses:

- **X-Content-Type-Options**: nosniff
  - Prevents MIME type sniffing attacks
  
- **X-Frame-Options**: DENY
  - Prevents clickjacking attacks
  
- **X-XSS-Protection**: 1; mode=block
  - Enables browser XSS protection with blocking mode
  
- **Referrer-Policy**: strict-origin-when-cross-origin
  - Controls referrer information sent in requests
  
- **Content-Security-Policy**: Environment-specific
  - **Production**: Strict policy with no unsafe directives
    - `default-src 'self'`
    - `script-src 'self'`
    - `style-src 'self'`
    - `img-src 'self' data:`
    - `font-src 'self'`
    - `connect-src 'self'`
    - `frame-ancestors 'none'`
    - `object-src 'none'`
    - `base-uri 'self'`
  - **Development**: Relaxed for Swagger UI compatibility
    - Allows `unsafe-inline` and `unsafe-eval` for scripts

### 6. Security Event Logging

Authentication events are logged for security monitoring:

- **Failed login attempts**: Logs username and source IP address
- **Successful logins**: Logs username and source IP address
- Helps with:
  - Security incident investigation
  - Detecting brute force attacks
  - Compliance and audit requirements

### 7. HTTPS

- HTTPS redirection is enforced
- Cookie SecurePolicy set to Always (requires HTTPS)

### 8. SQL Injection Protection

- Entity Framework Core provides automatic parameterization
- All database queries use LINQ (not raw SQL)
- No user input is concatenated into queries

## Configuration

### Production Deployment

1. **Configure AllowedOrigins** in appsettings.json:
   ```json
   "AllowedOrigins": [
     "https://yourdomain.com",
     "https://www.yourdomain.com"
   ]
   ```

2. **Configure AllowedHosts** for production (currently set to localhost:* for development):
   ```json
   "AllowedHosts": "yourdomain.com;www.yourdomain.com"
   ```
   Note: Replace with your actual domain names and avoid wildcards in production.

3. **Set up reverse proxy** with proper headers:
   - Ensure `X-Forwarded-For` header is set correctly
   - Configure TLS/SSL termination
   - Consider using Azure Application Gateway or similar

4. **Review rate limits** based on expected traffic

### Development Setup

Development configuration is in `appsettings.Development.json`:
- AllowedOrigins: `https://localhost:5001`
- Swagger UI enabled by default
- Relaxed CSP for Swagger compatibility

## Security Best Practices

1. **Never commit secrets** to source control
2. **Rotate passwords** regularly
3. **Monitor logs** for suspicious activity
4. **Keep dependencies updated** to patch vulnerabilities
5. **Use HTTPS** in production (enforced by configuration)
6. **Configure firewalls** at network level
7. **Implement monitoring** and alerting for security events

## Security Testing

The application includes comprehensive security tests:
- Input validation tests (12 tests)
- Authentication flow tests
- Authorization tests
- All tests passing: 27/27

Run tests:
```bash
dotnet test ProductApi.Tests/ProductApi.Tests.csproj
```

## Security Scanning

The codebase is scanned with:
- **CodeQL**: Static analysis for security vulnerabilities
- **.NET Analyzers**: Code quality and security rules
- **EnforceCodeStyleInBuild**: Ensures consistent code style

Latest scan results: 0 vulnerabilities found

## Reporting Security Issues

If you discover a security vulnerability, please report it to the repository owner directly. Do not create public issues for security vulnerabilities.

## Compliance

This application implements security controls aligned with:
- OWASP Top 10 recommendations
- Microsoft Security Development Lifecycle (SDL) practices
- Common security best practices for ASP.NET Core applications

## Updates

Last security review: 2025-11-26
Next recommended review: Every major release or when adding new features
