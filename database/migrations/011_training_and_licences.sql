-- Migration 011: Licences, training records, and shifts

-- ============================================================
-- TABLE: licences
-- EASA Part-66 / national AME licences held by employees
-- ============================================================
CREATE TABLE IF NOT EXISTS licences (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    employee_id         UUID        NOT NULL REFERENCES employees (id) ON DELETE RESTRICT,
    licence_type        TEXT        NOT NULL,
        -- e.g. 'EASA_PART66' | 'FAA_A&P' | 'TCCA_AME' | 'national'
    licence_number      TEXT        NOT NULL,
    issuing_authority   TEXT,
    issued_at           DATE        NOT NULL,
    expires_at          DATE,
    is_active           BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_licences_employee_licence_num
    ON licences (employee_id, licence_number)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_licences_employee_id    ON licences (employee_id);
CREATE INDEX IF NOT EXISTS ix_licences_licence_type   ON licences (licence_type);
CREATE INDEX IF NOT EXISTS ix_licences_expires_at     ON licences (expires_at);
CREATE INDEX IF NOT EXISTS ix_licences_is_active      ON licences (is_active);
CREATE INDEX IF NOT EXISTS ix_licences_deleted_at     ON licences (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_licences_updated_at') THEN
        CREATE TRIGGER trg_licences_updated_at
            BEFORE UPDATE ON licences
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: training_records
-- ============================================================
CREATE TABLE IF NOT EXISTS training_records (
    id              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    employee_id     UUID        NOT NULL REFERENCES employees (id) ON DELETE RESTRICT,
    course_name     TEXT        NOT NULL,
    provider        TEXT,
    completed_at    DATE        NOT NULL,
    expires_at      DATE,
    certificate_ref TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by      UUID,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID
);

CREATE INDEX IF NOT EXISTS ix_training_records_employee_id  ON training_records (employee_id);
CREATE INDEX IF NOT EXISTS ix_training_records_completed_at ON training_records (completed_at DESC);
CREATE INDEX IF NOT EXISTS ix_training_records_expires_at   ON training_records (expires_at);
CREATE INDEX IF NOT EXISTS ix_training_records_deleted_at   ON training_records (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_training_records_updated_at') THEN
        CREATE TRIGGER trg_training_records_updated_at
            BEFORE UPDATE ON training_records
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: shifts
-- Labour time tracking per employee, optionally linked to a work order
-- ============================================================
CREATE TABLE IF NOT EXISTS shifts (
    id              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    employee_id     UUID        NOT NULL REFERENCES employees (id) ON DELETE RESTRICT,
    work_order_id   UUID                 REFERENCES work_orders (id) ON DELETE RESTRICT,
    shift_date      DATE        NOT NULL,
    start_time      TIMETZ,
    end_time        TIMETZ,
    break_minutes   INT         NOT NULL DEFAULT 0,
    notes           TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by      UUID,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID
);

CREATE INDEX IF NOT EXISTS ix_shifts_employee_id   ON shifts (employee_id);
CREATE INDEX IF NOT EXISTS ix_shifts_work_order_id ON shifts (work_order_id);
CREATE INDEX IF NOT EXISTS ix_shifts_shift_date    ON shifts (shift_date DESC);
CREATE INDEX IF NOT EXISTS ix_shifts_deleted_at    ON shifts (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_shifts_updated_at') THEN
        CREATE TRIGGER trg_shifts_updated_at
            BEFORE UPDATE ON shifts
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
