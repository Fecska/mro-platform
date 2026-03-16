-- Migration 019: Notifications — in-app and push/email notification records

-- ============================================================
-- TABLE: notifications
-- ============================================================
CREATE TABLE IF NOT EXISTS notifications (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id     UUID                 REFERENCES organisations (id) ON DELETE RESTRICT,
    recipient_user_id   UUID        NOT NULL REFERENCES users (id) ON DELETE RESTRICT,
    title               TEXT        NOT NULL,
    body                TEXT,
    notification_type   TEXT        NOT NULL,
        -- e.g. 'workorder_assigned' | 'due_item_overdue' | 'release_signed'
        --      'tool_calibration_due' | 'licence_expiring' | 'defect_deferred'
        --      'inspection_required' | 'system_alert'
    entity_type         TEXT,       -- optional: links back to a related record
    entity_id           UUID,
    is_read             BOOLEAN     NOT NULL DEFAULT FALSE,
    read_at             TIMESTAMPTZ,
    channel             TEXT        NOT NULL DEFAULT 'in_app',
        -- in_app | email | push | sms
    sent_at             TIMESTAMPTZ,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID
);

-- Primary query: unread notifications for a user (inbox badge / listing)
CREATE INDEX IF NOT EXISTS ix_notifications_recipient_is_read
    ON notifications (recipient_user_id, is_read);

CREATE INDEX IF NOT EXISTS ix_notifications_organisation_id  ON notifications (organisation_id);
CREATE INDEX IF NOT EXISTS ix_notifications_recipient_user_id ON notifications (recipient_user_id);
CREATE INDEX IF NOT EXISTS ix_notifications_notification_type ON notifications (notification_type);
CREATE INDEX IF NOT EXISTS ix_notifications_entity           ON notifications (entity_type, entity_id);
CREATE INDEX IF NOT EXISTS ix_notifications_sent_at          ON notifications (sent_at DESC);
CREATE INDEX IF NOT EXISTS ix_notifications_deleted_at       ON notifications (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_notifications_updated_at') THEN
        CREATE TRIGGER trg_notifications_updated_at
            BEFORE UPDATE ON notifications
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
