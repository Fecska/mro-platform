-- Migration 007: Defects — findings, deferral categories (MEL / CDL / NEF)

-- ============================================================
-- TABLE: defects
-- ============================================================
CREATE TABLE IF NOT EXISTS defects (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id     UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    aircraft_id         UUID        NOT NULL REFERENCES aircraft (id) ON DELETE RESTRICT,
    defect_number       TEXT        NOT NULL,
    title               TEXT        NOT NULL,
    description         TEXT,
    status              TEXT        NOT NULL DEFAULT 'open',
        -- open | deferred | in_work | closed | cancelled
    deferral_category   TEXT,
        -- MEL | CDL | NEF | nil (null = not deferred)
    mel_ref             TEXT,
    cdl_ref             TEXT,
    raised_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    raised_by           UUID,
    deferred_until      DATE,
    closed_at           TIMESTAMPTZ,
    closed_by           UUID,
    work_order_id       UUID,       -- soft reference; FK enforced in 008 via ALTER
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_defects_org_defectnum
    ON defects (organisation_id, defect_number)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_defects_organisation_id   ON defects (organisation_id);
CREATE INDEX IF NOT EXISTS ix_defects_aircraft_id       ON defects (aircraft_id);
CREATE INDEX IF NOT EXISTS ix_defects_status            ON defects (status);
CREATE INDEX IF NOT EXISTS ix_defects_deferral_category ON defects (deferral_category);
CREATE INDEX IF NOT EXISTS ix_defects_raised_at         ON defects (raised_at DESC);
CREATE INDEX IF NOT EXISTS ix_defects_deferred_until    ON defects (deferred_until);
CREATE INDEX IF NOT EXISTS ix_defects_work_order_id     ON defects (work_order_id);
CREATE INDEX IF NOT EXISTS ix_defects_deleted_at        ON defects (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_defects_updated_at') THEN
        CREATE TRIGGER trg_defects_updated_at
            BEFORE UPDATE ON defects
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
