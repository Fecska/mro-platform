# Workflow: Work Package Planning

**Version:** 1.0
**Date:** 2026-03-14

---

## Overview

A work package groups multiple work orders for a planned maintenance input (e.g. a scheduled check, a base maintenance visit). The planning process ensures all resources are confirmed before execution begins.

---

## Flow

```
1. Maintenance input identified
   - Due items from maintenance program (FH/FC/calendar limits)
   - Deferred defects coming due
   - Operator requests (modifications, STCs)
       │
       ▼
2. Work package created
   - Aircraft
   - Planned start date / end date
   - Station
   - Package type (line / light base / heavy / component)
       │
       ▼
3. Work orders added to package
   - Due items → auto-generated WOs
   - Deferred defects → linked WOs
   - Additional tasks → manual WOs
       │
       ▼
4. Readiness planning — four dimensions

   ┌─────────────────┬──────────────────────────────────┐
   │ Dimension       │ Check                            │
   ├─────────────────┼──────────────────────────────────┤
   │ Documents       │ All WO tasks have current linked  │
   │                 │ maintenance data                  │
   ├─────────────────┼──────────────────────────────────┤
   │ Parts           │ All required parts reserved or    │
   │                 │ on order; trace docs present      │
   ├─────────────────┼──────────────────────────────────┤
   │ Tooling         │ All required tools available and  │
   │                 │ calibrated for input dates        │
   ├─────────────────┼──────────────────────────────────┤
   │ Personnel       │ Qualified, authorised staff       │
   │                 │ rostered for the input period     │
   └─────────────────┴──────────────────────────────────┘

   Readiness score = % of WOs where all 4 dimensions are green
       │
       ▼
5. Readiness snapshot saved
   - Stored in `package_readiness_snapshots`
   - Visible to planner and maintenance control
   - Updated whenever a resource is confirmed or a new WO is added
       │
       ▼
6. Package freeze (publish)
   [Gate: readiness score ≥ threshold] [HARD STOP HS-012]
   - Package locked for editing (change control applies)
   - Work orders move to `issued` state
   - Parts reservations confirmed
       │
       ▼
7. Execution
   - Mechanics work from issued work orders
   - Any additional work found → amendment process
       │
       ▼
8. Package closure
   - All WOs completed and released
   - Package closed
   - Records archived
   - Aircraft counters updated
   - Due items reset
```

---

## Readiness Score Calculation

```
score = (
  (WOs with docs ready / total WOs) × 0.30
+ (WOs with parts ready / total WOs) × 0.30
+ (WOs with tooling ready / total WOs) × 0.20
+ (WOs with staff assigned / total WOs) × 0.20
) × 100
```

Default freeze threshold: **80%**. Configurable per organisation.

---

## Package States

| State | Description |
|-------|-------------|
| `draft` | Being built |
| `planning` | Readiness checks in progress |
| `ready` | Score ≥ threshold, ready to freeze |
| `frozen` | Locked, work orders issued |
| `in_progress` | Execution started |
| `completed` | All WOs released |
| `closed` | Records archived |
| `cancelled` | Cancelled with reason |

---

## Amendment Process (After Freeze)

Any change to a frozen package requires:

1. Change reason documented
2. Planner or Maintenance Control approval
3. Affected WO returned to `planned` state
4. Re-issue after resource confirmation
5. Audit event created

---

## Actors

| Actor | Role |
|-------|------|
| Planner | Creates and manages work package |
| Stores | Confirms parts reservations |
| Tool store | Confirms tool availability and calibration |
| Maintenance Control | Approves freeze, manages amendments |
| Mechanic / Certifying staff | Executes after freeze |
