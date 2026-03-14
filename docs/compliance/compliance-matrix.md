# Compliance Matrix

**Version:** 1.0
**Date:** 2026-03-14
**Status:** Draft — requires review by a qualified Part-145 compliance officer

---

## How to Read This Matrix

Each row maps one regulatory requirement to:
- the module(s) that implement it
- the workflow gate that enforces it at runtime
- the audit evidence the system produces
- the test case that verifies it

---

## EASA Part-145

| ID | Rule | Requirement | Module(s) | Workflow Gate | Audit Evidence | Test Case |
|----|------|-------------|-----------|---------------|----------------|-----------|
| E-01 | 145.A.35 | Certifying staff must hold a valid authorisation with matching scope before issuing CRS | authorisations, release | Release: signer authorisation check (scope + expiry + currency) | `authorisation` record + `signature_event` | `TC-AUTH-001` |
| E-02 | 145.A.35 | Authorisation records must be maintained and accessible | authorisations | Read access to authorisation register for auditors | `authorisation` + `employee` | `TC-AUTH-002` |
| E-03 | 145.A.40 | Equipment and tools must be controlled; calibration records kept | tooling | Task cannot be signed off if required tool has expired calibration | `calibration_record` + `tool_event` | `TC-TOOL-001` |
| E-04 | 145.A.40 | Out-of-service tools must be quarantined | tooling | Tool marked out-of-service blocks assignment | `tool` status flag | `TC-TOOL-002` |
| E-05 | 145.A.45 | Current approved maintenance data must be available at the point of use | maintenance_data | Work order cannot move to `issued` without a linked current document revision | `document_revision` + effectivity check | `TC-DOC-001` |
| E-06 | 145.A.45 | Superseded or obsolete document revisions must not be used | maintenance_data | Superseded revision cannot be linked to a new work order | `document_revision.status` check | `TC-DOC-002` |
| E-07 | 145.A.47 | Production planning must ensure adequate personnel, tools, and parts | workpackages | Work package readiness score must meet threshold before freeze | `package_readiness_snapshot` | `TC-PKG-001` |
| E-08 | 145.A.48 | Maintenance must be performed per approved data | workorders | Task sign-off requires linked document revision to be acknowledged | `labour_entry` + `task_completion` | `TC-WO-001` |
| E-09 | 145.A.48 | Errors or omissions must be reportable | defects, occurrence | Defect can be promoted to occurrence report at any time | `occurrence_report` | `TC-OCC-001` |
| E-10 | 145.A.50 | CRS must be issued by authorised certifying staff after satisfactory completion | release | Release blocked unless: all tasks complete, all required inspections done, signer authorised for scope | `release_certificate` + `signature_event` | `TC-REL-001` |
| E-11 | 145.A.55 | Maintenance records kept minimum 3 years; protected from unauthorised modification | records | WORM logic applied to closed records; checksum verified on every read | `archived_record` + `backup_verification_log` | `TC-REC-001` |
| E-12 | 145.A.55 | Backup must exist and be verified | records | Nightly backup verification job; restore test monthly | `backup_verification_log` | `TC-REC-002` |
| E-13 | 145.A.60 | Safety occurrences must be reported | occurrence | Defect severity trigger creates occurrence with deadline; overdue alerts raised | `occurrence_report` + `submission_log` | `TC-OCC-002` |
| E-14 | 145.A.65 | Organisation must have maintenance procedures covering all activities | maintenance_data | Work order type must have a linked procedure document | `document_revision` | `TC-DOC-003` |

---

## FAA Part 145

| ID | Rule | Requirement | Module(s) | Workflow Gate | Audit Evidence | Test Case |
|----|------|-------------|-----------|---------------|----------------|-----------|
| F-01 | §145.109 | Must have equipment, materials, and technical data for rated work | maintenance_data, tooling, inventory | WO blocked without current data + required tooling + required material | doc link + tool assignment + material reservation | `TC-FAA-001` |
| F-02 | §145.151 | Adequate personnel; qualified for work performed | personnel | Assignment blocked if employee not qualified for task category | `employee` qualifications + `work_order_assignment` | `TC-FAA-002` |
| F-03 | §145.153 | Adequate supervisory personnel | personnel | Shift must have an assigned supervisor | `roster_shift.supervisor_id` | `TC-FAA-003` |
| F-04 | §145.155 | Inspectors must be independent from performing personnel | personnel, release | Inspection sign-off blocked if inspector = task performer on same task | cross-check on `inspection` + `labour_entry` | `TC-FAA-004` |
| F-05 | §145.157 | Training programme required; records kept minimum 2 years | personnel | Training currency checked before assignment to task category | `training_record` | `TC-FAA-005` |
| F-06 | §145.211 | Quality control system required | audit, release | Release requires passing inspection checklist per QC procedure | `inspection_record` | `TC-FAA-006` |
| F-07 | §145.213 | All maintenance must be inspected before release | release | Every work order requires at least one completed inspection before CRS | `inspection` + `release_certificate` | `TC-FAA-007` |
| F-08 | §145.215 | Maintenance release copy to owner/operator; records in English | records | Release certificate exportable in required format | `release_certificate` + delivery export | `TC-FAA-008` |
| F-09 | §145.217 | Repair station must maintain capability list | maintenance_data | Work order type must match approved capability | capability check on WO creation | `TC-FAA-009` |
| F-10 | §145.219 | May not perform functions outside approved ratings | authorisations | WO type and aircraft type must match station rating and staff authorisation | rating + authorisation scope check | `TC-FAA-010` |

---

## Hard Stop Summary

The following conditions must result in a hard system block (not a warning):

1. Signer authorisation expired → release blocked
2. Required tool calibration expired → task sign-off blocked
3. Maintenance document superseded → cannot link to new WO
4. Work package not ready → cannot freeze
5. Inspector = performer on same task → inspection sign-off blocked
6. Open safety block on WO → WO cannot move to `in_progress`
7. Deferred defect past due date → red alert + WO auto-created
8. Quarantined stock → cannot be issued to WO
9. Employee not qualified for task category → cannot be assigned
10. All tasks not complete → release blocked

---

## Sources

- EASA Regulation (EU) No 1321/2014, Annex II (Part-145), with AMC and GM
- 14 CFR Part 145 (FAA eCFR, current version)
- FAA Order 8900.1
