-- Migration 004: Aircraft counters (TT hours / cycles) and status log

-- ============================================================
-- TABLE: aircraft_counters
-- One row per aircraft — acts as the live utilisation snapshot.
-- ============================================================
CREATE TABLE IF NOT EXISTS aircraft_counters (
    id                          UUID            NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    aircraft_id                 UUID            NOT NULL REFERENCES aircraft (id) ON DELETE RESTRICT,
    total_hours                 NUMERIC(10,2)   NOT NULL DEFAULT 0,
    total_cycles                INT             NOT NULL DEFAULT 0,
    total_pressurisation_cycles INT             NOT NULL DEFAULT 0,
    last_updated_at             TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    created_at                  TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    created_by                  UUID,
    updated_at                  TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_by                  UUID,
    deleted_at                  TIMESTAMPTZ,
    deleted_by                  UUID,
    CONSTRAINT uq_aircraft_counters_aircraft_id UNIQUE (aircraft_id)
);

CREATE INDEX IF NOT EXISTS ix_aircraft_counters_aircraft_id ON aircraft_counters (aircraft_id);
CREATE INDEX IF NOT EXISTS ix_aircraft_counters_deleted_at  ON aircraft_counters (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_aircraft_counters_updated_at') THEN
        CREATE TRIGGER trg_aircraft_counters_updated_at
            BEFORE UPDATE ON aircraft_counters
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: aircraft_status_history
-- Append-only history of aircraft operational status changes.
-- ============================================================
CREATE TABLE IF NOT EXISTS aircraft_status_history (
    id              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    aircraft_id     UUID        NOT NULL REFERENCES aircraft (id) ON DELETE RESTRICT,
    status          TEXT        NOT NULL,  -- serviceable | unserviceable | in_maintenance | aog | stored
    previous_status TEXT,
    reason          TEXT,
    effective_at    TIMESTAMPTZ NOT NULL,
    recorded_by     UUID,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by      UUID,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID
);

CREATE INDEX IF NOT EXISTS ix_aircraft_status_history_aircraft_id  ON aircraft_status_history (aircraft_id);
CREATE INDEX IF NOT EXISTS ix_aircraft_status_history_effective_at ON aircraft_status_history (effective_at DESC);
CREATE INDEX IF NOT EXISTS ix_aircraft_status_history_status       ON aircraft_status_history (status);
CREATE INDEX IF NOT EXISTS ix_aircraft_status_history_deleted_at   ON aircraft_status_history (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_aircraft_status_history_updated_at') THEN
        CREATE TRIGGER trg_aircraft_status_history_updated_at
            BEFORE UPDATE ON aircraft_status_history
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
