-- Migration 020: Seed — roles, permissions, and role-permission assignments
-- All statements are idempotent via ON CONFLICT DO NOTHING.

-- ============================================================
-- ROLES
-- ============================================================
INSERT INTO roles (id, name, description, created_at, updated_at)
VALUES
    (gen_random_uuid(), 'admin',            'Full platform administrator — all capabilities',                        NOW(), NOW()),
    (gen_random_uuid(), 'manager',          'Maintenance manager — planning, work order oversight, reporting',       NOW(), NOW()),
    (gen_random_uuid(), 'certifying_staff', 'EASA Part-145 certifying staff — can sign CRS and close work orders',  NOW(), NOW()),
    (gen_random_uuid(), 'mechanic',         'Licensed aircraft mechanic — executes tasks, issues materials',         NOW(), NOW()),
    (gen_random_uuid(), 'inspector',        'Quality inspector — performs and approves inspections',                 NOW(), NOW()),
    (gen_random_uuid(), 'stores',           'Stores / logistics — manages inventory and tool allocation',            NOW(), NOW()),
    (gen_random_uuid(), 'quality',          'Quality assurance — read-only audit, release and inspection oversight', NOW(), NOW()),
    (gen_random_uuid(), 'support',          'IT / help-desk support — limited read access and user assistance',      NOW(), NOW()),
    (gen_random_uuid(), 'readonly',         'Read-only observer — no write capabilities',                            NOW(), NOW())
ON CONFLICT (name) DO NOTHING;

-- ============================================================
-- PERMISSIONS
-- ============================================================
INSERT INTO permissions (id, code, description, created_at, updated_at)
VALUES
    -- Aircraft
    (gen_random_uuid(), 'aircraft.read',            'View aircraft records and counters',                   NOW(), NOW()),
    (gen_random_uuid(), 'aircraft.write',           'Create and update aircraft records',                   NOW(), NOW()),

    -- Work Orders
    (gen_random_uuid(), 'workorder.read',           'View work orders and tasks',                           NOW(), NOW()),
    (gen_random_uuid(), 'workorder.write',          'Create and update work orders and tasks',              NOW(), NOW()),
    (gen_random_uuid(), 'workorder.close',          'Close or cancel work orders',                          NOW(), NOW()),

    -- Inspections
    (gen_random_uuid(), 'inspection.read',          'View inspection records',                              NOW(), NOW()),
    (gen_random_uuid(), 'inspection.write',         'Create and record inspection outcomes',                NOW(), NOW()),
    (gen_random_uuid(), 'inspection.approve',       'Approve or waive inspection findings',                 NOW(), NOW()),

    -- Release Certificates
    (gen_random_uuid(), 'release.read',             'View release certificates',                            NOW(), NOW()),
    (gen_random_uuid(), 'release.write',            'Create and update release certificate drafts',         NOW(), NOW()),
    (gen_random_uuid(), 'release.sign',             'Digitally sign a Certificate of Release to Service',   NOW(), NOW()),

    -- Inventory / Materials
    (gen_random_uuid(), 'inventory.read',           'View parts catalogue and stock levels',                NOW(), NOW()),
    (gen_random_uuid(), 'inventory.write',          'Add / adjust stock items and bin locations',           NOW(), NOW()),
    (gen_random_uuid(), 'inventory.issue',          'Issue materials against work order tasks',             NOW(), NOW()),

    -- Tools
    (gen_random_uuid(), 'tools.read',               'View tool register and calibration status',            NOW(), NOW()),
    (gen_random_uuid(), 'tools.write',              'Add and update tool records and calibration data',     NOW(), NOW()),
    (gen_random_uuid(), 'tools.checkout',           'Check tools in and out against tasks',                 NOW(), NOW()),

    -- Personnel
    (gen_random_uuid(), 'personnel.read',           'View employee, licence, and training records',         NOW(), NOW()),
    (gen_random_uuid(), 'personnel.write',          'Create and update employee and authorisation records', NOW(), NOW()),

    -- Maintenance planning
    (gen_random_uuid(), 'maintenance.read',         'View maintenance programs, due items, and packages',   NOW(), NOW()),
    (gen_random_uuid(), 'maintenance.write',        'Create and update maintenance programs and due items', NOW(), NOW()),

    -- Administration
    (gen_random_uuid(), 'admin.users',              'Manage platform users, roles, and permissions',        NOW(), NOW()),
    (gen_random_uuid(), 'admin.organisations',      'Manage organisations and stations',                    NOW(), NOW()),
    (gen_random_uuid(), 'admin.audit',              'Access full audit event log',                          NOW(), NOW())
