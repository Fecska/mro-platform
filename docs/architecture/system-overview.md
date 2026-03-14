# System Overview

**Version:** 1.0
**Date:** 2026-03-14

---

## What the System Is

The MRO Platform is an aircraft-first Maintenance, Repair & Overhaul management system. It covers the full maintenance lifecycle from defect reporting through work order execution to release to service and records archiving.

It is designed primarily for Part-145 approved maintenance organisations operating under EASA or FAA regulation.

---

## Core Lifecycle

```
Aircraft configuration
       │
       ▼
Maintenance requirements (due items)
       │
       ▼
Defect / Technical log entry
       │
       ▼
Work order creation
       │
       ▼
Work package planning (personnel + parts + tools + docs ready)
       │
       ▼
Task execution (mobile or desktop)
       │
       ▼
Inspection
       │
       ▼
Release to service (CRS / maintenance release)
       │
       ▼
Records archiving + audit trail
```

---

## System Boundaries

### In Scope (v1)

- Fleet / aircraft master and configuration
- Aircraft counters (FH, FC, landings)
- Technical log and defect management
- Work order lifecycle
- Work package planning and readiness
- Maintenance document control
- Personnel, licences, authorisations
- Stores / inventory / traceability
- Tooling and calibration
- Inspection and release to service
- Records and archiving
- Occurrence reporting
- Internal quality findings
- Mobile task execution (basic)
- Audit log

### Out of Scope (v1 — planned for later)

- Customer billing / invoicing
- Quotation and contract management
- CRM
- Full reliability / ETOPS module
- Vendor portal
- Multi-entity / multi-AOC hierarchy
- Airworthiness review
- External authority portal integration

---

## Deployment Model (v1)

Single organisation deployment. One database. One backend instance. One web frontend. Mobile apps connect to same backend.

```
Mobile (React Native)
         │
         ▼
Web browser (React)
         │
         ▼
   API (.NET 9)  ──── Redis cache
         │
         ▼
  PostgreSQL DB
         │
         ▼
  S3 storage (documents, attachments)
         │
  RabbitMQ (async jobs: notifications, backup verification, alerts)
```

---

## Key Non-Functional Requirements

| Property | Target |
|----------|--------|
| Availability | 99.5% (single org, scheduled maintenance windows allowed) |
| Audit immutability | Audit events are append-only, never modified or deleted |
| Record protection | Closed maintenance records: WORM-like, checksum verified |
| Data encryption | TLS 1.2+ in transit, AES-256 at rest |
| Backup | Nightly, verified, separate storage location |
| Session | MFA required for release and admin roles |
| Offline mobile | Basic read + queue-based write sync for task execution |

---

## Module Map

```
┌─────────────────────────────────────────────────────────┐
│                        MRO Platform                      │
├───────────┬──────────┬───────────┬────────┬─────────────┤
│ aircraft  │ defects  │workorders │  docs  │  personnel  │
│ fleet     │ tech log │ work pkgs │ revisions│ licences  │
│ counters  │ deferred │ labour    │ effectiv │ auth      │
├───────────┴──────────┴───────────┴────────┴─────────────┤
│ inventory │ tooling  │  release  │  audit │  occurrence │
│ parts     │ calib    │ inspection│ events │  reports    │
│ stock     │ kits     │ e-sign    │ immut  │  safety     │
├───────────┴──────────┴───────────┴────────┴─────────────┤
│              auth  │  notifications  │  shared          │
└───────────────────────────────────────────────────────────┘
```
