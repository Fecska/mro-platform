-- Migration 008: Work orders — the primary maintenance execution aggregate

-- ============================================================
-- TABLE: work_orders
-- ============================================================
CREATE TABLE IF NOT EXISTS work_orders (
    id                      UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id         UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    aircraft_id             UUID        NOT NULL REFERENCES aircraft (id) ON DELETE RESTRICT,
    work_order_number       TEXT        NOT NULL,
    title                   TEXT        NOT NULL,
    description             TEXT,
    status                  TEXT        NOT NULL DEFAULT 'draft',
        -- draft | planned | in_progress | completed | closed | cancelled
    work_order_type         TEXT        NOT NULL DEFAULT 'scheduled',
        -- scheduled | unscheduled | aog | line | base | modification
    station_id              UUID                 REFERENCES stations (id) ON DELETE RESTRICT,
    planned_start_date      DATE,
    planned_end_date        DATE,
    actual_start_date       DATE,
    actual_end_date         DATE,
    released_at             TIMESTAMPTZ,
    completed_at            TIMESTAMPTZ,
    closed_at               TIMESTAMPTZ,
    cancelled_at            TIMESTAMPTZ,
    cancellation_reason     TEXT,
    work_package_id         UUID,       -- soft reference; FK enforced in 016 via ALTER
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by              UUID,
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by              UUID,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_work_orders_org_wonum
    ON work_orders (organisation_id, work_order_number)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_work_orders_organisation_id    ON work_orders (organisation_id);
CREATE INDEX IF NOT EXISTS ix_work_orders_aircraft_id        ON work_orders (aircraft_id);
CREATE INDEX IF NOT EXISTS ix_work_orders_station_id         ON work_orders (station_id);
CREATE INDEX IF NOT EXISTS ix_work_orders_status             ON work_orders (status);
CREATE INDEX IF NOT EXISTS ix_work_orders_work_order_type    ON work_orders (work_order_type);
CREATE INDEX IF NOT EXISTS ix_work_orders_planned_start_date ON work_orders (planned_start_date);
CREATE INDEX IF NOT EXISTS ix_work_orders_planned_end_date   ON work_orders (planned_end_date);
CREATE INDEX IF NOT EXISTS ix_work_orders_work_package_id    ON work_orders (work_package_id);
CREATE INDEX IF NOT EXISTS ix_work_orders_deleted_at         ON work_orders (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_work_orders_updated_at') THEN
        CREATE TRIGGER trg_work_orders_updated_at
            BEFORE UPDATE ON work_orders
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- Back-fill FK from defects -> work_orders (deferred from 007)
-- ============================================================
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_defects_work_order_id'
    ) THEN
        ALTER TABLE defects
            ADD CONSTRAINT fk_defects_work_order_id
            FOREIGN KEY (work_order_id) REFERENCES work_orders (id) ON DELETE RESTRICT;
    END IF;
END;
$$;
