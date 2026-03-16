-- Migration 018: Audit events — immutable compliance and security event log
-- NOTE: audit_events rows are NEVER updated or soft-deleted.
--       There are no updated_at / deleted_at columns by design.
--       The table is append-only; use partitioning for long-term retention.

-- ============================================================
-- TABLE: audit_events
-- ============================================================
CREATE TABLE IF NOT EXISTS audit_events (
    id              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id UUID,                   -- NULL for system-level events (e.g. tenant creation)
    actor_id        UUID,                   -- NULL for automated / system-generated events
    event_type      TEXT        NOT NULL,
        -- Examples:
        --   auth.login_success | auth.login_failed | auth.logout
        --   workorder.created | workorder.status_changed | workorder.closed
        --   release.signed | release.voided
        --   inventory.issued | tools.checked_out | tools.checked_in
        --   user.created | user.role_granted | user.role_revoked
        --   aircraft.updated | due_item.overdue_detected
    entity_type     TEXT        NOT NULL,   -- table/aggregate name, e.g. 'work_orders'
    entity_id       UUID,                   -- PK of the affected row (NULL for bulk/system events)
    ip_address      TEXT,
    description     TEXT        NOT NULL,
    old_data        JSONB,                  -- previous state snapshot (redacted for PII if needed)
    new_data        JSONB,                  -- new state snapshot
    occurred_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Time-series queries within an organisation
CREATE INDEX IF NOT EXISTS ix_audit_events_org_occurred
    ON audit_events (organisation_id, occurred_at DESC);

-- Entity-specific history (e.g. "show me all events for WO-12345")
CREATE INDEX IF NOT EXISTS ix_audit_events_entity
    ON audit_events (entity_type, entity_id);

-- Actor activity log
CREATE INDEX IF NOT EXISTS ix_audit_events_actor_id
    ON audit_events (actor_id);

-- Event type filtering (e.g. "all failed logins")
CREATE INDEX IF NOT EXISTS ix_audit_events_event_type
    ON audit_events (event_type);

-- GIN index for full JSONB payload searches
CREATE INDEX IF NOT EXISTS ix_audit_events_new_data_gin
    ON audit_events USING GIN (new_data);

CREATE INDEX IF NOT EXISTS ix_audit_events_old_data_gin
    ON audit_events USING GIN (old_data);
