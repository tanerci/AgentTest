# GitHub Copilot Agent Framework

This document outlines the systematic organization of coding agents based on the **Well-Architected Framework** pillars with a consistent tagging scheme for easy discovery and management.

---

## 🏗️ Framework Structure

All agents are organized under six core pillars plus operational categories:

1. **Security** — Authentication, authorization, vulnerability scanning, input validation
2. **Performance Efficiency** — Query optimization, caching, memory management, latency
3. **Reliability** — Resiliency patterns, error handling, health checks, logging
4. **Operational Excellence** — Documentation, testing, configuration, consistency, localization
5. **Backup & Recovery** — Cloud/local backups, disaster recovery drills
6. **Cost Optimization** — Resource usage, logging costs
7. **Sustainability** — Energy efficiency, cloud footprint
8. **Accessibility** — A11y compliance, inclusive design
9. **CI/CD** — Pipeline automation, workflow optimization

---

## 🏷️ Tagging Vocabulary

### Controlled Tags by Category

| Pillar | Tags |
|--------|------|
| **Security** | `security`, `auth`, `authorization`, `validation`, `dependencies`, `vulnerabilities`, `cve`, `injection`, `xss`, `csrf` |
| **Performance** | `performance`, `sql`, `api`, `memory`, `latency`, `optimization`, `caching`, `gc`, `disposal`, `n-plus-one` |
| **Reliability** | `reliability`, `error`, `resiliency`, `logging`, `probes`, `retry`, `circuit-breaker`, `timeout`, `fallback`, `bulkhead` |
| **Operational Excellence** | `ops-excellence`, `docs`, `readme`, `swagger`, `openapi`, `comments`, `testing`, `configuration`, `localization`, `i18n`, `consistency` |
| **Backup & Recovery** | `backup`, `recovery`, `drill`, `azure`, `local`, `rpo`, `rto`, `restore` |
| **Cost Optimization** | `cost`, `resources`, `logging-cost`, `over-provisioning` |
| **Sustainability** | `sustainability`, `energy`, `cloud-footprint`, `green-build` |
| **Accessibility** | `accessibility`, `a11y`, `inclusive`, `wcag` |
| **CI/CD** | `cicd`, `pipeline`, `automation`, `workflow` |

---

## 📊 Current Agent Inventory

### 🔐 Security (5 agents)
| Agent | File | Tags | Status |
|-------|------|------|--------|
| Security Agent | `security-agent.yml` | `security`, `vulnerabilities`, `xss`, `injection` | ✅ Active |
| Dependency Agent | `dependency-agent.yml` | `security`, `dependencies`, `vulnerabilities`, `cve` | ✅ Active |
| Configuration Agent | `configuration-agent.yml` | `security`, `configuration`, `secrets` | ✅ Active |
| Auth Agent | `auth-agent.yml` | `security`, `auth`, `authorization`, `jwt`, `rbac` | ✅ Active |
| Input Validation Agent | `input-validation-agent.yml` | `security`, `validation`, `injection`, `xss`, `sanitization` | ✅ Active |

### ⚡ Performance Efficiency (4 agents)
| Agent | File | Tags | Status |
|-------|------|------|--------|
| Performance Agent | `performance-agent.yml` | `performance`, `sql`, `latency`, `optimization`, `n-plus-one` | ✅ Active |
| SQL Optimization Agent | `sql-optimization-agent.yml` | `performance`, `sql`, `ef-core`, `optimization`, `n-plus-one` | ✅ Active |
| API Latency Agent | `api-latency-agent.yml` | `performance`, `api`, `latency`, `caching`, `compression` | ✅ Active |
| Memory Leak Agent | `memory-leak-agent.yml` | `performance`, `memory`, `gc`, `disposal`, `idisposable` | ✅ Active |

### 🛡️ Reliability (6 agents)
| Agent | File | Tags | Status |
|-------|------|------|--------|
| Resiliency Agent | `resiliency-agent.yml` | `reliability`, `resiliency`, `retry`, `circuit-breaker`, `timeout` | ✅ Active |
| Health Probes Agent | `health-probes-agent.yml` | `reliability`, `probes`, `liveness`, `readiness` | ✅ Active |
| Message Broker Agent | `message-broker-reliability-agent.yml` | `reliability`, `messaging`, `dlq`, `idempotency` | ✅ Active |
| Error Handling Agent | `error-handling-agent.yml` | `reliability`, `error`, `exceptions`, `result-pattern` | ✅ Active |
| Logging Agent | `logging-agent.yml` | `reliability`, `logging`, `telemetry`, `observability` | ✅ Active |
| Distributed Architecture Agent | `distributed-architecture-agent.yml` | `reliability`, `distributed-systems`, `decoupling`, `fault-tolerance` | ✅ Active |

