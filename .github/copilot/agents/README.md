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

### 8. Performance Agent (`performance-agent.yml`)
**Purpose:** Detects performance bottlenecks, inefficient queries, blocking calls, and suggests optimizations.

**Triggers:**
- Pull request opened or synchronized
- Push events on `main` and `develop` branches

**Tasks:**
- Detect inefficient LINQ queries
- Identify N+1 query problems
- Find blocking async calls (.Result, .Wait())
- Check for AsNoTracking in read-only queries
- Suggest caching strategies
- Generate performance benchmarks
- Optimize database query patterns

---

### 9. Dependency Agent (`dependency-agent.yml`)
**Purpose:** Monitors NuGet packages for outdated or vulnerable versions and suggests upgrades.

**Triggers:**
- Weekly schedule (Monday at 9 AM)
- Pull request opened

**Tasks:**
- Scan for outdated packages
- Check security vulnerabilities
- Detect deprecated packages
- Validate license compliance
- Suggest version upgrades
- Generate automated update PRs
- Group related updates

---

### 10. Configuration Agent (`configuration-agent.yml`)
**Purpose:** Validates application configuration files for consistency, security, and best practices.

**Triggers:**
- Push events on `main` and `develop` branches
- Pull requests affecting configuration files

**Tasks:**
- Detect hardcoded secrets
- Validate required settings
- Check environment-specific configurations
- Ensure consistency across appsettings files
- Suggest User Secrets or Key Vault usage
- Generate configuration schema
- Validate JSON syntax

---

### 11. Accessibility Agent (`accessibility-agent.yml`)
**Purpose:** Ensures API endpoints and responses meet accessibility standards with clear messaging and localization.

**Triggers:**
- Pull request opened or synchronized

**Tasks:**
- Validate descriptive error messages
- Check localization support
- Ensure semantic naming conventions
- Verify inclusive language usage
- Validate response format consistency

---

## Reliability Pillar Agents (Well-Architected Framework)

### 12. Resiliency Agent (`resiliency-agent.yml`)
**Purpose:** Enforces runtime resiliency patterns using Polly - retries, circuit breakers, timeouts, bulkhead isolation, and fallback paths.

**Triggers:**
- Push events on `main` and `develop` branches
- Pull request opened or synchronized

**Tasks:**
- Validate Polly retry policies for HTTP clients
- Check circuit breaker configurations
- Ensure timeout policies on all external calls
- Validate idempotency for retryable operations
- Detect missing bulkhead isolation
- Check fallback handlers for critical paths
- Suggest policy composition improvements
- Validate telemetry integration

---

### 13. Message Broker Reliability Agent (`message-broker-reliability-agent.yml`)
**Purpose:** Enforces message broker reliability patterns for Azure Service Bus, RabbitMQ, Kafka - durable queues, DLQ, retry policies, partitioning.

**Triggers:**
- Push events on `main` and `develop` branches
- Pull request opened or synchronized

**Tasks:**
- Validate durable queue/topic configuration
- Ensure DLQ (dead letter queue) exists
- Check exponential backoff retry policies
- Validate poison message handling
- Ensure idempotent message processing
- Check partition key strategies
- Validate consumer concurrency limits
- Verify naming and versioning conventions
- Check processing timeouts

---

### 14. Health Probes Agent (`health-probes-agent.yml`)
**Purpose:** Validates liveness/readiness/startup health check endpoints, graceful shutdown, and deployment health probes.

**Triggers:**
- Push events on `main` and `develop` branches
- Pull request opened or synchronized

**Tasks:**
- Validate liveness endpoint existence
- Check readiness endpoint with dependency checks
- Verify startup probe for slow-starting apps
- Ensure lightweight health check implementation
- Validate dependency health checks (DB, cache, broker)
- Check graceful shutdown implementation
- Validate Kubernetes/App Service probe alignment
- Generate health check templates

---

### 15. Azure Backup & Recovery Agent (`azure-backup-recovery-agent.yml`)
**Purpose:** Validates Azure backup policies for Blob, Files, SQL, retention, geo-redundancy, and automated restore runbooks.

**Triggers:**
- Weekly schedule (Monday at 10 AM)
- Push events on `main` affecting infrastructure files

**Tasks:**
- Validate backup policies for Azure resources
- Check retention period compliance
- Verify geo-redundancy (RA-GRS) configuration
- Validate restore runbooks existence and currency
- Test recovery points and backup integrity
- Check quarterly drill schedule
- Validate encryption at rest and in transit
- Generate backup coverage and compliance reports

---

### 16. Local Backup & Recovery Agent (`local-backup-recovery-agent.yml`)
**Purpose:** Validates local/hybrid backup strategies - snapshots, encryption, air-gap/offsite, restore scripts, checksum verification.

**Triggers:**
- Weekly schedule (Monday at 11 AM)
- Push events on `main` affecting backup scripts

**Tasks:**
- Validate backup coverage completeness
- Check backup frequency aligns with RPO
- Verify encryption at rest (AES-256)
- Validate 3-2-1 backup strategy compliance
- Ensure offsite and air-gap copies exist
- Validate restore scripts and testing
- Check checksum integrity validation
- Verify retention policy enforcement
- Test restore process
- Generate backup status reports

---

### 17. DR Drill Agent (`dr-drill-agent.yml`)
**Purpose:** Ensures scheduled disaster recovery drills, captures RPO/RTO evidence, verifies automated restore paths, validates recovery procedures.

**Triggers:**
- Quarterly schedule (1st of quarter at 9 AM)
- Manual workflow dispatch for ad-hoc drills

**Tasks:**
- Validate drill schedule compliance (quarterly full, monthly partial)
- Verify RPO/RTO targets vs. actual measurements
- Test automated restore runbooks
- Validate drill documentation and plans
- Capture comprehensive evidence (logs, screenshots, timestamps)
- Generate post-drill reports within 48 hours
- Validate runbook currency after changes
- Test communication and notification systems
- Verify compliance requirements (SOC 2, ISO 27001, HIPAA)
- Track and validate action item completion

---

## Agent Summary

**Total Agents:** 17

**Categories:**
- **Code Quality:** Documentation, Testing, Consistency (3 agents)
- **Operations:** CI/CD, Logging, Error Handling (3 agents)
- **Performance & Security:** Performance, Security, Dependency (3 agents)
- **Configuration & Accessibility:** Configuration, Accessibility, Localization (3 agents)
- **Reliability (Well-Architected Framework):** Resiliency, Message Broker, Health Probes, Azure Backup, Local Backup, DR Drill (6 agents)

**Trigger Types:**
- **Push-based:** Continuous validation on code commits
- **Pull Request:** Pre-merge validation
- **Scheduled:** Weekly dependency scans, backup validations, quarterly DR drills
- **Manual:** On-demand drill execution

**Triggers:**
- Pull request opened or synchronized

**Tasks:**
- Validate descriptive error messages
- Check localization support (en, tr)
- Ensure inclusive language usage
- Validate semantic naming conventions
- Check API documentation completeness
- Ensure consistent response formats
- Verify helpful validation feedback

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
