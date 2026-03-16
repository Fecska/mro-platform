-- Migration 017: Attachments — generic file references for any entity

-- ============================================================
-- TABLE: attachments
-- Stores metadata for files uploaded to object storage.
-- entity_type + entity_id form a polymorphic association that
-- can reference any table (work_orders, defects, inspections, etc.).
-- The actual file bytes live in the configured object store;
-- storage_key is the opaque key (e.g. S3/Azure Blob path).
-- ============================================================
CREATE TABLE IF NOT EXISTS attachments (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id     UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    entity_type         TEXT        NOT NULL,   -- e.g. 'work_order' | 'defect' | 'inspection' | 'release_certificate'
    entity_id           UUID        NOT NULL,
    file_name           TEXT        NOT NULL,
    content_type        TEXT        NOT NULL,   -- MIME type, e.g. 'application/pdf'
    storage_key         TEXT        NOT NULL,   -- unique path within the object store bucket
    file_size_bytes     BIGINT,
    uploaded_by         UUID        NOT NULL    REFERENCES users (id) ON DELETE RESTRICT,
    uploaded_at         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID,
    -- Standard audit columns
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID
);

-- Primary access pattern: fetch all attachments for a given entity
CREATE INDEX IF NOT EXISTS ix_attachments_entity ON attachments (entity_type, entity_id);

CREATE INDEX IF NOT EXISTS ix_attachments_organisation_id ON attachments (organisation_id);
CREATE INDEX IF NOT EXISTS ix_attachments_uploaded_by     ON attachments (uploaded_by);
CREATE INDEX IF NOT EXISTS ix_attachments_uploaded_at     ON attachments (uploaded_at DESC);
CREATE INDEX IF NOT EXISTS ix_attachments_deleted_at      ON attachments (deleted_at) WHERE deleted_at IS NULL;

-- Enforce globally unique storage keys so no two records point to the same blob
CREATE UNIQUE INDEX IF NOT EXISTS uix_attachments_storage_key
    ON attachments (storage_key)
    WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_attachments_updated_at') THEN
        CREATE TRIGGER trg_attachments_updated_at
            BEFORE UPDATE ON attachments
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