### 📖 Operational Excellence (9 agents)
| Agent | File | Tags | Status |
|-------|------|------|--------|
| Documentation Agent | `documentation-agent.yml` | `ops-excellence`, `docs`, `swagger`, `comments` | ✅ Active |
| Consistency Agent | `consistency-agent.yml` | `ops-excellence`, `consistency`, `naming`, `dry` | ✅ Active |
| Localization Agent | `localization-agent.yml` | `ops-excellence`, `localization`, `i18n`, `globalization` | ✅ Active |
| Testing Agent | `testing-agent.yml` | `ops-excellence`, `testing`, `unit-tests`, `coverage` | ✅ Active |
| Configuration Agent | `configuration-agent.yml` | `ops-excellence`, `configuration`, `settings` | ✅ Active |
| Clean Architecture Agent | `clean-architecture-agent.yml` | `ops-excellence`, `architecture`, `clean-architecture`, `layering` | ✅ Active |
| Domain Driven Design Agent | `domain-driven-design-agent.yml` | `ops-excellence`, `architecture`, `ddd`, `aggregates`, `bounded-context` | ✅ Active |
| Microservices Agent | `microservices-agent.yml` | `ops-excellence`, `architecture`, `microservices`, `service-boundaries` | ✅ Active |
| Agent Evolution Agent | `agent-evolution-agent.yml` | `ops-excellence`, `meta-agent`, `continuous-improvement`, `pattern-analysis` | ✅ Active |

### 💾 Backup & Recovery (3 agents)
| Agent | File | Tags | Status |
|-------|------|------|--------|
| Azure Backup Agent | `azure-backup-recovery-agent.yml` | `backup`, `recovery`, `azure`, `rpo`, `rto` | ✅ Active |
| Local Backup Agent | `local-backup-recovery-agent.yml` | `backup`, `recovery`, `local`, `encryption` | ✅ Active |
| DR Drill Agent | `dr-drill-agent.yml` | `backup`, `recovery`, `drill`, `rpo`, `rto` | ✅ Active |

### 🌱 Accessibility (1 agent)
| Agent | File | Tags | Status |
|-------|------|------|--------|
| Accessibility Agent | `accessibility-agent.yml` | `accessibility`, `a11y`, `inclusive`, `localization` | ✅ Active |

### 🚀 CI/CD (1 agent)
| Agent | File | Tags | Status |
|-------|------|------|--------|
| CI/CD Agent | `cicd-agent.yml` | `cicd`, `pipeline`, `automation`, `workflow` | ✅ Active |

**Total Active Agents:** 29
| Auth Agent | `auth-agent.yml` | `security`, `authentication`, `authorization` | ✅ Active |
| Input Validation Agent | `input-validation-agent.yml` | `security`, `validation`, `injection-prevention` | ✅ Active |
| SQL Optimization Agent | `sql-optimization-agent.yml` | `performance`, `query`, `optimization`, `indexing` | ✅ Active |
| API Latency Agent | `api-latency-agent.yml` | `performance`, `latency`, `caching`, `endpoint` | ✅ Active |
| Memory Leak Agent | `memory-leak-agent.yml` | `performance`, `memory`, `GC`, `leak` | ✅ Active |
| Clean Architecture Agent | `clean-architecture-agent.yml` | `architecture`, `clean`, `structure` | ✅ Active |
| Domain Driven Design Agent | `domain-driven-design-agent.yml` | `architecture`, `domain`, `ddd` | ✅ Active |
| Microservices Agent | `microservices-agent.yml` | `architecture`, `microservices`, `distributed` | ✅ Active |
| Distributed Architecture Agent | `distributed-architecture-agent.yml` | `architecture`, `distributed`, `scalability` | ✅ Active |
| Agent Evolution Agent | `agent-evolution-agent.yml` | `meta`, `evolution`, `self-improvement` | ✅ Active |

**Total Active Agents:** 28

---

## 🆕 Planned Specialized Agents

### 🔐 Security (1 new)
- [x] `auth-agent.yml` — Authentication/authorization enforcement
- [x] `input-validation-agent.yml` — Injection prevention (SQL, XSS, command)
- [ ] `secrets-scanning-agent.yml` — API keys, passwords, tokens detection

