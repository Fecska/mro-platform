-- Migration 014: Tools (GSE / special tools) and calibration records

-- ============================================================
-- TABLE: tools
-- ============================================================
CREATE TABLE IF NOT EXISTS tools (
    id                          UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id             UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    tool_number                 TEXT        NOT NULL,
    description                 TEXT        NOT NULL,
    category                    TEXT        NOT NULL,
        -- e.g. 'measuring' | 'torque' | 'gse' | 'special' | 'test_equipment'
    status                      TEXT        NOT NULL DEFAULT 'available',
        -- available | checked_out | in_calibration | quarantine | lost | retired
    serial_number               TEXT,
    location                    TEXT,
    calibration_required        BOOLEAN     NOT NULL DEFAULT FALSE,
    next_calibration_due        TIMESTAMPTZ,
    checked_out_to_task_id      UUID                 REFERENCES work_order_tasks (id) ON DELETE RESTRICT,
    checked_out_by              UUID,
    checked_out_at              TIMESTAMPTZ,
    created_at                  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by                  UUID,
    updated_at                  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by                  UUID,
    deleted_at                  TIMESTAMPTZ,
    deleted_by                  UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_tools_org_toolnum
    ON tools (organisation_id, tool_number)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_tools_organisation_id        ON tools (organisation_id);
CREATE INDEX IF NOT EXISTS ix_tools_status                 ON tools (status);
CREATE INDEX IF NOT EXISTS ix_tools_category               ON tools (category);
CREATE INDEX IF NOT EXISTS ix_tools_calibration_required   ON tools (calibration_required) WHERE calibration_required = TRUE;
CREATE INDEX IF NOT EXISTS ix_tools_next_calibration_due   ON tools (next_calibration_due);
CREATE INDEX IF NOT EXISTS ix_tools_checked_out_to_task_id ON tools (checked_out_to_task_id);
CREATE INDEX IF NOT EXISTS ix_tools_deleted_at             ON tools (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_tools_updated_at') THEN
        CREATE TRIGGER trg_tools_updated_at
            BEFORE UPDATE ON tools
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: calibration_records
-- Append-only history of calibration events per tool
-- ============================================================
CREATE TABLE IF NOT EXISTS calibration_records (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    tool_id             UUID        NOT NULL REFERENCES tools (id) ON DELETE RESTRICT,
    calibrated_by       TEXT        NOT NULL,   -- name / lab identifier
    calibrated_at       TIMESTAMPTZ NOT NULL,
    calibration_due     TIMESTAMPTZ NOT NULL,
    certificate_ref     TEXT,
    notes               TEXT,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID
);

CREATE INDEX IF NOT EXISTS ix_calibration_records_tool_id        ON calibration_records (tool_id);
CREATE INDEX IF NOT EXISTS ix_calibration_records_calibrated_at  ON calibration_records (calibrated_at DESC);
CREATE INDEX IF NOT EXISTS ix_calibration_records_calibration_due ON calibration_records (calibration_due);
CREATE INDEX IF NOT EXISTS ix_calibration_records_deleted_at     ON calibration_records (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_calibration_records_updated_at') THEN
        CREATE TRIGGER trg_calibration_records_updated_at
            BEFORE UPDATE ON calibration_records
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
