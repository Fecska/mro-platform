# ADR-002: Technology Stack

**Date:** 2026-03-14
**Status:** Accepted

## Decision

| Layer | Technology |
|-------|-----------|
| Backend API | C# / .NET 9 |
| Web frontend | React 19 + TypeScript |
| Mobile | React Native (Expo) |
| Database | PostgreSQL 16 |
| Cache | Redis 7 |
| Message queue | RabbitMQ 3.13 |
| File storage | S3-compatible (MinIO for dev, cloud S3 for prod) |
| Auth | Keycloak (OIDC) or custom JWT service |
| Search | PostgreSQL full-text (OpenSearch later if needed) |
| Infra (dev) | Docker Compose |
| Infra (prod) | TBD — VM + managed DB or Kubernetes |

## Rationale

- C# / .NET: strong typing, mature enterprise ecosystem, excellent VS Code support
- PostgreSQL: reliable, ACID, good JSON support for flexible metadata
- React: large ecosystem, shared code possible between web and mobile
- RabbitMQ: reliable async processing for notifications, background jobs
- Modular approach avoids premature infrastructure complexity
