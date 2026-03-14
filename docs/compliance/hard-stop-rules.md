# Hard Stop Rules

**Version:** 1.0
**Date:** 2026-03-14
**Status:** Active

---

## Purpose

Hard stops are non-negotiable system blocks. Unlike warnings, they cannot be overridden by the user and must prevent the action from proceeding. This list is derived directly from EASA Part-145, FAA Part 145, and safety-critical operational logic.

No developer, product owner, or user may bypass a hard stop without a formal change control record.

---

## Hard Stop Definitions

### HS-001 — Expired Authorisation Blocks Release

**Trigger:** User attempts to issue a Release to Service (CRS/maintenance release)
**Condition:** The signing user's authorisation has expired OR does not cover the aircraft type/task category
**Action:** Block release. Display: "Release blocked: your authorisation (ref: {id}) expired on {date} or does not cover this scope."
**Regulatory basis:** EASA 145.A.35, FAA §145.219
**Cannot be overridden by:** any role, including admin

---

### HS-002 — Expired Calibration Blocks Task Sign-Off

**Trigger:** User attempts to sign off a task
**Condition:** A required tool assigned to the task has a calibration due date in the past
**Action:** Block sign-off. Display: "Sign-off blocked: tool {tool_ref} calibration expired on {date}. Recalibrate or assign a valid substitute."
**Regulatory basis:** EASA 145.A.40, FAA §145.109
**Cannot be overridden by:** any role

---

### HS-003 — Superseded Document Blocks Work Order

**Trigger:** User attempts to link a maintenance document to a new work order
**Condition:** The selected document revision has status `superseded` or `obsolete`
**Action:** Block link. Display: "Document revision {rev} is superseded. Use revision {current_rev} or later."
**Regulatory basis:** EASA 145.A.45, FAA §145.109
**Cannot be overridden by:** any role

---

### HS-004 — Inspector = Performer Blocks Inspection Sign-Off

**Trigger:** User attempts to sign an inspection on a task
**Condition:** The user performed labour on the same task (i.e. has a `labour_entry` on this task)
**Action:** Block inspection sign-off. Display: "Independent inspection required. You performed work on this task and cannot inspect it."
**Regulatory basis:** FAA §145.155, EASA 145.A.50
**Cannot be overridden by:** any role

---

### HS-005 — Incomplete Tasks Block Release

**Trigger:** User attempts to issue release certificate
**Condition:** One or more work order tasks have status != `completed`
**Action:** Block release. Display: "Release blocked: {n} task(s) not yet completed."
**Regulatory basis:** EASA 145.A.50, FAA §145.213
**Cannot be overridden by:** any role

---

### HS-006 — Missing Inspection Blocks Release

**Trigger:** User attempts to issue release certificate
**Condition:** Required inspection(s) not signed off
**Action:** Block release. Display: "Release blocked: required inspection(s) not completed."
**Regulatory basis:** EASA 145.A.50, FAA §145.213, FAA §145.211
**Cannot be overridden by:** any role

---

### HS-007 — Open Safety Block on Work Order

**Trigger:** User attempts to move work order to `in_progress`
**Condition:** A safety block flag is active on the work order
**Action:** Block transition. Display: "Work order has an open safety block. Resolve block {ref} before starting."
**Regulatory basis:** EASA 145.A.48
**Cannot be overridden by:** any role except Quality Manager (with mandatory audit record)

---

### HS-008 — Quarantined Part Cannot Be Issued

**Trigger:** User attempts to issue a stock item to a work order
**Condition:** The stock item has status `quarantine`
**Action:** Block issue. Display: "Part {pn} / {serial} is in quarantine and cannot be issued."
**Regulatory basis:** EASA 145.A.40, FAA §145.109
**Cannot be overridden by:** any role

---

### HS-009 — Expired Shelf-Life Blocks Part Issue

**Trigger:** User attempts to issue a shelf-life controlled stock item
**Condition:** `shelf_life_expires_at` is in the past
**Action:** Block issue. Display: "Part {pn} shelf life expired on {date}. This item must be quarantined."
**Regulatory basis:** EASA 145.A.40, FAA §145.109
**Cannot be overridden by:** any role

---

### HS-010 — Missing Trace Document Blocks Part Issue

**Trigger:** User attempts to issue a serialised or batch-controlled part
**Condition:** No approved trace document (cert, 8130, EASA Form 1) attached to stock item
**Action:** Block issue. Display: "Part {pn} has no release document. Attach Form 1 / 8130 or certificate before issuing."
**Regulatory basis:** EASA 145.A.42, FAA §43.9
**Cannot be overridden by:** any role

---

### HS-011 — Unqualified Personnel Cannot Be Assigned

**Trigger:** User attempts to assign an employee to a task
**Condition:** Employee does not hold the required qualification/licence category for the task type
**Action:** Block assignment. Display: "Employee {name} is not qualified for task category {cat}."
**Regulatory basis:** EASA 145.A.35, FAA §145.151
**Cannot be overridden by:** any role

---

### HS-012 — Work Package Not Ready Cannot Be Frozen

**Trigger:** User attempts to freeze (publish) a work package for execution
**Condition:** Package readiness score is below threshold (default: 80%) for any of: docs, parts, tooling, staff
**Action:** Block freeze. Display: "Work package readiness: {score}%. Minimum 80% required. Blockers: {list}."
**Regulatory basis:** EASA 145.A.47
**Cannot be overridden by:** Planner (can request override from Production Manager with audit record)

---

### HS-013 — Deferred Defect Past Due Date

**Trigger:** System scheduled check or user action on aircraft
**Condition:** A deferred defect has passed its due date (FH, FC, calendar)
**Action:** Aircraft flagged as non-dispatchable. Alert raised to Maintenance Control. Work order auto-created.
**Regulatory basis:** EASA 145.A.48 (MEL/CDL deferral limits), operator procedures
**Cannot be overridden by:** any role without Maintenance Controller approval and audit record

---

## Change Control

Any modification to a hard stop rule requires:

1. Written justification
2. Review by Quality Manager
3. Compliance officer sign-off (for regulatory-based rules)
4. ADR or change record created in `docs/adr/`
5. Test case updated

---

## Revision History

| Version | Date | Change |
|---------|------|--------|
| 1.0 | 2026-03-14 | Initial version — 13 hard stops defined |
