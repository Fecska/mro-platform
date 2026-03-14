# Database Schema Overview

**Version:** 1.0
**Date:** 2026-03-14

---

## Database

PostgreSQL 16. Single database for v1 (modular monolith). Each module's tables are logically grouped by prefix.

---

## Migration Strategy

- All schema changes via numbered migration files in `database/migrations/`
- Forward-only migrations (no down migrations in production)
- Naming: `{number}_{module}_{description}.sql`
- Example: `001_auth_users_roles.sql`, `006_aircraft_master.sql`

---

## Table Groups by Module

### auth
| Table | Purpose |
|-------|---------|
| `users` | Platform user accounts |
| `roles` | Role definitions |
| `permissions` | Permission definitions |
| `user_roles` | User ↔ role assignments |
| `role_permissions` | Role ↔ permission assignments |
| `sessions` | Active sessions (JWT blocklist) |
| `mfa_factors` | MFA devices per user |

### organisations / stations
| Table | Purpose |
|-------|---------|
| `organisations` | Owning organisation (v1: one row) |
| `stations` | Maintenance stations / bases |
| `station_capabilities` | Approved work categories per station |

### aircraft
| Table | Purpose |
|-------|---------|
| `aircraft_types` | Aircraft type definitions (A320, B737, ...) |
| `aircraft` | Registered aircraft (tail number, MSN, ...) |
| `aircraft_configurations` | Config variants per aircraft |
| `aircraft_counters` | Current FH, FC, landing counts |
| `aircraft_counter_history` | Counter update log |
| `aircraft_status_history` | AOG, serviceable, in-maintenance status log |
| `components` | Component type master (engine, APU, LG, ...) |
| `installed_components` | Component instances installed on aircraft |

### defects
| Table | Purpose |
|-------|---------|
| `defects` | Defect / discrepancy records |
| `defect_actions` | Actions taken against a defect |
| `defect_attachments` | Photos and documents |
| `defer_references` | MEL/CDL/NEF deferral data |
| `recurring_defect_markers` | Flags for repeat defects |

### workorders
| Table | Purpose |
|-------|---------|
| `work_orders` | Work order master |
| `work_order_tasks` | Individual task cards within a WO |
| `work_order_assignments` | Personnel assigned to WO/task |
| `labour_entries` | Time booked against tasks |
| `required_skills` | Skills needed for task |
| `required_parts` | Parts needed for task |
| `required_tools` | Tools needed for task |
| `work_order_blockers` | Active safety/compliance blockers |

### workpackages
| Table | Purpose |
|-------|---------|
| `due_items` | Maintenance due list (from counters + program) |
| `work_packages` | Grouped WOs for a planned input |
| `package_items` | WOs included in a package |
| `package_readiness_snapshots` | Readiness score history |

### maintenance_data
| Table | Purpose |
|-------|---------|
| `maintenance_documents` | Document master (AMM, CMM, SB, AD, ...) |
| `document_revisions` | Revision history per document |
| `document_effectivities` | Aircraft/config applicability |
| `task_document_links` | Task ↔ required document |
| `document_acknowledgements` | User acknowledgement of revision change |

### personnel
| Table | Purpose |
|-------|---------|
| `employees` | Employee master |
| `employee_roles` | System roles per employee |
| `licences` | EASA Part-66 / FAA A&P licences |
| `training_records` | Training completion records |
| `competence_assessments` | OJT / competence sign-offs |
| `roster_shifts` | Shift schedule |

### authorisations
| Table | Purpose |
|-------|---------|
| `authorisations` | Certifying/support staff authorisations |
| `authorisation_scopes` | Aircraft types, categories per authorisation |
| `authorisation_history` | Amendments and renewals |

### inventory
| Table | Purpose |
|-------|---------|
| `parts` | Part number master |
| `alternate_parts` | Approved interchangeable PNs |
| `stock_items` | Individual stock records (serialised or batch) |
| `stock_balances` | Current on-hand per bin |
| `bin_locations` | Physical storage locations |
| `serial_traces` | Serial number movement history |
| `batch_traces` | Batch number tracking |
| `material_reservations` | Parts reserved to WO/task |
| `material_issues` | Parts issued to WO |
| `material_returns` | Parts returned from WO |
| `quarantine_records` | Quarantine log |

### tooling
| Table | Purpose |
|-------|---------|
| `tools` | Tool master register |
| `tool_kits` | Kit groupings |
| `tool_assignments` | Tool assigned to WO/task |
| `calibration_records` | Calibration history |
| `calibration_dues` | Next calibration due date |
| `tool_events` | Out-of-service, damage, loss events |

### release
| Table | Purpose |
|-------|---------|
| `inspections` | Inspection records |
| `release_certificates` | CRS / maintenance release |
| `signature_events` | Cryptographic signature events |
| `release_limitations` | Any limitations noted on release |

### records
| Table | Purpose |
|-------|---------|
| `maintenance_records` | Closed record summaries |
| `archived_records` | Long-term archive (WORM) |
| `record_retention_policies` | Retention rules by type |
| `legal_holds` | Records on legal hold |
| `backup_verification_logs` | Daily backup check results |

### audit
| Table | Purpose |
|-------|---------|
| `audit_events` | Immutable audit log (append-only) |

### occurrence
| Table | Purpose |
|-------|---------|
| `occurrence_reports` | Occurrence report records |
| `occurrence_classifications` | Severity / type classification |
| `report_submission_logs` | External submission tracking |

### quality
| Table | Purpose |
|-------|---------|
| `quality_findings` | Internal findings |
| `corrective_actions` | CAPA records |
| `root_causes` | Root cause analysis |
| `audit_plans` | Internal audit schedule |
| `audit_evidence` | Evidence per audit item |

### notifications
| Table | Purpose |
|-------|---------|
| `notifications` | Notification queue |
| `notification_preferences` | User alert preferences |

---

## Common Columns (all tables)

| Column | Type | Description |
|--------|------|-------------|
| `id` | UUID | Primary key (gen_random_uuid()) |
| `created_at` | TIMESTAMPTZ | Row creation timestamp |
| `updated_at` | TIMESTAMPTZ | Last update timestamp (trigger-maintained) |
| `created_by` | UUID | User who created the row |
| `organisation_id` | UUID | Owning organisation (for future multi-tenancy) |

---

## Key Design Decisions

- UUID primary keys throughout (not serial integers)
- `organisation_id` on every table — multi-tenancy ready from day one
- `updated_at` maintained by PostgreSQL trigger, not application code
- Soft delete (`deleted_at`) only where legally or operationally required; hard delete avoided
- All status columns are backed by CHECK constraints matching the defined state machine values
