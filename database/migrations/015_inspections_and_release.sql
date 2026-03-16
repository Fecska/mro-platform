-- Migration 015: Inspections, release certificates (CRS/EASA Form 1), and signature events

-- ============================================================
-- TABLE: inspections
-- ============================================================
CREATE TABLE IF NOT EXISTS inspections (
    id                      UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id         UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    work_order_id           UUID        NOT NULL REFERENCES work_orders (id) ON DELETE RESTRICT,
    inspection_number       TEXT        NOT NULL,
    inspection_type         TEXT        NOT NULL,
        -- e.g. 'incoming' | 'in_process' | 'final' | 'duplicate' | 'independent' | 'ramp'
    status                  TEXT        NOT NULL DEFAULT 'pending',
        -- pending | in_progress | passed | failed | deferred | waived
    zone                    TEXT,
    description             TEXT,
    findings                TEXT,
    remarks                 TEXT,
    outcome_recorded_at     TIMESTAMPTZ,
    outcome_recorded_by     UUID,
    waiver_reason           TEXT,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by              UUID,
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by              UUID,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_inspections_wo_inspnum
    ON inspections (work_order_id, inspection_number)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_inspections_organisation_id      ON inspections (organisation_id);
CREATE INDEX IF NOT EXISTS ix_inspections_work_order_id        ON inspections (work_order_id);
CREATE INDEX IF NOT EXISTS ix_inspections_status               ON inspections (status);
CREATE INDEX IF NOT EXISTS ix_inspections_inspection_type      ON inspections (inspection_type);
CREATE INDEX IF NOT EXISTS ix_inspections_outcome_recorded_by  ON inspections (outcome_recorded_by);
CREATE INDEX IF NOT EXISTS ix_inspections_deleted_at           ON inspections (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_inspections_updated_at') THEN
        CREATE TRIGGER trg_inspections_updated_at
            BEFORE UPDATE ON inspections
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: release_certificates
-- Certificate of Release to Service (CRS) / EASA Form 1
-- ============================================================
CREATE TABLE IF NOT EXISTS release_certificates (
    id                          UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    organisation_id             UUID        NOT NULL REFERENCES organisations (id) ON DELETE RESTRICT,
    work_order_id               UUID        NOT NULL REFERENCES work_orders (id) ON DELETE RESTRICT,
    aircraft_id                 UUID        NOT NULL REFERENCES aircraft (id) ON DELETE RESTRICT,
    certificate_number          TEXT        NOT NULL,
    certificate_type            TEXT        NOT NULL DEFAULT 'crs',
        -- crs | easa_form1 | faa_8130 | dual_release
    status                      TEXT        NOT NULL DEFAULT 'draft',
        -- draft | signed | void
    aircraft_registration       TEXT        NOT NULL,
    work_order_number           TEXT        NOT NULL,
    scope                       TEXT        NOT NULL,
    regulatory_basis            TEXT        NOT NULL,   -- e.g. 'EASA Part-145 / EU 2042/2003'
    certifying_staff_user_id    UUID        NOT NULL REFERENCES users (id) ON DELETE RESTRICT,
    void_reason                 TEXT,
    void_at                     TIMESTAMPTZ,
    void_by                     UUID,
    created_at                  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by                  UUID,
    updated_at                  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by                  UUID,
    deleted_at                  TIMESTAMPTZ,
    deleted_by                  UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_release_certificates_org_certnum
    ON release_certificates (organisation_id, certificate_number)
    WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_release_certificates_organisation_id          ON release_certificates (organisation_id);
CREATE INDEX IF NOT EXISTS ix_release_certificates_work_order_id            ON release_certificates (work_order_id);
CREATE INDEX IF NOT EXISTS ix_release_certificates_aircraft_id              ON release_certificates (aircraft_id);
CREATE INDEX IF NOT EXISTS ix_release_certificates_status                   ON release_certificates (status);
CREATE INDEX IF NOT EXISTS ix_release_certificates_certifying_staff_user_id ON release_certificates (certifying_staff_user_id);
CREATE INDEX IF NOT EXISTS ix_release_certificates_deleted_at               ON release_certificates (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_release_certificates_updated_at') THEN
        CREATE TRIGGER trg_release_certificates_updated_at
            BEFORE UPDATE ON release_certificates
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;

-- ============================================================
-- TABLE: signature_events
-- Immutable cryptographic audit trail of each digital signature
-- applied to a release certificate.
-- ============================================================
CREATE TABLE IF NOT EXISTS signature_events (
    id              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    certificate_id  UUID        NOT NULL REFERENCES release_certificates (id) ON DELETE RESTRICT,
    signer_user_id  UUID        NOT NULL REFERENCES users (id) ON DELETE RESTRICT,
    signed_at       TIMESTAMPTZ NOT NULL,
    licence_ref     TEXT        NOT NULL,   -- Part-66 licence number used to sign
    method          TEXT        NOT NULL,   -- e.g. 'pin_based' | 'biometric' | 'hardware_token'
    statement_text  TEXT        NOT NULL,   -- verbatim regulatory statement accepted
    ip_address      TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by      UUID,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID
);

CREATE INDEX IF NOT EXISTS ix_signature_events_certificate_id ON signature_events (certificate_id);
CREATE INDEX IF NOT EXISTS ix_signature_events_signer_user_id ON signature_events (signer_user_id);
CREATE INDEX IF NOT EXISTS ix_signature_events_signed_at      ON signature_events (signed_at DESC);
CREATE INDEX IF NOT EXISTS ix_signature_events_deleted_at     ON signature_events (deleted_at) WHERE deleted_at IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_signature_events_updated_at') THEN
        CREATE TRIGGER trg_signature_events_updated_at
            BEFORE UPDATE ON signature_events
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
