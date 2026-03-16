-- Migration 005: Maintenance documents — categories, AMM, IPC, SRM, MEL, etc.

-- ============================================================
-- TABLE: document_categories
-- Lookup table for document type groupings (AMM, IPC, SB, AD…).
-- ============================================================
CREATE TABLE IF NOT EXISTS document_categories (
    id              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    code            TEXT        NOT NULL,   -- AMM | IPC | SRM | MEL | CDL | SB | AD | EO | WDM | CMM
    name            TEXT        NOT NULL,
    description     TEXT,
    requires_approval   BOOLEAN NOT NULL DEFAULT FALSE,
    is_regulatory       BOOLEAN NOT NULL DEFAULT FALSE,
    is_active           BOOLEAN NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by      UUID,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID,
    CONSTRAINT uq_document_categories_code UNIQUE (code)
);

CREATE INDEX IF NOT EXISTS ix_document_categories_is_active  ON document_categories (is_active);
CREATE INDEX IF NOT EXISTS ix_document_categories_deleted_at ON document_categories (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_document_categories_updated_at') THEN
        CREATE TRIGGER trg_document_categories_updated_at
            BEFORE UPDATE ON document_categories
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- Seed standard document categories (idempotent)
INSERT INTO document_categories (code, name, description, requires_approval, is_regulatory) VALUES
    ('AMM',  'Aircraft Maintenance Manual',       'Manufacturer task procedures',               FALSE, FALSE),
    ('IPC',  'Illustrated Parts Catalog',         'Part numbers and exploded views',             FALSE, FALSE),
    ('SRM',  'Structural Repair Manual',          'Approved structural repair schemes',          TRUE,  FALSE),
    ('MEL',  'Minimum Equipment List',            'Approved dispatch with inoperative items',    TRUE,  TRUE ),
    ('CDL',  'Configuration Deviation List',      'Approved dispatch with missing parts',        TRUE,  TRUE ),
    ('SB',   'Service Bulletin',                  'Manufacturer recommended modification',       FALSE, FALSE),
    ('AD',   'Airworthiness Directive',           'Mandatory regulatory action',                 TRUE,  TRUE ),
    ('EO',   'Engineering Order',                 'Internal approved modification',              TRUE,  FALSE),
    ('WDM',  'Wiring Diagram Manual',             'Aircraft wiring schematics',                  FALSE, FALSE),
    ('CMM',  'Component Maintenance Manual',      'Component-level overhaul procedures',         FALSE, FALSE),
    ('AWL',  'Airworthiness Limitations',         'Mandatory life limits and intervals',         TRUE,  TRUE )
ON CONFLICT (code) DO NOTHING;

-- ============================================================
-- TABLE: maintenance_documents
-- ============================================================
CREATE TABLE IF NOT EXISTS maintenance_documents (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id     UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    category_id         UUID        REFERENCES document_categories (id) ON DELETE RESTRICT,
    document_number     TEXT        NOT NULL,
    document_type       TEXT        NOT NULL,  -- mirrors document_categories.code for denormalised filtering
    title               TEXT        NOT NULL,
    aircraft_type_code  TEXT,
    is_active           BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID
);

-- A document number must be unique within an organisation
CREATE UNIQUE INDEX IF NOT EXISTS uix_maintenance_documents_org_docnum
    ON maintenance_documents (organisation_id, document_number)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_maintenance_documents_organisation_id  ON maintenance_documents (organisation_id);
CREATE INDEX IF NOT EXISTS ix_maintenance_documents_category_id      ON maintenance_documents (category_id);
CREATE INDEX IF NOT EXISTS ix_maintenance_documents_document_type    ON maintenance_documents (document_type);
CREATE INDEX IF NOT EXISTS ix_maintenance_documents_aircraft_type    ON maintenance_documents (aircraft_type_code);
CREATE INDEX IF NOT EXISTS ix_maintenance_documents_is_active        ON maintenance_documents (is_active);
CREATE INDEX IF NOT EXISTS ix_maintenance_documents_deleted_at       ON maintenance_documents (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_maintenance_documents_updated_at') THEN
        CREATE TRIGGER trg_maintenance_documents_updated_at
            BEFORE UPDATE ON maintenance_documents
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