ON CONFLICT (code) DO NOTHING;

-- ============================================================
-- ROLE-PERMISSION ASSIGNMENTS
-- Uses CTEs to resolve names to IDs so re-running is safe.
-- ============================================================

-- Helper: assign a list of permission codes to a named role
-- We use a DO block with dynamic resolution to avoid hard-coding UUIDs.

DO $$
DECLARE
    v_role_id       UUID;
    v_perm_id       UUID;
    v_role_name     TEXT;
    v_perm_code     TEXT;

    -- Ordered list of (role_name, permission_code) pairs
    assignments     TEXT[][] := ARRAY[

        -- ------------------------------------------------
        -- admin: everything
        -- ------------------------------------------------
        ARRAY['admin', 'aircraft.read'],
        ARRAY['admin', 'aircraft.write'],
        ARRAY['admin', 'workorder.read'],
        ARRAY['admin', 'workorder.write'],
        ARRAY['admin', 'workorder.close'],
        ARRAY['admin', 'inspection.read'],
        ARRAY['admin', 'inspection.write'],
        ARRAY['admin', 'inspection.approve'],
        ARRAY['admin', 'release.read'],
        ARRAY['admin', 'release.write'],
        ARRAY['admin', 'release.sign'],
        ARRAY['admin', 'inventory.read'],
        ARRAY['admin', 'inventory.write'],
        ARRAY['admin', 'inventory.issue'],
        ARRAY['admin', 'tools.read'],
        ARRAY['admin', 'tools.write'],
        ARRAY['admin', 'tools.checkout'],
        ARRAY['admin', 'personnel.read'],
        ARRAY['admin', 'personnel.write'],
        ARRAY['admin', 'maintenance.read'],
        ARRAY['admin', 'maintenance.write'],
        ARRAY['admin', 'admin.users'],
        ARRAY['admin', 'admin.organisations'],
        ARRAY['admin', 'admin.audit'],

        -- ------------------------------------------------
        -- manager: most write capabilities, no admin + no release.sign
        -- ------------------------------------------------
        ARRAY['manager', 'aircraft.read'],
        ARRAY['manager', 'aircraft.write'],
        ARRAY['manager', 'workorder.read'],
        ARRAY['manager', 'workorder.write'],
        ARRAY['manager', 'workorder.close'],
        ARRAY['manager', 'inspection.read'],
        ARRAY['manager', 'inspection.write'],
        ARRAY['manager', 'inspection.approve'],
        ARRAY['manager', 'release.read'],
        ARRAY['manager', 'release.write'],
        ARRAY['manager', 'inventory.read'],
        ARRAY['manager', 'inventory.write'],
        ARRAY['manager', 'inventory.issue'],
        ARRAY['manager', 'tools.read'],
        ARRAY['manager', 'tools.write'],
        ARRAY['manager', 'tools.checkout'],
        ARRAY['manager', 'personnel.read'],
        ARRAY['manager', 'personnel.write'],
        ARRAY['manager', 'maintenance.read'],
        ARRAY['manager', 'maintenance.write'],
        ARRAY['manager', 'admin.audit'],

        -- ------------------------------------------------
        -- certifying_staff: sign CRS, approve inspections, close WOs
        -- ------------------------------------------------
        ARRAY['certifying_staff', 'aircraft.read'],
        ARRAY['certifying_staff', 'workorder.read'],
        ARRAY['certifying_staff', 'workorder.write'],
        ARRAY['certifying_staff', 'workorder.close'],
        ARRAY['certifying_staff', 'inspection.read'],
        ARRAY['certifying_staff', 'inspection.write'],
        ARRAY['certifying_staff', 'inspection.approve'],
        ARRAY['certifying_staff', 'release.read'],
        ARRAY['certifying_staff', 'release.write'],
        ARRAY['certifying_staff', 'release.sign'],
        ARRAY['certifying_staff', 'inventory.read'],
        ARRAY['certifying_staff', 'tools.read'],
        ARRAY['certifying_staff', 'maintenance.read'],
        ARRAY['certifying_staff', 'personnel.read'],

        -- ------------------------------------------------
        -- mechanic: execute tasks, issue materials, use tools
        -- ------------------------------------------------
        ARRAY['mechanic', 'aircraft.read'],
        ARRAY['mechanic', 'workorder.read'],
        ARRAY['mechanic', 'workorder.write'],
        ARRAY['mechanic', 'inspection.read'],
        ARRAY['mechanic', 'inventory.read'],
        ARRAY['mechanic', 'inventory.issue'],
        ARRAY['mechanic', 'tools.read'],
        ARRAY['mechanic', 'tools.checkout'],
        ARRAY['mechanic', 'maintenance.read'],
        ARRAY['mechanic', 'personnel.read'],

        -- ------------------------------------------------
        -- inspector: write and approve inspections
        -- ------------------------------------------------
        ARRAY['inspector', 'aircraft.read'],
        ARRAY['inspector', 'workorder.read'],
        ARRAY['inspector', 'inspection.read'],
        ARRAY['inspector', 'inspection.write'],
        ARRAY['inspector', 'inspection.approve'],
        ARRAY['inspector', 'release.read'],
        ARRAY['inspector', 'inventory.read'],
        ARRAY['inspector', 'tools.read'],
        ARRAY['inspector', 'maintenance.read'],
        ARRAY['inspector', 'personnel.read'],

        -- ------------------------------------------------
        -- stores: inventory + tools management
        -- ------------------------------------------------
        ARRAY['stores', 'aircraft.read'],
        ARRAY['stores', 'workorder.read'],
        ARRAY['stores', 'inventory.read'],
        ARRAY['stores', 'inventory.write'],
        ARRAY['stores', 'inventory.issue'],
        ARRAY['stores', 'tools.read'],
        ARRAY['stores', 'tools.write'],
        ARRAY['stores', 'tools.checkout'],
        ARRAY['stores', 'maintenance.read'],

        -- ------------------------------------------------
        -- quality: read-only across inspection, release, audit
        -- ------------------------------------------------
        ARRAY['quality', 'aircraft.read'],
        ARRAY['quality', 'workorder.read'],
        ARRAY['quality', 'inspection.read'],
        ARRAY['quality', 'release.read'],
        ARRAY['quality', 'inventory.read'],
        ARRAY['quality', 'tools.read'],
        ARRAY['quality', 'personnel.read'],
        ARRAY['quality', 'maintenance.read'],
        ARRAY['quality', 'admin.audit'],

        -- ------------------------------------------------
        -- support: limited read for user assistance
        -- ------------------------------------------------
        ARRAY['support', 'aircraft.read'],
        ARRAY['support', 'workorder.read'],
        ARRAY['support', 'inspection.read'],
        ARRAY['support', 'release.read'],
        ARRAY['support', 'inventory.read'],
        ARRAY['support', 'tools.read'],
        ARRAY['support', 'personnel.read'],
        ARRAY['support', 'maintenance.read'],

        -- ------------------------------------------------
        -- readonly: all .read permissions
        -- ------------------------------------------------
        ARRAY['readonly', 'aircraft.read'],
        ARRAY['readonly', 'workorder.read'],
        ARRAY['readonly', 'inspection.read'],
        ARRAY['readonly', 'release.read'],
        ARRAY['readonly', 'inventory.read'],
        ARRAY['readonly', 'tools.read'],
        ARRAY['readonly', 'personnel.read'],
        ARRAY['readonly', 'maintenance.read']
    ];

    pair TEXT[];
BEGIN
    FOREACH pair SLICE 1 IN ARRAY assignments
    LOOP
        v_role_name := pair[1];
        v_perm_code := pair[2];

        SELECT id INTO v_role_id FROM roles WHERE name = v_role_name LIMIT 1;
        SELECT id INTO v_perm_id FROM permissions WHERE code = v_perm_code LIMIT 1;

        IF v_role_id IS NULL THEN
            RAISE WARNING 'Role not found: %', v_role_name;
            CONTINUE;
        END IF;
        IF v_perm_id IS NULL THEN
            RAISE WARNING 'Permission not found: %', v_perm_code;
            CONTINUE;
        END IF;

        INSERT INTO role_permissions (role_id, permission_id, created_at, updated_at)
        VALUES (v_role_id, v_perm_id, NOW(), NOW())
        ON CONFLICT (role_id, permission_id) DO NOTHING;
    END LOOP;
END;
$$;
