-- Migration 003: Aircraft core — aircraft types, registration, installed components

-- ============================================================
-- TABLE: aircraft_types
-- ICAO type designator catalogue (global reference data).
-- ============================================================
CREATE TABLE IF NOT EXISTS aircraft_types (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    icao_type_code      TEXT        NOT NULL,
    iata_type_code      TEXT,
    manufacturer        TEXT        NOT NULL,
    model               TEXT        NOT NULL,
    category            TEXT        NOT NULL DEFAULT 'fixed_wing',  -- fixed_wing | rotorcraft | turboprop | piston
    max_takeoff_weight  NUMERIC(10,2),
    engine_count        INT,
    engine_type         TEXT,                                        -- turbofan | turboprop | piston | electric
    is_active           BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID,
    CONSTRAINT uq_aircraft_types_icao UNIQUE (icao_type_code)
);

CREATE INDEX IF NOT EXISTS ix_aircraft_types_is_active  ON aircraft_types (is_active);
CREATE INDEX IF NOT EXISTS ix_aircraft_types_deleted_at ON aircraft_types (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_aircraft_types_updated_at') THEN
        CREATE TRIGGER trg_aircraft_types_updated_at
            BEFORE UPDATE ON aircraft_types
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: aircraft
-- ============================================================
CREATE TABLE IF NOT EXISTS aircraft (
    id                      UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id         UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    aircraft_type_id        UUID        REFERENCES aircraft_types (id) ON DELETE RESTRICT,
    registration            TEXT        NOT NULL,
    icao_type_code          TEXT        NOT NULL,
    msn                     TEXT,
    manufacturer            TEXT,
    model                   TEXT,
    year_of_manufacture     INT,
    operator_name           TEXT,
    operator_icao           TEXT,
    is_active               BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by              UUID,
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by              UUID,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID
);

-- Unique registration per organisation (ignores soft-deleted rows)
CREATE UNIQUE INDEX IF NOT EXISTS uix_aircraft_org_registration
    ON aircraft (organisation_id, registration)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_aircraft_organisation_id  ON aircraft (organisation_id);
CREATE INDEX IF NOT EXISTS ix_aircraft_aircraft_type_id ON aircraft (aircraft_type_id);
CREATE INDEX IF NOT EXISTS ix_aircraft_icao_type_code   ON aircraft (icao_type_code);
CREATE INDEX IF NOT EXISTS ix_aircraft_registration     ON aircraft (registration);
CREATE INDEX IF NOT EXISTS ix_aircraft_is_active        ON aircraft (is_active);
CREATE INDEX IF NOT EXISTS ix_aircraft_deleted_at       ON aircraft (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_aircraft_updated_at') THEN
        CREATE TRIGGER trg_aircraft_updated_at
            BEFORE UPDATE ON aircraft
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: installed_components
-- Tracks rotable/life-limited parts currently fitted to an
-- aircraft or a position on it (e.g. engine, APU, landing gear).
-- ============================================================
CREATE TABLE IF NOT EXISTS installed_components (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id     UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    aircraft_id         UUID        NOT NULL REFERENCES aircraft (id) ON DELETE RESTRICT,
    part_number         TEXT        NOT NULL,
    serial_number       TEXT        NOT NULL,
    description         TEXT        NOT NULL,
    position            TEXT,                   -- e.g. 'ENG-1', 'LG-NOSE', 'APU'
    installed_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    installed_by        UUID,
    installed_work_order_id UUID,
    removed_at          TIMESTAMPTZ,
    removed_by          UUID,
    removed_work_order_id   UUID,
    removal_reason      TEXT,
    hours_at_install    NUMERIC(10,2),
    cycles_at_install   INT,
    is_life_limited     BOOLEAN     NOT NULL DEFAULT FALSE,
    life_limit_hours    NUMERIC(10,2),
    life_limit_cycles   INT,
    life_limit_date     DATE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID
);

-- Unique: a serial number can only be fitted once at a time
CREATE UNIQUE INDEX IF NOT EXISTS uix_installed_components_sn_active
    ON installed_components (serial_number)
    WHERE removed_at IS NULL AND deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_installed_components_aircraft_id      ON installed_components (aircraft_id);
CREATE INDEX IF NOT EXISTS ix_installed_components_organisation_id  ON installed_components (organisation_id);
CREATE INDEX IF NOT EXISTS ix_installed_components_part_number      ON installed_components (part_number);
CREATE INDEX IF NOT EXISTS ix_installed_components_is_life_limited  ON installed_components (is_life_limited) WHERE is_life_limited = TRUE;
CREATE INDEX IF NOT EXISTS ix_installed_components_deleted_at       ON installed_components (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_installed_components_updated_at') THEN
        CREATE TRIGGER trg_installed_components_updated_at
            BEFORE UPDATE ON installed_components
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
