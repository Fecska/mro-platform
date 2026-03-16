-- Migration 012: Parts catalogue, bin locations, and stock items

-- ============================================================
-- TABLE: parts
-- Master parts catalogue (P/N level, not serialised)
-- ============================================================
CREATE TABLE IF NOT EXISTS parts (
    id                      UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id         UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    part_number             TEXT        NOT NULL,
    description             TEXT        NOT NULL,
    unit_of_measure         TEXT        NOT NULL DEFAULT 'each',
        -- each | kg | litre | metre | set | pair
    status                  TEXT        NOT NULL DEFAULT 'active',
        -- active | obsolete | superseded | restricted
    manufacturer            TEXT,
    alternate_part_numbers  TEXT[],
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by              UUID,
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by              UUID,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_parts_org_partnum
    ON parts (organisation_id, part_number)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_parts_organisation_id ON parts (organisation_id);
CREATE INDEX IF NOT EXISTS ix_parts_status          ON parts (status);
CREATE INDEX IF NOT EXISTS ix_parts_deleted_at      ON parts (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_parts_updated_at') THEN
        CREATE TRIGGER trg_parts_updated_at
            BEFORE UPDATE ON parts
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: bin_locations
-- Physical storage locations within a station/store
-- ============================================================
CREATE TABLE IF NOT EXISTS bin_locations (
    id              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    station_id      UUID                 REFERENCES stations (id) ON DELETE RESTRICT,
    code            TEXT        NOT NULL,
    description     TEXT,
    is_active       BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by      UUID,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_bin_locations_org_code
    ON bin_locations (organisation_id, code)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_bin_locations_organisation_id ON bin_locations (organisation_id);
CREATE INDEX IF NOT EXISTS ix_bin_locations_station_id      ON bin_locations (station_id);
CREATE INDEX IF NOT EXISTS ix_bin_locations_is_active       ON bin_locations (is_active);
CREATE INDEX IF NOT EXISTS ix_bin_locations_deleted_at      ON bin_locations (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_bin_locations_updated_at') THEN
        CREATE TRIGGER trg_bin_locations_updated_at
            BEFORE UPDATE ON bin_locations
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: stock_items
-- Individual stock records (serialised or batch/qty-based)
-- ============================================================
CREATE TABLE IF NOT EXISTS stock_items (
    id                  UUID            NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id     UUID            NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    part_id             UUID            NOT NULL REFERENCES parts (id) ON DELETE RESTRICT,
    bin_location_id     UUID                     REFERENCES bin_locations (id) ON DELETE RESTRICT,
    serial_number       TEXT,
    batch_number        TEXT,
    quantity_on_hand    NUMERIC(12,4)   NOT NULL DEFAULT 0,
    condition           TEXT            NOT NULL DEFAULT 'serviceable',
        -- serviceable | unserviceable | quarantine | awaiting_inspection | scrap
    certificate_ref     TEXT,           -- traceability certificate (e.g. EASA Form 1 / FAA 8130-3)
    received_at         TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    expiry_date         DATE,
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID
);

CREATE INDEX IF NOT EXISTS ix_stock_items_organisation_id  ON stock_items (organisation_id);
CREATE INDEX IF NOT EXISTS ix_stock_items_part_id          ON stock_items (part_id);
CREATE INDEX IF NOT EXISTS ix_stock_items_bin_location_id  ON stock_items (bin_location_id);
CREATE INDEX IF NOT EXISTS ix_stock_items_condition        ON stock_items (condition);
CREATE INDEX IF NOT EXISTS ix_stock_items_serial_number    ON stock_items (serial_number);
CREATE INDEX IF NOT EXISTS ix_stock_items_expiry_date      ON stock_items (expiry_date);
CREATE INDEX IF NOT EXISTS ix_stock_items_deleted_at       ON stock_items (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_stock_items_updated_at') THEN
        CREATE TRIGGER trg_stock_items_updated_at
            BEFORE UPDATE ON stock_items
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
