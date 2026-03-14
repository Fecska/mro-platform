# API Standards

**Version:** 1.0
**Date:** 2026-03-14

---

## Base URL

```
/api/v1/{resource}
```

All API responses are JSON. All requests with a body use `Content-Type: application/json`.

---

## Authentication

Every request must include a valid JWT in the Authorization header:

```
Authorization: Bearer {token}
```

Unauthenticated requests receive `401 Unauthorized`.
Authorised but forbidden requests receive `403 Forbidden` with an error code explaining which permission or scope is missing.

---

## HTTP Methods

| Method | Use |
|--------|-----|
| GET | Read (list or single record) |
| POST | Create |
| PATCH | Partial update (only fields provided are changed) |
| PUT | Full replacement (used sparingly) |
| DELETE | Soft delete or deactivate |

State transitions (e.g. issue a work order, release a certificate) are **not** done via PATCH status.
They use explicit action endpoints:

```
POST /api/v1/work-orders/{id}/issue
POST /api/v1/work-orders/{id}/start
POST /api/v1/release-certificates/{id}/sign
```

This makes compliance-critical actions explicit, auditable, and individually permissioned.

---

## URL Structure

```
/api/v1/aircraft                          List all aircraft
/api/v1/aircraft/{id}                     Single aircraft
/api/v1/aircraft/{id}/counters            Counters for an aircraft
/api/v1/aircraft/{id}/components          Installed components
/api/v1/work-orders                       List work orders
/api/v1/work-orders/{id}                  Single work order
/api/v1/work-orders/{id}/tasks            Tasks for a work order
/api/v1/work-orders/{id}/tasks/{taskId}/sign-off    Sign off a task
/api/v1/work-orders/{id}/issue            Issue the work order
/api/v1/release-certificates/{id}/sign   Sign a release
```

---

## Response Format

### Success — single resource

```json
{
  "data": {
    "id": "uuid",
    "status": "issued",
    ...
  }
}
```

### Success — list

```json
{
  "data": [...],
  "meta": {
    "total": 243,
    "page": 1,
    "page_size": 25
  }
}
```

### Error

```json
{
  "error": {
    "code": "AUTHORISATION_EXPIRED",
    "message": "Your authorisation (ref: AUTH-2024-0012) expired on 2026-01-01.",
    "field": null
  }
}
```

### Validation error

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more fields are invalid.",
    "fields": [
      { "field": "quantity", "message": "Must be greater than 0." },
      { "field": "part_no", "message": "Required." }
    ]
  }
}
```

---

## Error Codes

| Code | HTTP | Description |
|------|------|-------------|
| `UNAUTHORISED` | 401 | No valid token |
| `FORBIDDEN` | 403 | Token valid but missing permission or scope |
| `NOT_FOUND` | 404 | Resource not found or not visible to this user |
| `VALIDATION_ERROR` | 422 | Input validation failed |
| `HARD_STOP` | 409 | A compliance hard stop prevented the action |
| `STATE_TRANSITION_INVALID` | 409 | Requested state transition is not allowed from current state |
| `CONFLICT` | 409 | Concurrent modification conflict |
| `INTERNAL_ERROR` | 500 | Unexpected server error |

Hard stops always return `HARD_STOP` with the specific hard stop rule ID in the message:

```json
{
  "error": {
    "code": "HARD_STOP",
    "message": "HS-001: Release blocked. Authorisation AUTH-2024-0012 expired on 2026-01-01.",
    "hard_stop_rule": "HS-001"
  }
}
```

---

## Pagination

List endpoints support:

```
GET /api/v1/work-orders?page=2&page_size=25
```

Default page size: 25. Maximum: 100.

---

## Filtering and Sorting

```
GET /api/v1/work-orders?status=in_progress&aircraft_id={uuid}
GET /api/v1/defects?aircraft_id={uuid}&open=true
GET /api/v1/stock-items?part_no=PN1234&location=BIN-A1
GET /api/v1/work-orders?sort=created_at:desc
```

---

## Versioning

API is versioned via URL prefix (`/api/v1/`). Breaking changes require a new version. Old versions are deprecated with a minimum 6-month notice.

---

## Audit

Every state-changing API call (POST, PATCH, PUT, DELETE, action endpoints) automatically produces an audit event. This is handled by middleware, not individually in each controller.

---

## Rate Limiting

- Standard users: 300 requests / minute
- Mobile sync: 600 requests / minute
- Background worker: 1000 requests / minute
- Rate limit exceeded: `429 Too Many Requests`
