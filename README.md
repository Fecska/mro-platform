# MRO Platform

Aircraft-first Maintenance, Repair & Overhaul platform built on Part-145/MRO logic.

## Architecture

Modular monolith — C# / .NET API, React web, React Native mobile, PostgreSQL, Redis, RabbitMQ.

See [docs/architecture/](docs/architecture/) for ADRs and system design.

## Modules

| Module | Description |
|--------|-------------|
| aircraft | Fleet master, configuration, counters |
| defects | Technical log, defect lifecycle |
| workorders | Work order engine, state machine |
| workpackages | Production planning, readiness |
| personnel | Employees, licences, shifts |
| authorisations | Certifying staff, scope, currency |
| maintenance_data | Document control, revisions |
| inventory | Parts, stock, traceability |
| tooling | Tools, calibration |
| release | Inspection, CRS, e-signature |
| audit | Immutable audit events |
| auth | Auth, RBAC, session |
| notifications | Alerts, push, email |

## Getting Started

```bash
cp .env.example .env
docker compose up -d
```

## Compliance

See [docs/compliance/](docs/compliance/) for regulatory mapping (EASA Part-145, FAA Part 145).

> This platform supports the operational processes of a Part-145 approved organisation.
> Software alone does not constitute regulatory approval.