### ⚡ Performance Efficiency (0 new)
- [x] `sql-optimization-agent.yml` — Query tuning, indexing, N+1 prevention
- [x] `api-latency-agent.yml` — Endpoint performance, response caching
- [x] `memory-leak-agent.yml` — IDisposable, GC pressure, memory leaks
- [ ] `secrets-scanning-agent.yml` — API keys, passwords, tokens detection

### ⚡ Performance Efficiency (0 new)

### 🛡️ Reliability (2 new)
- [x] `distributed-architecture-agent.yml` — Distributed system design principles
- [ ] `distributed-tracing-agent.yml` — Correlation IDs, OpenTelemetry
- [ ] `rate-limiting-agent.yml` — Rate limiting, throttling patterns

### 📖 Operational Excellence (3 new)
- [x] `clean-architecture-agent.yml` — Clean Architecture principles enforcement
- [x] `domain-driven-design-agent.yml` — DDD patterns and bounded contexts
- [x] `microservices-agent.yml` — Microservices architecture patterns
- [x] `agent-evolution-agent.yml` — Agent improvement and pattern analysis
- [ ] `readme-agent.yml` — README.md generation/updates
- [ ] `api-documentation-agent.yml` — OpenAPI/Swagger completeness
- [ ] `code-comments-agent.yml` — XML documentation enforcement

### 💰 Cost Optimization (2 new)
- [ ] `resource-usage-agent.yml` — Over-provisioned resources detection
- [ ] `logging-cost-agent.yml` — Excessive logging/storage usage

### 🌱 Sustainability (2 new)
- [ ] `green-build-agent.yml` — Energy-efficient build configurations
- [ ] `cloud-footprint-agent.yml` — Unnecessary cloud resource detection

---

## 📋 Agent Metadata Template

Every agent file should include the following metadata:

```yaml
name: agent-name
description: >
  Brief description of what the agent does
pillar: [security|performance|reliability|ops-excellence|backup|cost|sustainability|accessibility|cicd]
tags: [tag1, tag2, tag3]
version: "1.0"
triggers:
  - event: push
    branches: ["master", "main", "develop"]
  - event: pull_request
    actions: ["opened", "synchronize"]
tasks:
  - type: task-type
    inputs:
      source: "**/*.cs"
      ruleset: ".github/copilot/rules/ruleset-name.json"
      checks: [...]
```

---

## 🔍 Discovery Commands

### Find agents by pillar
```bash
# Security agents
grep -l "pillar: security" .github/copilot/agents/*.yml

# Performance agents
grep -l "pillar: performance" .github/copilot/agents/*.yml
```

### Find agents by tag
```bash
# All agents handling authentication
grep -l "tags:.*auth" .github/copilot/agents/*.yml

# All agents dealing with logging
grep -l "tags:.*logging" .github/copilot/agents/*.yml
```

---

## 🚦 Agent Status Definitions

- **✅ Active** — Fully implemented with ruleset
- **🚧 In Progress** — Agent file exists, ruleset incomplete
- **📋 Planned** — Documented but not yet implemented
- **🔄 Deprecated** — Replaced by newer agent

---

## 📈 Roadmap

### Phase 1: Foundation (Complete)
- ✅ Core reliability agents
- ✅ Security basics
- ✅ Operational excellence
- ✅ Backup & recovery

### Phase 2: Performance & Cost (In Progress)
- [x] SQL optimization agent
- [x] API latency agent
- [x] Memory leak agent
- [ ] Resource usage agent
- [ ] Logging cost agent

### Phase 3: Advanced Security (In Progress)
- [x] Auth agent
- [x] Input validation agent
- [ ] Secrets scanning agent

### Phase 4: Architecture (Complete)
- [x] Clean Architecture agent
- [x] Domain Driven Design agent
- [x] Microservices agent
- [x] Distributed Architecture agent
- [x] Agent Evolution agent

### Phase 5: Sustainability (Future)
- [ ] Green build agent
- [ ] Cloud footprint agent

---

## 🤝 Contributing New Agents

1. Choose the appropriate pillar
2. Select tags from the controlled vocabulary
3. Create agent YAML with metadata
4. Create corresponding ruleset JSON
5. Update this framework document
6. Add examples to templates directory
7. Test with real codebase

---

## 📚 References

- [Azure Well-Architected Framework](https://learn.microsoft.com/en-us/azure/well-architected/)
- [GitHub Copilot Agents Documentation](https://docs.github.com/en/copilot)
- [Polly Resilience Patterns](https://github.com/App-vNext/Polly)
