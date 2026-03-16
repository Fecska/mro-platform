-- Migration 009: Work order tasks and task assignments

-- ============================================================
-- TABLE: work_order_tasks
-- ============================================================
CREATE TABLE IF NOT EXISTS work_order_tasks (
    id                      UUID            NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    work_order_id           UUID            NOT NULL REFERENCES work_orders (id) ON DELETE RESTRICT,
    task_number             TEXT            NOT NULL,
    description             TEXT            NOT NULL,
    zone                    TEXT,
    access_panel            TEXT,
    estimated_man_hours     NUMERIC(8,2),
    actual_man_hours        NUMERIC(8,2),
    status                  TEXT            NOT NULL DEFAULT 'pending',
        -- pending | in_progress | completed | inspected | signed_off | na | deferred
    completed_at            TIMESTAMPTZ,
    completed_by            UUID,
    task_reference          TEXT,           -- AMM / SB / AD reference
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    created_by              UUID,
    updated_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_by              UUID,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_work_order_tasks_wo_tasknum
    ON work_order_tasks (work_order_id, task_number)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_work_order_tasks_work_order_id ON work_order_tasks (work_order_id);
CREATE INDEX IF NOT EXISTS ix_work_order_tasks_status        ON work_order_tasks (status);
CREATE INDEX IF NOT EXISTS ix_work_order_tasks_completed_by  ON work_order_tasks (completed_by);
CREATE INDEX IF NOT EXISTS ix_work_order_tasks_deleted_at    ON work_order_tasks (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_work_order_tasks_updated_at') THEN
        CREATE TRIGGER trg_work_order_tasks_updated_at
            BEFORE UPDATE ON work_order_tasks
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: task_assignments
-- ============================================================
CREATE TABLE IF NOT EXISTS task_assignments (
    id              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    task_id         UUID        NOT NULL REFERENCES work_order_tasks (id) ON DELETE RESTRICT,
    employee_id     UUID        NOT NULL,   -- FK to employees added in 010 via ALTER
    assigned_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    assigned_by     UUID,
    unassigned_at   TIMESTAMPTZ,
    role_on_task    TEXT,       -- e.g. 'mechanic' | 'inspector' | 'certifying_staff'
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by      UUID,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID
);

CREATE INDEX IF NOT EXISTS ix_task_assignments_task_id     ON task_assignments (task_id);
CREATE INDEX IF NOT EXISTS ix_task_assignments_employee_id ON task_assignments (employee_id);
CREATE INDEX IF NOT EXISTS ix_task_assignments_deleted_at  ON task_assignments (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_task_assignments_updated_at') THEN
        CREATE TRIGGER trg_task_assignments_updated_at
            BEFORE UPDATE ON task_assignments
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
