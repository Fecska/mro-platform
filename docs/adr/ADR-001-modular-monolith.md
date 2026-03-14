# ADR-001: Modular Monolith Architecture

**Date:** 2026-03-14
**Status:** Accepted

## Context

The platform needs to cover multiple MRO domains (aircraft, defects, work orders, personnel, inventory, release). We must decide between microservices, modular monolith, or traditional layered monolith.

## Decision

We will use a **modular monolith** for v1.

Each domain is a self-contained module with its own:
- domain models
- application services
- repository interfaces
- public API contracts

Modules communicate through internal contracts (interfaces), not HTTP calls.

## Rationale

- Faster initial development
- Easier debugging and tracing
- No distributed transaction complexity at this stage
- Can be extracted to services later if needed
- Sufficient for a single-organisation deployment

## Consequences

- All modules deploy together
- Team must respect module boundaries (no direct DB access across modules)
- Public contracts between modules must be versioned
- Future extraction to microservices is possible but not planned for v1
