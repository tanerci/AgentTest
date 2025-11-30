# Consistency Agent

## Overview

The Consistency Agent enforces naming conventions, DRY principles, and consistent error handling across the codebase to maintain code quality and readability. It ensures architectural consistency following Clean Architecture and DDD patterns.

**Pillar:** Operational Excellence  
**Version:** 1.1  
**Tags:** consistency, naming, dry, refactoring, code-quality, clean-architecture, ddd

---

## Tasks

### 1. Lint

Validates code against defined style rules.

| Property | Value |
|----------|-------|
| Type | lint |
| Ruleset | `.github/copilot/rules/csharp-style.json` |
| Source | `**/*.cs` |

### 2. Refactor

Identifies and suggests refactoring for common code issues.

| Pattern | Description |
|---------|-------------|
| duplicate-methods | Detect duplicated code that violates DRY |
| inconsistent-naming | Flag naming that doesn't follow conventions |
| missing-error-handling | Identify missing error handling |
| magic-numbers | Detect hardcoded values that should be constants |
| long-methods | Flag methods exceeding 50 lines |
| missing-xml-documentation | Identify public members without XML docs |
| inconsistent-localization | Find hardcoded strings that should be localized |

### 3. Validate

Validates ASP.NET Core framework conventions.

| Convention | Description |
|------------|-------------|
| controller-naming | Controllers must end with `Controller` |
| dto-naming | DTOs must end with `Dto`, `Request`, or `Response` |
| async-suffix | Async methods must end with `Async` (except controller actions) |
| result-pattern | Use Result<T> pattern for error handling |
| value-objects | Use value objects for domain concepts |
| repository-pattern | Follow repository pattern for data access |

---

## Guidelines

### Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `ProductService` |
| Private Fields | _camelCase | `_productRepository` |
| Interfaces | IPascalCase | `IProductService` |
| Async Methods | *Async suffix | `GetProductsAsync()` |
| DTOs | *Dto/*Request/*Response | `ProductDto`, `LoginRequest` |
| Controllers | *Controller | `ProductsController` |
| Entities | *Entity (domain) | `ProductEntity` |

### Error Handling

| Pattern | Usage |
|---------|-------|
| Result<T> | Expected failures (validation, not found) |
| Domain Exceptions | Invariant violations |
| GlobalExceptionHandler | Unexpected errors |
| ErrorMessages class | Centralized error message strings |

### Architecture

This project follows **Clean Architecture** with the following layer separation:

```
┌─────────────────────────────────────────────────────────────┐
│                       Presentation                          │
│              (Controllers, API Endpoints)                   │
├─────────────────────────────────────────────────────────────┤
│                       Application                           │
│        (Services, DTOs, Use Cases, Result Pattern)         │
├─────────────────────────────────────────────────────────────┤
│                         Domain                              │
│   (Entities, Value Objects, Repositories Interfaces)       │
├─────────────────────────────────────────────────────────────┤
│                     Infrastructure                          │
│        (Repository Implementations, Persistence)           │
└─────────────────────────────────────────────────────────────┘
```

**Key Principles:**
- Domain layer has no external dependencies
- Application layer orchestrates use cases
- Infrastructure layer implements repository interfaces
- Use value objects for domain concepts (Money, ProductName, Stock)

### Localization

| Rule | Description |
|------|-------------|
| Resource Files | All user-facing strings in `.resx` files |
| IStringLocalizer | Use for message formatting |
| Culture Consistency | Maintain all translations across culture files |

### Documentation

| Requirement | Description |
|-------------|-------------|
| XML Documentation | Required on all public types and members |
| API Examples | Include sample requests/responses |
| Response Types | Document with `[ProducesResponseType]` attributes |

---

## Style Rules Reference

The consistency agent uses the following C# style rules (from `csharp-style.json`):

| Rule ID | Name | Severity | Description |
|---------|------|----------|-------------|
| CS001 | PascalCaseClassNames | Error | Class names must use PascalCase |
| CS002 | CamelCasePrivateFields | Warning | Private fields use _camelCase prefix |
| CS003 | AsyncMethodSuffix | Error | Async methods end with 'Async' (except controllers) |
| CS004 | ControllerSuffix | Error | Controller classes end with 'Controller' |
| CS005 | DtoSuffix | Warning | DTOs end with 'Dto', 'Request', or 'Response' |
| CS006 | NoMagicNumbers | Warning | Avoid hardcoded values, use constants |
| CS007 | MaxMethodLength | Warning | Methods should not exceed 50 lines |
| CS008 | RequireXmlComments | Info | Public types/members require XML documentation |
| CS009 | InterfacePrefix | Error | Interfaces must start with 'I' |
| CS010 | EntitySuffix | Info | Domain entities should end with 'Entity' |
| CS011 | RepositorySuffix | Warning | Repository classes end with 'Repository' |
| CS012 | ServiceSuffix | Warning | Service classes end with 'Service' |

### Architecture Rules

The `csharp-style.json` also includes architecture validation rules:

```json
{
  "architectureRules": {
    "cleanArchitecture": {
      "layers": ["Domain", "Application", "Infrastructure", "Presentation"],
      "dependencyDirection": "inward",
      "domainNoDependencies": true
    },
    "ddd": {
      "valueObjects": true,
      "aggregateRoots": true,
      "repositoryPattern": true
    }
  }
}
```

---

## Integration with EditorConfig

This agent works alongside `.editorconfig` settings for:
- Indentation (4 spaces)
- Brace style (new line)
- Naming conventions
- Code analysis rules

---

## Related Agents

- [clean-architecture-agent](clean-architecture-agent.yml) - Validates Clean Architecture patterns
- [domain-driven-design-agent](domain-driven-design-agent.yml) - Enforces DDD principles
- [error-handling-agent](error-handling-agent.yml) - Validates error handling patterns
- [documentation-agent](documentation-agent.yml) - Ensures documentation coverage
