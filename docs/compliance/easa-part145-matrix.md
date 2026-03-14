# EASA Part-145 Compliance Matrix

> Status: DRAFT — to be reviewed by a qualified Part-145 compliance officer before live use.

| Rule | Title | Requirement Summary | Module | Workflow Gate | Audit Evidence |
|------|-------|---------------------|--------|---------------|----------------|
| 145.A.35 | Certifying/support staff | Certifying staff must hold appropriate authorisation; records maintained | personnel, authorisations | Release blocked if authorisation expired or scope mismatch | authorisation record + release signature |
| 145.A.40 | Equipment and tools | Equipment/tools must be controlled; calibration records kept | tooling | Task blocked if required tool has expired calibration | calibration_record |
| 145.A.45 | Maintenance data | Current, approved maintenance data must be available at point of use | maintenance_data | Work order cannot go to 'issued' without linked current document | document_revision + effectivity check |
| 145.A.47 | Production planning | Workload planning must ensure adequate personnel, tools, parts | workpackages | Package readiness score must reach threshold before freeze | package_readiness_snapshot |
| 145.A.48 | Performance of maintenance | Maintenance must be performed per approved data; errors reported | workorders, defects | Task sign-off requires linked document revision | labour_entry + task_completion |
| 145.A.50 | Certification of maintenance | CRS must be issued by authorised certifying staff after satisfactory completion | release | Release blocked unless all tasks complete, inspections done, signer authorised | release_certificate + signature_event |
| 145.A.55 | Record-keeping | Records kept minimum 3 years; protected from unauthorised modification | records | WORM logic on closed records; nightly backup verification | backup_verification_log + checksum |
| 145.A.60 | Occurrence reporting | Safety occurrences must be reported; internal process documented | occurrence | Defect severity triggers occurrence workflow; deadline tracked | occurrence_report + submission_log |
| 145.A.65 | Maintenance procedures | Organisation must have procedures covering all maintenance activities | all modules | Procedure documents linked to work order types | document_revision |

## Sources

- EASA Regulation (EU) No 1321/2014, Annex II (Part-145)
- EASA AMC and GM to Part-145 (latest consolidated version)
