# ADR-002: .NET + React + PostgreSQL Stack

**Date:** 2026-03-14
**Status:** Accepted
**Supersedes:** ADR-002-tech-stack.md (same decision, canonical name)

## Context

We need a backend, frontend, and database technology for the MRO Platform that is:
- Strongly typed (safety-critical domain)
- Well-supported in VS Code
- Mature and enterprise-proven
- Suitable for a modular monolith

## Decision

| Layer | Technology | Version |
|-------|-----------|---------|
| Backend API | C# / .NET | 9 |
| Web frontend | React + TypeScript | React 19 |
| Mobile | React Native (Expo) | Expo 54+ |
| Database | PostgreSQL | 16 |
| Cache | Redis | 7 |
| Message queue | RabbitMQ | 3.13 |
| File storage | MinIO (dev) / S3 (prod) | — |
| Auth | Keycloak | 25+ |
| Containerisation | Docker Compose (dev) | — |

## Rationale

**C# / .NET:**
Strong typing is essential for a safety-critical domain with complex state machines and compliance rules.
.NET 9 has excellent performance, mature ORM (EF Core), built-in DI, and full VS Code support.

**React + TypeScript:**
Large ecosystem, strong typing, component reuse between web and mobile possible.
TypeScript prevents a class of runtime errors that matter in a compliance context.

**PostgreSQL:**
ACID compliance, excellent JSON support for flexible metadata (occurrence reports, custom fields), row-level security, mature full-text search.
Avoids the complexity of multiple databases in v1.

**React Native:**
Allows sharing business logic and types between web and mobile.
Expo simplifies build and distribution.

## Consequences

- All backend business logic in C#, no mixed-language backend
- Database access only via EF Core repositories (no raw SQL in application code, except migrations)
- Frontend type definitions shared with mobile where possible
- PostgreSQL used as the single data store for v1 (no separate NoSQL, no separate search DB)
