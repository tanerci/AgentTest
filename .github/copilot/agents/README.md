# GitHub Copilot Agents

This directory contains configuration files for GitHub Copilot agents that automate various aspects of the development workflow.

## Available Agents

### 1. Documentation Agent (`documentation-agent.yml`)
**Purpose:** Keeps API documentation up to date by generating XML comments, Swagger/OpenAPI specs, and refreshing README.md when endpoints change.

**Triggers:**
- Push events on `main` and `develop` branches

**Tasks:**
- Generate documentation from source code
- Update README with latest API information
- Maintain OpenAPI specifications

---

### 2. Testing Agent (`testing-agent.yml`)
**Purpose:** Ensures new endpoints have unit and integration tests. Runs test suite on pull requests.

**Triggers:**
- Pull request opened or synchronized

**Tasks:**
- Scaffold missing tests using xUnit framework
- Run test suite with code coverage
- Generate coverage reports with 80% threshold

---

### 3. Consistency Agent (`consistency-agent.yml`)
**Purpose:** Enforces naming conventions, DRY principles, and consistent error handling.

**Triggers:**
- Pull request reviews

**Tasks:**
- Lint code against C# style rules
- Detect and refactor duplicate code
- Enforce naming conventions for controllers, DTOs, and methods
- Validate async/await patterns

---

### 4. CI/CD Agent (`cicd-agent.yml`)
**Purpose:** Generates and updates GitHub Actions workflows for build, test, and deploy pipelines.

**Triggers:**
- Push events on `main` branch

**Tasks:**
- Generate GitHub Actions workflows from templates
- Validate build configuration
- Optimize pipeline with caching and parallel execution
- Manage build artifacts

---

### 5. Security Agent (`security-agent.yml`)
**Purpose:** Scans for missing authentication/authorization and suggests middleware improvements.

**Triggers:**
- Push events on `main` and `develop` branches
- Pull request opened or synchronized

**Tasks:**
- Security vulnerability scanning
- SQL injection detection
- XSS vulnerability checks
- Authentication/authorization validation
- Cookie security verification
- Suggest security middleware improvements

---

### 6. Logging Agent (`logging-agent.yml`)
**Purpose:** Ensures consistent logging practices, proper log levels, structured logging, and prevents logging of sensitive data.

**Triggers:**
- Push events on `main` and `develop` branches
- Pull request opened or synchronized

**Tasks:**
- Validate structured logging usage
- Check appropriate log levels
- Detect sensitive data in logs
- Ensure proper exception logging
- Suggest ILogger dependency injection
- Generate logger implementation templates

---

### 7. Error Handling Agent (`error-handling-agent.yml`)
**Purpose:** Enforces consistent error handling patterns, exception management, proper HTTP status codes, and global exception handling.

**Triggers:**
- Push events on `main` and `develop` branches
- Pull request opened or synchronized

**Tasks:**
- Detect empty catch blocks
- Validate exception handling patterns
- Check proper HTTP status code usage
- Ensure ProblemDetails responses
- Generate global exception handler middleware
- Suggest custom exception classes

---

## Configuration Files

### Rules

#### `csharp-style.json`
Defines C# coding standards including:
- Naming conventions (PascalCase, camelCase, etc.)
- Code formatting rules
- Method length limits
- XML documentation requirements

#### `security.json`
Defines security rules including:
- Required authorization attributes
- SQL injection prevention
- XSS protection
- Secure cookie configuration
- Password hashing requirements
- Sensitive data logging prevention
- CORS policy restrictions
- Rate limiting recommendations

### Templates

#### `workflow.yml`
GitHub Actions workflow template for:
- .NET 9 build pipeline
- Unit testing with coverage
- Security scanning
- Deployment configuration

---

## Usage

These agents work automatically based on their configured triggers. They:
1. Monitor repository events (push, pull request, etc.)
2. Execute tasks defined in their configuration
3. Provide feedback through GitHub's interface
4. Generate or update code as needed

## Customization

You can customize agent behavior by modifying:
- **Trigger conditions:** Change branches or events
- **Task parameters:** Adjust input/output paths, thresholds, etc.
- **Rule severity:** Modify error vs. warning levels
- **Security checks:** Add or remove specific vulnerability checks

## Integration with GitHub Copilot

These agent configurations work with GitHub Copilot's agent framework to provide:
- Automated code generation
- Proactive suggestions
- Continuous quality improvements
- Security best practices enforcement
