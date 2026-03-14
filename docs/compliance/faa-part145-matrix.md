# FAA Part 145 Compliance Matrix

> Status: DRAFT — to be reviewed by a qualified FAA repair station compliance officer before live use.

| Rule | Title | Requirement Summary | Module | Workflow Gate | Audit Evidence |
|------|-------|---------------------|--------|---------------|----------------|
| §145.109 | Equipment, materials, data | Must have equipment, materials, and technical data to perform rated work | maintenance_data, tooling, inventory | Work order blocked without current data, required tooling, required parts | document link + tool assignment + material reservation |
| §145.151 | Personnel | Must have adequate personnel; qualified for work performed | personnel | Assignment blocked if employee not qualified for task type | employee qualifications + assignment record |
| §145.153 | Supervisory personnel | Must have adequate supervisory personnel | personnel | Shift must have assigned supervisor | roster_shift + supervisor flag |
| §145.155 | Inspection personnel | Inspectors must be qualified; independent from performing staff | personnel, release | Inspection cannot be signed by same person who performed the task | inspection + performer/inspector cross-check |
| §145.157 | Personnel training | Training programme required; records kept minimum 2 years | personnel | Training currency checked before assignment to task type | training_record |
| §145.211 | Quality control system | Must have quality control system and manual | audit, release | Release blocked without passing inspection checklist | inspection_record |
| §145.213 | Inspection of maintenance | All maintenance must be inspected | release | Every work order requires inspection before release | inspection + release_certificate |
| §145.215 | Records | Maintenance release copy to owner/operator; records in English | records | Release issued as exportable record in required format | release_certificate + delivery_log |
| §145.217 | Capability list | Repair station must maintain capability list | maintenance_data | Work order type must match approved capability | capability_check |
| §145.219 | Maintenance functions | May not perform functions outside approved ratings | authorisations | Work order type/aircraft type must match station rating | rating_check + authorisation_scope |

## Sources

- 14 CFR Part 145 (current version via FAA eCFR)
- FAA Order 8900.1 (relevant chapters)
