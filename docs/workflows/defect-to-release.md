# Workflow: Defect to Release

**Version:** 1.0
**Date:** 2026-03-14

---

## Overview

This is the primary operational workflow of the system. A defect is discovered, a work order is raised, the work is planned and executed, an inspection is performed, and a release to service is issued.

---

## Full Flow

```
1. Defect reported
       │
       ▼
2. Defect triaged (ATA chapter, severity, MEL/CDL check)
       │
       ├──── Deferrable? ──► Deferral applied → due date set → monitor
       │
       ▼ (not deferred)
3. Work order created from defect
       │
       ▼
4. Work order planned
   - Maintenance data linked (current revision required) [HARD STOP E-06]
   - Required personnel assigned (qualification check) [HARD STOP HS-011]
   - Required parts reserved (traceability check, shelf-life check) [HARD STOP HS-009, HS-010]
   - Required tools assigned (calibration check) [HARD STOP HS-002]
       │
       ▼
5. Work order issued
   [Gate: all required docs linked and current] [HARD STOP HS-003]
       │
       ▼
6. Work order in progress
   [Gate: no open safety block] [HARD STOP HS-007]
   - Mechanic executes tasks
   - Each task signed off (with doc reference, tool reference)
   - Labour time booked
   - Parts issued from stores [HARD STOP HS-008, HS-009, HS-010]
       │
       ▼
7. All tasks completed
       │
       ▼
8. Inspection performed
   [Gate: inspector ≠ task performer] [HARD STOP HS-004]
   - Inspector reviews work
   - Inspector signs inspection record
       │
       ▼
9. Release to service
   [Gate: all tasks complete] [HARD STOP HS-005]
   [Gate: all required inspections done] [HARD STOP HS-006]
   [Gate: signer authorisation valid for scope] [HARD STOP HS-001]
   - Certifying staff signs CRS / maintenance release
   - Electronic signature event recorded
   - Release certificate generated
       │
       ▼
10. Records archived
    - Work order closed
    - Defect closed
    - Aircraft counters updated
    - Release certificate stored (WORM)
    - Audit trail complete
```

---

## Defect States

| State | Description |
|-------|-------------|
| `reported` | Freshly entered in system |
| `triaged` | ATA, severity, MEL check done |
| `open` | Active, work order not yet created |
| `deferred` | Deferral applied, due date set |
| `rectification_in_progress` | Work order open and in progress |
| `inspection_pending` | Tasks done, awaiting inspection |
| `cleared` | Inspection and release complete |
| `closed` | Records archived |

---

## Work Order States

| State | Description |
|-------|-------------|
| `draft` | Created, not yet planned |
| `planned` | Parts/tools/docs/personnel assigned |
| `issued` | Released to workshop |
| `in_progress` | Work has started |
| `waiting_parts` | Blocked on missing material |
| `waiting_tooling` | Blocked on missing/uncalibrated tool |
| `waiting_inspection` | Tasks done, waiting inspector |
| `waiting_certification` | Inspection done, waiting CRS |
| `completed` | CRS issued |
| `closed` | Records archived |
| `cancelled` | Cancelled with reason |

---

## Release States

| State | Description |
|-------|-------------|
| `not_required` | No release needed (internal job) |
| `required` | Release required, not yet signed |
| `inspection_pending` | Awaiting inspection completion |
| `signoff_pending` | Inspection done, awaiting certifying staff |
| `issued` | CRS signed and issued |
| `superseded` | Replaced by a new release |
| `revoked` | Revoked with reason and audit record |

---

## Deferral Sub-Flow

```
Defect triaged → MEL/CDL reference selected
                        │
                        ▼
              Deferral category applied
              Due limits set (FH / FC / calendar)
                        │
                        ▼
              Daily check: any deferred defect past due?
                        │
              YES ──────┴──── Alert raised → Maintenance Control
                              Aircraft flagged non-dispatchable [HARD STOP HS-013]
                              Work order auto-created
```

---

## Actors

| Actor | Role in this workflow |
|-------|-----------------------|
| Line mechanic | Reports defect, executes tasks |
| Certifying staff | Issues release to service |
| Inspector | Signs inspection (must be independent) |
| Maintenance Control | Manages deferrals and AOG decisions |
| Stores | Issues and traces parts |
| Planner | Plans work packages |
