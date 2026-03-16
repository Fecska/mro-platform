-- Migration 016: Maintenance programs, due items, work packages, and package items

-- ============================================================
-- TABLE: maintenance_programs
-- Approved maintenance programme (AMP) definitions per aircraft type
-- ============================================================
CREATE TABLE IF NOT EXISTS maintenance_programs (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id     UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    program_number      TEXT        NOT NULL,
    aircraft_type_code  TEXT        NOT NULL,
    title               TEXT        NOT NULL,
    revision_number     TEXT        NOT NULL,
    revision_date       DATE        NOT NULL,
    approval_reference  TEXT,
    is_active           BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_maintenance_programs_org_prognum
    ON maintenance_programs (organisation_id, program_number)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_maintenance_programs_organisation_id  ON maintenance_programs (organisation_id);
CREATE INDEX IF NOT EXISTS ix_maintenance_programs_aircraft_type    ON maintenance_programs (aircraft_type_code);
CREATE INDEX IF NOT EXISTS ix_maintenance_programs_is_active        ON maintenance_programs (is_active);
CREATE INDEX IF NOT EXISTS ix_maintenance_programs_deleted_at       ON maintenance_programs (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_maintenance_programs_updated_at') THEN
        CREATE TRIGGER trg_maintenance_programs_updated_at
            BEFORE UPDATE ON maintenance_programs
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: due_items
-- Individual scheduled maintenance tasks / ADs / SBs that
-- have a calculated next-due threshold.
-- ============================================================
CREATE TABLE IF NOT EXISTS due_items (
    id                                  UUID            NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id                     UUID            NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    aircraft_id                         UUID            NOT NULL REFERENCES aircraft (id) ON DELETE RESTRICT,
    due_item_ref                        TEXT            NOT NULL,
    due_item_type                       TEXT            NOT NULL,
        -- scheduled | ad | sb | eo | cdl | mel | one_time
    interval_type                       TEXT            NOT NULL,
        -- calendar | hours | cycles | calendar_or_hours | calendar_or_cycles | hours_or_cycles | triple
    description                         TEXT            NOT NULL,
    maintenance_program_id              UUID                     REFERENCES maintenance_programs (id) ON DELETE RESTRICT,
    regulatory_ref                      TEXT,
    interval_value                      NUMERIC(12,4),
    interval_days                       INT,
    tolerance_value                     NUMERIC(12,4),
    next_due_date                       TIMESTAMPTZ,
    next_due_hours                      NUMERIC(10,2),
    next_due_cycles                     INT,
    last_accomplished_at                TIMESTAMPTZ,
    last_accomplished_work_order_id     UUID                     REFERENCES work_orders (id) ON DELETE RESTRICT,
    status                              TEXT            NOT NULL DEFAULT 'current',
        -- current | overdue | due_soon | accomplished | deferred | n_a | cancelled
    created_at                          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    created_by                          UUID,
    updated_at                          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_by                          UUID,
    deleted_at                          TIMESTAMPTZ,
    deleted_by                          UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_due_items_org_ref_aircraft
    ON due_items (organisation_id, due_item_ref, aircraft_id)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_due_items_organisation_id                 ON due_items (organisation_id);
CREATE INDEX IF NOT EXISTS ix_due_items_aircraft_id                     ON due_items (aircraft_id);
CREATE INDEX IF NOT EXISTS ix_due_items_status                          ON due_items (status);
CREATE INDEX IF NOT EXISTS ix_due_items_due_item_type                   ON due_items (due_item_type);
CREATE INDEX IF NOT EXISTS ix_due_items_next_due_date                   ON due_items (next_due_date);
CREATE INDEX IF NOT EXISTS ix_due_items_next_due_hours                  ON due_items (next_due_hours);
CREATE INDEX IF NOT EXISTS ix_due_items_maintenance_program_id          ON due_items (maintenance_program_id);
CREATE INDEX IF NOT EXISTS ix_due_items_last_accomplished_work_order_id ON due_items (last_accomplished_work_order_id);
CREATE INDEX IF NOT EXISTS ix_due_items_deleted_at                      ON due_items (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_due_items_updated_at') THEN
        CREATE TRIGGER trg_due_items_updated_at
            BEFORE UPDATE ON due_items
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: work_packages
-- Planning-level containers grouping multiple work orders
-- for a base maintenance event (e.g. C-check, annual)
-- ============================================================
CREATE TABLE IF NOT EXISTS work_packages (
    id                      UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id         UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    aircraft_id             UUID        NOT NULL REFERENCES aircraft (id) ON DELETE RESTRICT,
    package_number          TEXT        NOT NULL,
    description             TEXT        NOT NULL,
    status                  TEXT        NOT NULL DEFAULT 'draft',
        -- draft | planned | open | in_progress | completed | closed | cancelled
    station_id              UUID                 REFERENCES stations (id) ON DELETE RESTRICT,
    related_work_order_id   UUID                 REFERENCES work_orders (id) ON DELETE RESTRICT,
    planned_start_date      DATE        NOT NULL,
    planned_end_date        DATE,
    actual_start_date       DATE,
    actual_end_date         DATE,
    released_at             TIMESTAMPTZ,
    completed_at            TIMESTAMPTZ,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by              UUID,
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by              UUID,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_work_packages_org_pkgnum
    ON work_packages (organisation_id, package_number)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_work_packages_organisation_id     ON work_packages (organisation_id);
CREATE INDEX IF NOT EXISTS ix_work_packages_aircraft_id         ON work_packages (aircraft_id);
CREATE INDEX IF NOT EXISTS ix_work_packages_status              ON work_packages (status);
CREATE INDEX IF NOT EXISTS ix_work_packages_station_id          ON work_packages (station_id);
CREATE INDEX IF NOT EXISTS ix_work_packages_planned_start_date  ON work_packages (planned_start_date);
CREATE INDEX IF NOT EXISTS ix_work_packages_deleted_at          ON work_packages (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_work_packages_updated_at') THEN
        CREATE TRIGGER trg_work_packages_updated_at
            BEFORE UPDATE ON work_packages
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- Back-fill FK from work_orders -> work_packages (deferred from 008)
-- ============================================================
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_work_orders_work_package_id'
    ) THEN
        ALTER TABLE work_orders
            ADD CONSTRAINT fk_work_orders_work_package_id
            FOREIGN KEY (work_package_id) REFERENCES work_packages (id) ON DELETE RESTRICT;
    END IF;
END;
$$;

-- ============================================================
-- TABLE: package_items
-- Individual scope lines within a work package
-- ============================================================
CREATE TABLE IF NOT EXISTS package_items (
    id                      UUID            NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    work_package_id         UUID            NOT NULL REFERENCES work_packages (id) ON DELETE RESTRICT,
    description             TEXT            NOT NULL,
    status                  TEXT            NOT NULL DEFAULT 'pending',
        -- pending | in_work | accomplished | deferred | n_a | cancelled
    due_item_id             UUID                     REFERENCES due_items (id) ON DELETE RESTRICT,
    task_reference          TEXT,
    estimated_man_hours     NUMERIC(8,2),
    actual_man_hours        NUMERIC(8,2),
    linked_work_order_id    UUID                     REFERENCES work_orders (id) ON DELETE RESTRICT,
    accomplished_at         TIMESTAMPTZ,
    accomplished_by         UUID,
    deferral_reason         TEXT,
    na_reason               TEXT,
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    created_by              UUID,
    updated_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_by              UUID,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID
);

CREATE INDEX IF NOT EXISTS ix_package_items_work_package_id      ON package_items (work_package_id);
CREATE INDEX IF NOT EXISTS ix_package_items_due_item_id          ON package_items (due_item_id);
CREATE INDEX IF NOT EXISTS ix_package_items_linked_work_order_id ON package_items (linked_work_order_id);
CREATE INDEX IF NOT EXISTS ix_package_items_status               ON package_items (status);
CREATE INDEX IF NOT EXISTS ix_package_items_accomplished_by      ON package_items (accomplished_by);
CREATE INDEX IF NOT EXISTS ix_package_items_deleted_at           ON package_items (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_package_items_updated_at') THEN
        CREATE TRIGGER trg_package_items_updated_at
            BEFORE UPDATE ON package_items
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
