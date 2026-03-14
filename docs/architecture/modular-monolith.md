# Modular Monolith Architecture

**Version:** 1.0
**Date:** 2026-03-14

---

## Principle

The MRO Platform is a modular monolith. All modules are deployed as a single process, but each module is a self-contained unit with clear boundaries.

Modules must not:
- Access another module's database tables directly
- Import another module's internal domain classes
- Bypass another module's business logic

Modules communicate only through:
- Defined public contracts (interfaces / DTOs)
- Internal event bus (in-process, not HTTP)

---

## Module Structure

Each module follows the same internal layout:

```
src/modules/{module_name}/
  Domain/
    Entities/
    ValueObjects/
    Enums/
    Events/
    StateMachines/
  Application/
    Commands/
    Queries/
    Validators/
    Services/
  Infrastructure/
    Repositories/
    Persistence/
  Contracts/
    DTOs/
    Interfaces/
  Api/
    Controllers/
    Requests/
    Responses/
```

### Layer responsibilities

| Layer | Responsibility |
|-------|----------------|
| Domain | Business logic, state machines, invariants — no framework dependencies |
| Application | Use cases (commands + queries), orchestration |
| Infrastructure | Database access, external services, file storage |
| Contracts | Public interface for other modules — only this is importable externally |
| Api | HTTP controllers — thin, delegates to Application |

---

## Inter-Module Communication

### Synchronous (query/command)

Module A calls Module B only via its `Contracts/Interfaces`. Example:

```
WorkOrders module → IAuthorisationService (from authorisations Contracts)
```

The implementation is injected via DI. WorkOrders never imports `authorisations/Domain/`.

### Asynchronous (events)

When something happens in one module that others need to know about, raise a domain event on the internal bus. Example:

```
Release module raises → MaintenanceReleaseIssuedEvent
Personnel module listens → updates last_active_at on certifying staff
Audit module listens → writes audit event
Notifications module listens → sends alert
```

Events are defined in `shared/domain/events/`.

---

## Database Boundaries

Each module owns a set of tables. No other module may write to those tables.

| Module | Owns Tables Prefixed |
|--------|---------------------|
| aircraft | `aircraft_`, `component_` |
| defects | `defect_`, `defer_` |
| workorders | `work_order_`, `labour_` |
| workpackages | `work_package_`, `due_` |
| maintenance_data | `document_`, `revision_` |
| personnel | `employee_`, `licence_`, `training_`, `roster_` |
| authorisations | `authorisation_` |
| inventory | `part_`, `stock_`, `bin_`, `material_` |
| tooling | `tool_`, `calibration_` |
| release | `inspection_`, `release_`, `signature_` |
| audit | `audit_` |
| auth | `user_`, `session_`, `role_`, `permission_` |
| occurrence | `occurrence_` |
| notifications | `notification_` |

---

## State Machines

Every domain entity with a lifecycle must have an explicit state machine. Status is never a free-form string.

Example — Work Order state machine:

```
draft
  │  (plan)
  ▼
planned
  │  (issue)
  ▼
issued
  │  (start)
  ▼
in_progress ──── (block) ──► waiting_parts
              ├── (block) ──► waiting_tooling
              └── (hold)  ──► waiting_inspection
  │  (complete all tasks)
  ▼
waiting_certification
  │  (release issued)
  ▼
completed
  │  (close)
  ▼
closed

Any state → cancelled (with reason, audit record)
```

State machines live in `Domain/StateMachines/` and are the only place allowed to change an entity's status.

---

## Future Extraction

If a module needs to scale independently or be owned by a separate team, it can be extracted to a separate service without changing other modules — because they only depend on its contract, not its implementation.

This is the main advantage of this architecture over a traditional layered monolith.
