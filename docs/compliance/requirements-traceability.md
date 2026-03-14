# Requirements Traceability Matrix

**Version:** 1.0
**Date:** 2026-03-14
**Status:** Draft

---

## Purpose

This matrix traces every system requirement back to its source (regulatory rule or business need), forward to its implementation (module + feature), and to its verification (test case).

---

## Requirement Categories

| Prefix | Category |
|--------|----------|
| REG- | Regulatory requirement (EASA / FAA) |
| BIZ- | Business / operational requirement |
| SEC- | Security requirement |
| ARC- | Architectural requirement |

---

## Regulatory Requirements

| Req ID | Source Rule | Requirement Statement | Module | Feature | Test Case | Status |
|--------|------------|----------------------|--------|---------|-----------|--------|
| REG-001 | EASA 145.A.35 | System must verify certifying staff authorisation (scope, expiry, currency) before allowing release sign-off | authorisations, release | Authorisation scope check | TC-AUTH-001 | Planned |
| REG-002 | EASA 145.A.35 | Authorisation records must be stored with expiry and scope and be auditable | authorisations | Authorisation register | TC-AUTH-002 | Planned |
| REG-003 | EASA 145.A.40 | Tool calibration due dates must be tracked; expired calibration must block task sign-off | tooling | Calibration tracking + task gate | TC-TOOL-001 | Planned |
| REG-004 | EASA 145.A.40 | Out-of-service tools must be flagged and blocked from assignment | tooling | Tool status management | TC-TOOL-002 | Planned |
| REG-005 | EASA 145.A.45 | Maintenance data must be current at point of use; superseded data must not be usable | maintenance_data | Document revision lifecycle | TC-DOC-001 | Planned |
| REG-006 | EASA 145.A.47 | Work package readiness must be verified before execution start | workpackages | Package readiness score | TC-PKG-001 | Planned |
| REG-007 | EASA 145.A.48 | Each task must reference the document revision used during execution | workorders | Task sign-off with doc reference | TC-WO-001 | Planned |
| REG-008 | EASA 145.A.50 | CRS can only be issued when all tasks complete, all inspections done, signer authorised | release | Release gate checks | TC-REL-001 | Planned |
| REG-009 | EASA 145.A.55 | Records must be protected from modification; retained minimum 3 years | records | WORM records + retention policy | TC-REC-001 | Planned |
| REG-010 | EASA 145.A.55 | Backup must exist, be verified, and restorable | records | Backup verification job | TC-REC-002 | Planned |
| REG-011 | EASA 145.A.60 | Safety occurrences must trigger a reportable workflow with deadline | occurrence | Occurrence report lifecycle | TC-OCC-001 | Planned |
| REG-012 | FAA §145.155 | Inspector must be different person from the one who performed the task | personnel, release | Inspector ≠ performer check | TC-FAA-004 | Planned |
| REG-013 | FAA §145.157 | Training records must be maintained; currency checked before task assignment | personnel | Training currency check | TC-FAA-005 | Planned |
| REG-014 | FAA §145.211 | Quality control inspection must be completed before maintenance release | release | QC inspection gate | TC-FAA-006 | Planned |
| REG-015 | FAA §145.219 | Work must not exceed station and staff authorisation scope | authorisations | Rating + scope check on WO creation | TC-FAA-010 | Planned |

---

## Business Requirements

| Req ID | Source | Requirement Statement | Module | Feature | Test Case | Status |
|--------|--------|----------------------|--------|---------|-----------|--------|
| BIZ-001 | Operations | Aircraft counters (FH, FC, landings) must be updateable and drive due item calculations | aircraft | Counter update + due calculation | TC-AC-001 | Planned |
| BIZ-002 | Operations | Deferred defects must have a due date and raise alerts when overdue | defects | Deferral + overdue alert | TC-DEF-001 | Planned |
| BIZ-003 | Operations | Repeat defects must be automatically identified and flagged | defects | Repeat defect detection | TC-DEF-002 | Planned |
| BIZ-004 | Stores | Parts must have full serial/batch traceability from receipt to installation | inventory | Serial + batch trace | TC-INV-001 | Planned |
| BIZ-005 | Stores | Shelf-life controlled items must be blocked from issue when expired | inventory | Shelf-life check on issue | TC-INV-002 | Planned |
| BIZ-006 | Planning | Work package must show a readiness percentage (docs, parts, tools, staff) | workpackages | Readiness score | TC-PKG-001 | Planned |
| BIZ-007 | Mobile | Mechanics must be able to execute tasks and sign off from a mobile device | mobile | Task execution + mobile sign-off | TC-MOB-001 | Planned |
| BIZ-008 | Quality | Internal findings must trigger CAPA with closure approval | quality | Finding + CAPA workflow | TC-QA-001 | Planned |

---

## Security Requirements

| Req ID | Source | Requirement Statement | Module | Feature | Test Case | Status |
|--------|--------|----------------------|--------|---------|-----------|--------|
| SEC-001 | EASA Part-IS | System must have MFA for all users with release or admin access | auth | MFA enforcement | TC-SEC-001 | Planned |
| SEC-002 | General | All data in transit must be encrypted (TLS 1.2+) | infrastructure | TLS configuration | TC-SEC-002 | Planned |
| SEC-003 | General | All data at rest must be encrypted | infrastructure | Disk/DB encryption | TC-SEC-003 | Planned |
| SEC-004 | Audit | All critical actions must produce immutable audit events | audit | Audit event pipeline | TC-SEC-004 | Planned |
| SEC-005 | General | RBAC must enforce access at module and operation level | auth | Permission enforcement | TC-SEC-005 | Planned |
| SEC-006 | General | Privileged actions (release, record delete) must require secondary approval | auth, release | Privileged action approval | TC-SEC-006 | Planned |

---

## Architectural Requirements

| Req ID | Source | Requirement Statement | Module | ADR | Status |
|--------|--------|----------------------|--------|-----|--------|
| ARC-001 | ADR-001 | System must be a modular monolith; modules may not access each other's DB tables directly | all | ADR-001 | Active |
| ARC-002 | ADR-002 | Backend: C# / .NET; Frontend: React + TypeScript; DB: PostgreSQL | all | ADR-002 | Active |
| ARC-003 | ADR-003 | All design must follow clean-room policy | all | ADR-003 | Active |
| ARC-004 | General | Every domain entity must use state machines, not free-form status strings | workorders, defects, release | — | Active |
| ARC-005 | General | All migrations must be versioned and forward-only | database | — | Active |
