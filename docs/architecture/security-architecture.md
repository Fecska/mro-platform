# Security Architecture

**Version:** 1.0
**Date:** 2026-03-14

---

## Security Layers

```
Internet / Client
       │
  [TLS 1.2+]
       │
  [API Gateway / Reverse Proxy]
       │
  [Authentication — JWT / OIDC]
       │
  [RBAC + Scope enforcement]
       │
  [Application logic + hard stops]
       │
  [Database — row-level security where applicable]
       │
  [Encryption at rest]
       │
  [Audit log — append only]
```

---

## Authentication

- All API endpoints require a valid JWT (except health check)
- JWT issued by Keycloak or internal OIDC service
- Token contains: `user_id`, `organisation_id`, `roles[]`, `station_ids[]`
- Token lifetime: 15 minutes access token, 8 hours refresh
- MFA required for: `release`, `admin`, `quality`, `certifying_staff` roles
- Sessions invalidatable server-side (token blocklist in Redis)

---

## Authorisation — RBAC + Operational Scope

Standard RBAC is not sufficient. The system uses **role + scope** authorisation.

### Roles

| Role | Description |
|------|-------------|
| `mechanic` | Execute tasks, view work orders |
| `certifying_staff` | Sign off tasks, issue release (within authorisation scope) |
| `inspector` | Perform and sign inspections |
| `stores` | Issue, return, and manage inventory |
| `planner` | Create and manage work orders and packages |
| `maintenance_control` | Full view, manage deferrals and AOG |
| `quality` | Quality findings, CAPA, audits, override safety blocks |
| `admin` | User management, system configuration |
| `readonly` | View only, no write access |

### Operational Scope

In addition to role, access is scoped to:

| Scope | Description |
|-------|-------------|
| `station` | Employee can only work at assigned station(s) |
| `aircraft_type` | Certifying staff authorised only for specific types |
| `release_category` | What categories of work a certifying staff member can sign |
| `document_access` | Can only see documents linked to their station/type |

Permission check = role permission AND scope check. Both must pass.

---

## Audit Log

The audit log is the security backbone of the system.

### What is logged

| Event type | Examples |
|------------|---------|
| Authentication | Login, logout, failed login, MFA success/fail |
| Data modification | Create, update, delete on any entity |
| Compliance action | Release issued, inspection signed, authorisation granted |
| Security event | Permission denied, role change, session invalidated |
| Privileged action | Safety block override, record hard-delete |
| Integration | External API call in/out |

### Immutability rules

- Audit events are insert-only — no update or delete permitted
- Audit table has no `UPDATE` or `DELETE` grants, even for admin
- Each event stores: `actor_id`, `action`, `entity_type`, `entity_id`, `timestamp`, `ip`, `user_agent`, `old_value`, `new_value`, `context`
- Events are checksummed on write and verified periodically

---

## Encryption

| Data | Encryption |
|------|-----------|
| In transit | TLS 1.2+ (minimum), TLS 1.3 preferred |
| Database at rest | Provider-level AES-256 (managed DB) or OS-level encryption |
| Documents/attachments | AES-256 on S3-compatible storage |
| Secrets | Vault or environment secrets — never stored in code or DB |
| Signatures | Cryptographic signature reference stored; key managed in vault |

---

## Secrets Management

- No secrets in source code, ever
- No secrets in database
- `.env.example` contains only placeholder values
- Production secrets: environment variables injected at deploy time or via secret manager
- Signing keys: separate vault, rotatable

---

## Electronic Signature Security

Release to Service certificates use electronic signatures. Requirements:

- Signer identity verified at sign time (re-authentication or MFA step)
- Signature event records: `signer_id`, `timestamp`, `authorisation_ref`, `document_hash`, `ip`
- Signatures are non-repudiable (cannot be deleted)
- Signature can be independently verified via `signature_event.certificate_reference`
- Revocation of a release creates a new `revocation_event`, does not delete the original

---

## Dependency and Supply Chain Security

- All dependencies must be pinned to specific versions
- Dependency audit run in CI on every build
- SBOM (Software Bill of Materials) generated on release
- No unreviewed direct dependencies added without pull request review

---

## Incident Response

Runbook stored in `docs/operations/incident-response.md` (to be created).

Key contacts, escalation path, and breach notification procedure to be defined before go-live.

---

## Compliance References

- EASA Part-IS (Information Security for aviation organisations)
- EASA AMC/GM continuing airworthiness — cyber incident management
- GDPR (if personal data of EU-based employees is stored)
- FAA AC 120-78B (electronic signatures and records)
