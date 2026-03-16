-- Migration 013: Material reservations and material issues

-- ============================================================
-- TABLE: material_reservations
-- Reserves stock against a specific work order task.
-- quantity_on_hand on stock_items is decremented only at issue time.
-- ============================================================
CREATE TABLE IF NOT EXISTS material_reservations (
    id                      UUID            NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    stock_item_id           UUID            NOT NULL REFERENCES stock_items (id) ON DELETE RESTRICT,
    work_order_id           UUID            NOT NULL REFERENCES work_orders (id) ON DELETE RESTRICT,
    work_order_task_id      UUID            NOT NULL REFERENCES work_order_tasks (id) ON DELETE RESTRICT,
    quantity_reserved       NUMERIC(12,4)   NOT NULL,
    quantity_issued         NUMERIC(12,4)   NOT NULL DEFAULT 0,
    status                  TEXT            NOT NULL DEFAULT 'pending',
        -- pending | partially_issued | fully_issued | cancelled
    reserved_by             UUID,
    reserved_at             TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    cancelled_at            TIMESTAMPTZ,
    cancellation_reason     TEXT,
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    created_by              UUID,
    updated_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_by              UUID,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID
);

CREATE INDEX IF NOT EXISTS ix_material_reservations_stock_item_id      ON material_reservations (stock_item_id);
CREATE INDEX IF NOT EXISTS ix_material_reservations_work_order_id      ON material_reservations (work_order_id);
CREATE INDEX IF NOT EXISTS ix_material_reservations_work_order_task_id ON material_reservations (work_order_task_id);
CREATE INDEX IF NOT EXISTS ix_material_reservations_status             ON material_reservations (status);
CREATE INDEX IF NOT EXISTS ix_material_reservations_deleted_at         ON material_reservations (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_material_reservations_updated_at') THEN
        CREATE TRIGGER trg_material_reservations_updated_at
            BEFORE UPDATE ON material_reservations
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: material_issues
-- Immutable ledger of actual stock withdrawals.
-- Each row permanently decrements effective stock.
-- ============================================================
CREATE TABLE IF NOT EXISTS material_issues (
    id                  UUID            NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    stock_item_id       UUID            NOT NULL REFERENCES stock_items (id) ON DELETE RESTRICT,
    reservation_id      UUID                     REFERENCES material_reservations (id) ON DELETE RESTRICT,
    work_order_id       UUID            NOT NULL REFERENCES work_orders (id) ON DELETE RESTRICT,
    work_order_task_id  UUID            NOT NULL REFERENCES work_order_tasks (id) ON DELETE RESTRICT,
    quantity_issued     NUMERIC(12,4)   NOT NULL,
    issued_by           UUID            NOT NULL,
    issued_at           TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID
);

CREATE INDEX IF NOT EXISTS ix_material_issues_stock_item_id      ON material_issues (stock_item_id);
CREATE INDEX IF NOT EXISTS ix_material_issues_reservation_id     ON material_issues (reservation_id);
CREATE INDEX IF NOT EXISTS ix_material_issues_work_order_id      ON material_issues (work_order_id);
CREATE INDEX IF NOT EXISTS ix_material_issues_work_order_task_id ON material_issues (work_order_task_id);
CREATE INDEX IF NOT EXISTS ix_material_issues_issued_at          ON material_issues (issued_at DESC);
CREATE INDEX IF NOT EXISTS ix_material_issues_issued_by          ON material_issues (issued_by);
CREATE INDEX IF NOT EXISTS ix_material_issues_deleted_at         ON material_issues (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_material_issues_updated_at') THEN
        CREATE TRIGGER trg_material_issues_updated_at
            BEFORE UPDATE ON material_issues
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
