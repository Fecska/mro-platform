-- Migration 006: Document revisions and effectivity

-- ============================================================
-- TABLE: document_revisions
-- ============================================================
CREATE TABLE IF NOT EXISTS document_revisions (
    id              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    document_id     UUID        NOT NULL REFERENCES maintenance_documents (id) ON DELETE RESTRICT,
    revision_number TEXT        NOT NULL,
    revision_date   DATE        NOT NULL,
    status          TEXT        NOT NULL DEFAULT 'draft',   -- draft | active | superseded | cancelled
    approved_by     UUID,
    approved_at     TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by      UUID,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID
);

-- Only one active revision per document at a time is enforced at the application layer,
-- but we index heavily to support fast lookups.
CREATE UNIQUE INDEX IF NOT EXISTS uix_document_revisions_doc_revnum
    ON document_revisions (document_id, revision_number)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_document_revisions_document_id   ON document_revisions (document_id);
CREATE INDEX IF NOT EXISTS ix_document_revisions_status        ON document_revisions (status);
CREATE INDEX IF NOT EXISTS ix_document_revisions_revision_date ON document_revisions (revision_date DESC);
CREATE INDEX IF NOT EXISTS ix_document_revisions_approved_by   ON document_revisions (approved_by);
CREATE INDEX IF NOT EXISTS ix_document_revisions_deleted_at    ON document_revisions (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_document_revisions_updated_at') THEN
        CREATE TRIGGER trg_document_revisions_updated_at
            BEFORE UPDATE ON document_revisions
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: document_effectivity
-- Defines which aircraft (by registration, type, or MSN range)
-- a document or revision applies to.
-- ============================================================
CREATE TABLE IF NOT EXISTS document_effectivity (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    document_id         UUID        NOT NULL REFERENCES maintenance_documents (id) ON DELETE RESTRICT,
    aircraft_id         UUID                 REFERENCES aircraft (id) ON DELETE RESTRICT,
    aircraft_type_code  TEXT,
    msn_from            TEXT,
    msn_to              TEXT,
    is_active           BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID
);

CREATE INDEX IF NOT EXISTS ix_document_effectivity_document_id        ON document_effectivity (document_id);
CREATE INDEX IF NOT EXISTS ix_document_effectivity_aircraft_id        ON document_effectivity (aircraft_id);
CREATE INDEX IF NOT EXISTS ix_document_effectivity_aircraft_type_code ON document_effectivity (aircraft_type_code);
CREATE INDEX IF NOT EXISTS ix_document_effectivity_is_active          ON document_effectivity (is_active);
CREATE INDEX IF NOT EXISTS ix_document_effectivity_deleted_at         ON document_effectivity (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_document_effectivity_updated_at') THEN
        CREATE TRIGGER trg_document_effectivity_updated_at
            BEFORE UPDATE ON document_effectivity
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
