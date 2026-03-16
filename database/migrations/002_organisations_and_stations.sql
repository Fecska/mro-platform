-- Migration 002: Organisations and stations (airports / MRO bases)

-- ============================================================
-- TABLE: organisations
-- ============================================================
CREATE TABLE IF NOT EXISTS organisations (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    name                TEXT        NOT NULL,
    icao_code           TEXT,
    country_code        TEXT,
    address             TEXT,
    is_active           BOOLEAN     NOT NULL DEFAULT TRUE,
    easa_approval_ref   TEXT,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID
);

CREATE INDEX IF NOT EXISTS ix_organisations_is_active   ON organisations (is_active);
CREATE INDEX IF NOT EXISTS ix_organisations_icao_code   ON organisations (icao_code);
CREATE INDEX IF NOT EXISTS ix_organisations_deleted_at  ON organisations (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_organisations_updated_at') THEN
        CREATE TRIGGER trg_organisations_updated_at
            BEFORE UPDATE ON organisations
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: stations
-- ============================================================
CREATE TABLE IF NOT EXISTS stations (
    id              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    icao_code       TEXT        NOT NULL,
    name            TEXT        NOT NULL,
    country_code    TEXT,
    is_active       BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by      UUID,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID
);

CREATE INDEX IF NOT EXISTS ix_stations_organisation_id ON stations (organisation_id);
CREATE INDEX IF NOT EXISTS ix_stations_icao_code        ON stations (icao_code);
CREATE INDEX IF NOT EXISTS ix_stations_is_active        ON stations (is_active);
CREATE INDEX IF NOT EXISTS ix_stations_deleted_at       ON stations (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_stations_updated_at') THEN
        CREATE TRIGGER trg_stations_updated_at
            BEFORE UPDATE ON stations
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: organisation_settings
-- Key-value store for per-organisation configuration flags.
-- ============================================================
CREATE TABLE IF NOT EXISTS organisation_settings (
    id              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id UUID        NOT NULL REFERENCES organisations (id) ON DELETE CASCADE,
    key             TEXT        NOT NULL,
    value           TEXT,
    value_type      TEXT        NOT NULL DEFAULT 'string',  -- string | bool | int | json
    description     TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by      UUID,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID,
    CONSTRAINT uq_organisation_settings_org_key UNIQUE (organisation_id, key)
);

CREATE INDEX IF NOT EXISTS ix_organisation_settings_organisation_id ON organisation_settings (organisation_id);

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_organisation_settings_updated_at') THEN
        CREATE TRIGGER trg_organisation_settings_updated_at
            BEFORE UPDATE ON organisation_settings
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- Back-fill FK from users -> organisations (deferred from 001)
-- ============================================================
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_users_organisation_id'
    ) THEN
        ALTER TABLE users
            ADD CONSTRAINT fk_users_organisation_id
            FOREIGN KEY (organisation_id) REFERENCES organisations (id) ON DELETE RESTRICT;
    END IF;
END;
$$;
