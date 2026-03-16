-- Migration 001: Initial identity schema — users, roles, permissions, user_roles, role_permissions

-- ============================================================
-- EXTENSIONS
-- ============================================================
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================
-- TABLE: users
-- ============================================================
CREATE TABLE IF NOT EXISTS users (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    email               TEXT        NOT NULL,
    password_hash       TEXT        NOT NULL,
    display_name        TEXT,
    is_active           BOOLEAN     NOT NULL DEFAULT TRUE,
    last_login_at       TIMESTAMPTZ,
    organisation_id     UUID,                          -- FK added in migration 002 via ALTER
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by          UUID,
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by          UUID,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID,
    CONSTRAINT uq_users_email UNIQUE (email)
);

CREATE INDEX IF NOT EXISTS ix_users_organisation_id  ON users (organisation_id);
CREATE INDEX IF NOT EXISTS ix_users_is_active         ON users (is_active);
CREATE INDEX IF NOT EXISTS ix_users_deleted_at        ON users (deleted_at) WHERE deleted_at IS NULL;

-- ============================================================
-- TABLE: roles
-- ============================================================
CREATE TABLE IF NOT EXISTS roles (
    id          UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    name        TEXT        NOT NULL,
    description TEXT,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by  UUID,
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by  UUID,
    deleted_at  TIMESTAMPTZ,
    deleted_by  UUID,
    CONSTRAINT uq_roles_name UNIQUE (name)
);

-- ============================================================
-- TABLE: permissions
-- ============================================================
CREATE TABLE IF NOT EXISTS permissions (
    id          UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    code        TEXT        NOT NULL,
    description TEXT,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by  UUID,
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by  UUID,
    deleted_at  TIMESTAMPTZ,
    deleted_by  UUID,
    CONSTRAINT uq_permissions_code UNIQUE (code)
);

-- ============================================================
-- TABLE: user_roles  (composite PK)
-- ============================================================
CREATE TABLE IF NOT EXISTS user_roles (
    user_id     UUID        NOT NULL REFERENCES users (id) ON DELETE RESTRICT,
    role_id     UUID        NOT NULL REFERENCES roles (id) ON DELETE RESTRICT,
    granted_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    granted_by  UUID,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by  UUID,
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by  UUID,
    deleted_at  TIMESTAMPTZ,
    deleted_by  UUID,
    CONSTRAINT pk_user_roles PRIMARY KEY (user_id, role_id)
);

CREATE INDEX IF NOT EXISTS ix_user_roles_role_id ON user_roles (role_id);

-- ============================================================
-- TABLE: role_permissions  (composite PK)
-- ============================================================
CREATE TABLE IF NOT EXISTS role_permissions (
    role_id       UUID NOT NULL REFERENCES roles (id) ON DELETE RESTRICT,
    permission_id UUID NOT NULL REFERENCES permissions (id) ON DELETE RESTRICT,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by    UUID,
    updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by    UUID,
    deleted_at    TIMESTAMPTZ,
    deleted_by    UUID,
    CONSTRAINT pk_role_permissions PRIMARY KEY (role_id, permission_id)
);

CREATE INDEX IF NOT EXISTS ix_role_permissions_permission_id ON role_permissions (permission_id);

-- ============================================================
-- TABLE: refresh_tokens
-- ============================================================
CREATE TABLE IF NOT EXISTS refresh_tokens (
    id              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id         UUID        NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    token_hash      TEXT        NOT NULL,
    expires_at      TIMESTAMPTZ NOT NULL,
    issued_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    revoked_at      TIMESTAMPTZ,
    revoked_reason  TEXT,
    device_info     TEXT,
    ip_address      TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by      UUID,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID,
    CONSTRAINT uq_refresh_tokens_token_hash UNIQUE (token_hash)
);

CREATE INDEX IF NOT EXISTS ix_refresh_tokens_user_id    ON refresh_tokens (user_id);
CREATE INDEX IF NOT EXISTS ix_refresh_tokens_expires_at ON refresh_tokens (expires_at);
CREATE INDEX IF NOT EXISTS ix_refresh_tokens_revoked_at ON refresh_tokens (revoked_at) WHERE revoked_at IS NULL;

-- ============================================================
-- AUTO-UPDATE updated_at via trigger function (shared)
-- ============================================================
CREATE OR REPLACE FUNCTION fn_set_updated_at()
RETURNS TRIGGER
LANGUAGE plpgsql AS
$$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_users_updated_at') THEN
        CREATE TRIGGER trg_users_updated_at
            BEFORE UPDATE ON users
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_roles_updated_at') THEN
        CREATE TRIGGER trg_roles_updated_at
            BEFORE UPDATE ON roles
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_permissions_updated_at') THEN
        CREATE TRIGGER trg_permissions_updated_at
            BEFORE UPDATE ON permissions
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_user_roles_updated_at') THEN
        CREATE TRIGGER trg_user_roles_updated_at
            BEFORE UPDATE ON user_roles
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_role_permissions_updated_at') THEN
        CREATE TRIGGER trg_role_permissions_updated_at
            BEFORE UPDATE ON role_permissions
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_refresh_tokens_updated_at') THEN
        CREATE TRIGGER trg_refresh_tokens_updated_at
            BEFORE UPDATE ON refresh_tokens
            FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
    END IF;
END;
$$;
