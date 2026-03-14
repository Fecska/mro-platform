# MVP Scope

**Version:** 1.0
**Date:** 2026-03-14

---

## Goal

The MVP is the minimum set of features that allows a single Part-145 maintenance organisation to manage its day-to-day line and light base maintenance operations in the system, replacing paper or spreadsheet-based processes.

The MVP is not feature-complete. It is the smallest usable, safe, and auditable system.

---

## In MVP

### Aircraft & Fleet
- [ ] Aircraft master (registration, type, MSN, config)
- [ ] Aircraft counter management (FH, FC, landings)
- [ ] Aircraft status (serviceable / AOG / in-maintenance)
- [ ] Installed component tracking (engine, APU, major components)

### Technical Log & Defects
- [ ] Defect entry (ATA, description, severity, reporter)
- [ ] Defect triage (MEL/CDL reference, deferral decision)
- [ ] Deferral with due limits (FH/FC/calendar)
- [ ] Overdue deferral alerts
- [ ] Repeat defect detection
- [ ] Defect-to-work-order linking

### Maintenance Data
- [ ] Document upload and versioning (AMM, CMM, SB, AD, task card)
- [ ] Document effectivity (by aircraft / config)
- [ ] Superseded revision blocking
- [ ] Link document to work order task

### Work Orders
- [ ] Work order creation (from defect, from due item, manual)
- [ ] Work order state machine (draft → planned → issued → in_progress → completed → closed)
- [ ] Task management within work order
- [ ] Required parts, tools, skills per task
- [ ] Task sign-off with document reference
- [ ] Labour time entry
- [ ] All compliance hard stops enforced

### Personnel & Authorisations
- [ ] Employee register
- [ ] Licence management (EASA Part-66 / FAA A&P)
- [ ] Authorisation register (certifying staff scope)
- [ ] Authorisation expiry alerts
- [ ] Training record entry and currency check

### Inventory (Basic)
- [ ] Part number master
- [ ] Stock item register (serialised and batch)
- [ ] Material reservation to work order
- [ ] Material issue from stores
- [ ] Trace document attachment (Form 1 / 8130)
- [ ] Shelf-life expiry check
- [ ] Quarantine management
- [ ] Basic bin locations

### Tooling
- [ ] Tool register
- [ ] Calibration due date tracking
- [ ] Tool assignment to work order
- [ ] Expired calibration block

### Inspection & Release
- [ ] Inspection record per work order
- [ ] Independent inspector enforcement
- [ ] Release certificate generation
- [ ] Electronic signature (internal, basic)
- [ ] Release gate checks (all hard stops)

### Records
- [ ] Closed work order archiving
- [ ] Release certificate storage
- [ ] WORM-like protection on closed records
- [ ] Basic record search

### Audit
- [ ] Immutable audit log for all state changes
- [ ] Compliance action log (release, inspection, sign-off)

### Auth & Security
- [ ] User accounts with role assignment
- [ ] MFA for certifying staff, release, admin
- [ ] RBAC with operational scope (station, aircraft type)
- [ ] Session management

### Web UI
- [ ] Dashboard (due items, open defects, open WOs, upcoming releases)
- [ ] Aircraft detail page
- [ ] Defect management screens
- [ ] Work order management screens
- [ ] Stores screens
- [ ] Personnel / authorisation screens

### Mobile (basic)
- [ ] Task execution screen (view assigned tasks, sign off)
- [ ] Defect reporting from device
- [ ] Material issue confirmation

---

## Explicitly Out of MVP

| Feature | Target Version |
|---------|---------------|
| Work package builder | v1.1 |
| Due item auto-generation from maintenance program | v1.1 |
| Occurrence reporting workflow | v1.1 |
| Quality findings / CAPA | v1.2 |
| Production planning / readiness score | v1.1 |
| Purchasing / PO management | v2 |
| Customer billing | v2 |
| Quotation / contracts | v2 |
| Multi-entity / multi-AOC | v2 |
| Reliability reporting | v2 |
| External authority portal | v3 |
| Full offline mobile sync | v1.2 |

---

## MVP Success Criteria

The MVP is considered complete when:

1. A defect can be entered, triaged, and linked to a work order
2. A work order can move through all states with all hard stops enforced
3. A part can be reserved, issued, and traced to the task
4. An inspection can be recorded by an independent inspector
5. A release to service can be signed by an authorised certifying staff member
6. The signed release certificate is stored and retrievable
7. The complete audit trail of the above is available for review
8. All 13 hard stops are covered by automated tests

---

## Sprint Plan (High Level)

| Sprint | Focus |
|--------|-------|
| 1 | Repo, auth, audit skeleton, organisations/stations, aircraft master |
| 2 | Defects, maintenance data, basic work orders |
| 3 | Personnel, authorisations, inspection, release |
| 4 | Inventory, tooling, hard stops, compliance gates |
| 5 | Web UI polish, mobile basic, dashboard |
| 6 | Testing, bug fixes, MVP acceptance |
