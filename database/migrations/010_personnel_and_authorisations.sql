-- Migration 010: Personnel (employees) and EASA Part-145 authorisations

-- ============================================================
-- TABLE: employees
-- ============================================================
CREATE TABLE IF NOT EXISTS employees (
    id                          UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id             UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    user_id                     UUID                 REFERENCES users (id) ON DELETE RESTRICT,
    first_name                  TEXT        NOT NULL,
    last_name                   TEXT        NOT NULL,
    email                       TEXT,
    date_of_birth               DATE,
    nationality_code            TEXT,
    employment_status           TEXT        NOT NULL DEFAULT 'active',
        -- active | suspended | terminated | on_leave
    emergency_contact_name      TEXT,
    emergency_contact_phone     TEXT,
    created_at                  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by                  UUID,
    updated_at                  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by                  UUID,
    deleted_at                  TIMESTAMPTZ,
    deleted_by                  UUID
);

CREATE INDEX IF NOT EXISTS ix_employees_organisation_id   ON employees (organisation_id);
CREATE INDEX IF NOT EXISTS ix_employees_user_id           ON employees (user_id);
CREATE INDEX IF NOT EXISTS ix_employees_employment_status ON employees (employment_status);
CREATE INDEX IF NOT EXISTS ix_employees_last_name         ON employees (last_name);
CREATE INDEX IF NOT EXISTS ix_employees_deleted_at        ON employees (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_employees_updated_at') THEN
        CREATE TRIGGER trg_employees_updated_at
            BEFORE UPDATE ON employees
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- Back-fill FK from task_assignments -> employees (deferred from 009)
-- ============================================================
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_task_assignments_employee_id'
    ) THEN
        ALTER TABLE task_assignments
            ADD CONSTRAINT fk_task_assignments_employee_id
            FOREIGN KEY (employee_id) REFERENCES employees (id) ON DELETE RESTRICT;
    END IF;
END;
$$;

-- ============================================================
-- TABLE: authorisations
-- EASA Part-145 certifying staff authorisations
-- ============================================================
CREATE TABLE IF NOT EXISTS authorisations (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    employee_id         UUID        NOT NULL REFERENCES employees (id) ON DELETE RESTRICT,
    category            TEXT        NOT NULL,
        -- e.g. A | B1.1 | B1.2 | B2 | C  (EASA Part-66 categories)
    aircraft_types      TEXT,       -- comma-separated ICAO type codes, or NULL = all in category
    scope               TEXT,       -- narrative scope of authorisation
    issued_at           DATE        NOT NULL,
    expires_at          DATE,
    issued_by_user_id   UUID        REFERENCES users (id) ON DELETE RESTRICT,
    revoked_at          TIMESTAMPTZ,
    revocation_reason   TEXT,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID
);

CREATE INDEX IF NOT EXISTS ix_authorisations_employee_id      ON authorisations (employee_id);
CREATE INDEX IF NOT EXISTS ix_authorisations_category         ON authorisations (category);
CREATE INDEX IF NOT EXISTS ix_authorisations_expires_at       ON authorisations (expires_at);
CREATE INDEX IF NOT EXISTS ix_authorisations_issued_by_user_id ON authorisations (issued_by_user_id);
CREATE INDEX IF NOT EXISTS ix_authorisations_deleted_at       ON authorisations (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_authorisations_updated_at') THEN
        CREATE TRIGGER trg_authorisations_updated_at
            BEFORE UPDATE ON authorisations
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
