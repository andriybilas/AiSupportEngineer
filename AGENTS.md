# AGENTS.md — AiSupportEngineer

## Purpose

AiSupportEngineer is a multi-tenant AI support engineer platform built with .NET.
Users belong to tenants; tenants own services. The codebase follows Onion / Clean Architecture.

## Architecture

Dependencies point **inward only**:

```
Api → Application → Domain
Api → Infrastructure → Application → Domain
```

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Entities and domain logic. No project references. Persistence-ignorant. |
| **Application** | Use cases, DTOs/models, ports (abstractions). References **Domain only**. |
| **Infrastructure** | EF Core, Identity, repositories (adapters). Implements Application ports. References Application + Domain. |
| **Api** | Composition root: HTTP, DI wiring, OpenAPI. References Application + Infrastructure. |
| **Tests** | Unit/integration tests. May reference Application and/or Domain (and Infrastructure when needed). |

### Conventions

- **Ports** live in Application (`Abstractions/` or `Interfaces/`).
- **Adapters** (repository implementations, EF entities, Identity) live in Infrastructure.
- **Api** registers Infrastructure via `AddInfrastructure(...)` and application services.
- Identity + EF Core stay in Infrastructure; Domain never references them.
- Map EF entities ↔ Domain models inside Infrastructure.

## Solution map

- `AiSupport.Api` — HTTP host / composition root
- `AiSupport.Application` — services, app models, repository interfaces
- `AiSupport.Domain` — domain models
- `AiSupport.Infrastructure` — DbContext, EF entities, migrations, repository implementations, DI extension
- `AiSupport.Tests` — tests

Solution file: `AiSupportEngineer/AiSupportEngineer.slnx`

## Coding style

- Always write `if` as multiline blocks with braces. Never a same-line `if` body.
- Separate each `if` block from surrounding statements with a blank line. Keep `else` attached to the `if`.
- Split complex conditions, method calls, and LINQ across multiple lines.
- Separate every multiline expression/statement from surrounding statements with a blank line (not against enclosing braces).
- Do not pass a ternary directly as a method argument — assign to a named local first.
- Extract dense predicates into named methods when it helps readability.

## Don'ts

- Do **not** reference Infrastructure from Application.
- Do **not** add any project references from Domain.
- Do **not** commit secrets; leave connection strings in local appsettings only.
- Do **not** edit `bin/` or `obj/`.
- Do **not** commit or push without explicit user approval.
- Do **not** clone over the local project (GitHub remote may only have README).

## Language

- Chat with the user may be in Ukrainian.
- Code, comments, and this file stay in **English**.
