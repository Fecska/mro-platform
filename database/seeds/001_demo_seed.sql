-- Demo seed data for MRO Platform
-- Password for all demo accounts: Demo1234!
-- BCrypt hash (work factor 12): $2a$12$1fZH1d0saZ899mMGl0.vFuGvk4rV/0J2HiYh/5LSKDfV2iRtZGrpu
-- Run AFTER all migrations (001–020) have been applied.
-- All statements are idempotent via ON CONFLICT DO NOTHING.

DO $$
DECLARE
    -- Organisation & Station
    v_org_id        UUID := 'a0000000-0000-0000-0000-000000000001';
    v_station_id    UUID := 'a0000000-0000-0000-0000-000000000002';

    -- Users
    v_admin_id      UUID := 'b0000000-0000-0000-0000-000000000001';
    v_mgr_id        UUID := 'b0000000-0000-0000-0000-000000000002';
    v_cert1_id      UUID := 'b0000000-0000-0000-0000-000000000003';
    v_cert2_id      UUID := 'b0000000-0000-0000-0000-000000000004';
    v_mech1_id      UUID := 'b0000000-0000-0000-0000-000000000005';

    -- Aircraft
    v_ac1_id        UUID := 'c0000000-0000-0000-0000-000000000001'; -- HA-LVA  B738  serviceable
    v_ac2_id        UUID := 'c0000000-0000-0000-0000-000000000002'; -- HA-LVB  B738  in_maintenance
    v_ac3_id        UUID := 'c0000000-0000-0000-0000-000000000003'; -- HA-LVC  A320  serviceable
    v_ac4_id        UUID := 'c0000000-0000-0000-0000-000000000004'; -- HA-LVD  A320  aog
    v_ac5_id        UUID := 'c0000000-0000-0000-0000-000000000005'; -- HA-LVE  DH8D  stored

    -- Employees
    v_emp1_id       UUID := 'd0000000-0000-0000-0000-000000000001'; -- Nagy Péter     B1.1 GREEN
    v_emp2_id       UUID := 'd0000000-0000-0000-0000-000000000002'; -- Kovács Anna    B2   GREEN
    v_emp3_id       UUID := 'd0000000-0000-0000-0000-000000000003'; -- Szabó Gábor    B1.1 AMBER
    v_emp4_id       UUID := 'd0000000-0000-0000-0000-000000000004'; -- Tóth Mária     C    RED
    v_emp5_id       UUID := 'd0000000-0000-0000-0000-000000000005'; -- Fekete László  B1.1+C GREEN
    v_emp6_id       UUID := 'd0000000-0000-0000-0000-000000000006'; -- Varga Eszter   B2   AMBER
    v_emp7_id       UUID := 'd0000000-0000-0000-0000-000000000007'; -- Horváth Dániel B1.2 GREEN on_leave

    -- Work Orders
    v_wo1_id        UUID := 'e0000000-0000-0000-0000-000000000001';
    v_wo2_id        UUID := 'e0000000-0000-0000-0000-000000000002';
    v_wo3_id        UUID := 'e0000000-0000-0000-0000-000000000003';
    v_wo4_id        UUID := 'e0000000-0000-0000-0000-000000000004';

    -- Defects
    v_def1_id       UUID := 'f0000000-0000-0000-0000-000000000001';
    v_def2_id       UUID := 'f0000000-0000-0000-0000-000000000002';
    v_def3_id       UUID := 'f0000000-0000-0000-0000-000000000003';
    v_def4_id       UUID := 'f0000000-0000-0000-0000-000000000004';

    v_pw_hash       TEXT := '$2a$12$1fZH1d0saZ899mMGl0.vFuGvk4rV/0J2HiYh/5LSKDfV2iRtZGrpu';
    v_role_admin    UUID;
    v_role_manager  UUID;
    v_role_cert     UUID;
    v_role_mech     UUID;
BEGIN

-- ── Organisation ─────────────────────────────────────────────
INSERT INTO organisations (id, name, icao_code, country_code, easa_approval_ref, created_at, updated_at)
VALUES (v_org_id, 'Demo MRO Ltd.', 'DMRO', 'HU', 'HU.145.0042', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- ── Station ───────────────────────────────────────────────────
INSERT INTO stations (id, organisation_id, icao_code, name, created_at, updated_at)
VALUES (v_station_id, v_org_id, 'LHBP', 'Budapest Ferihegy Base', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- ── Users ─────────────────────────────────────────────────────
INSERT INTO users (id, email, password_hash, display_name, organisation_id, is_active, created_at, updated_at)
VALUES
    (v_admin_id,  'admin@demo-mro.hu',           v_pw_hash, 'Admin',          v_org_id, TRUE, NOW(), NOW()),
    (v_mgr_id,    'fekete.laszlo@demo-mro.hu',   v_pw_hash, 'Fekete László',  v_org_id, TRUE, NOW(), NOW()),
    (v_cert1_id,  'nagy.peter@demo-mro.hu',      v_pw_hash, 'Nagy Péter',     v_org_id, TRUE, NOW(), NOW()),
    (v_cert2_id,  'kovacs.anna@demo-mro.hu',     v_pw_hash, 'Kovács Anna',    v_org_id, TRUE, NOW(), NOW()),
    (v_mech1_id,  'szabo.gabor@demo-mro.hu',     v_pw_hash, 'Szabó Gábor',   v_org_id, TRUE, NOW(), NOW())
ON CONFLICT (email) DO NOTHING;

-- ── Role assignments ──────────────────────────────────────────
SELECT id INTO v_role_admin   FROM roles WHERE name = 'admin'            LIMIT 1;
SELECT id INTO v_role_manager FROM roles WHERE name = 'manager'          LIMIT 1;
SELECT id INTO v_role_cert    FROM roles WHERE name = 'certifying_staff' LIMIT 1;
SELECT id INTO v_role_mech    FROM roles WHERE name = 'mechanic'         LIMIT 1;

INSERT INTO user_roles (user_id, role_id, granted_at, created_at, updated_at)
VALUES
    (v_admin_id,  v_role_admin,   NOW(), NOW(), NOW()),
    (v_mgr_id,    v_role_manager, NOW(), NOW(), NOW()),
    (v_cert1_id,  v_role_cert,    NOW(), NOW(), NOW()),
    (v_cert2_id,  v_role_cert,    NOW(), NOW(), NOW()),
    (v_mech1_id,  v_role_mech,    NOW(), NOW(), NOW())
ON CONFLICT (user_id, role_id) DO NOTHING;

-- ── Aircraft types ────────────────────────────────────────────
INSERT INTO aircraft_types (id, icao_type_code, iata_type_code, manufacturer, model, category, engine_count, engine_type, created_at, updated_at)
VALUES
    (gen_random_uuid(), 'B738', '73H', 'Boeing',     '737-800',      'fixed_wing', 2, 'turbofan',  NOW(), NOW()),
    (gen_random_uuid(), 'A320', '320', 'Airbus',     'A320-200',     'fixed_wing', 2, 'turbofan',  NOW(), NOW()),
    (gen_random_uuid(), 'DH8D', 'DH4', 'Bombardier', 'Dash 8 Q400', 'turboprop',  2, 'turboprop', NOW(), NOW())
ON CONFLICT (icao_type_code) DO NOTHING;

-- ── Aircraft ──────────────────────────────────────────────────
INSERT INTO aircraft (id, organisation_id, registration, icao_type_code, msn, manufacturer, model, year_of_manufacture, operator_name, operator_icao, is_active, created_at, updated_at)
VALUES
    (v_ac1_id, v_org_id, 'HA-LVA', 'B738', 'MSN-29340', 'Boeing',     '737-800',      2015, 'Demo Airlines', 'DMA', TRUE, NOW(), NOW()),
    (v_ac2_id, v_org_id, 'HA-LVB', 'B738', 'MSN-29341', 'Boeing',     '737-800',      2015, 'Demo Airlines', 'DMA', TRUE, NOW(), NOW()),
    (v_ac3_id, v_org_id, 'HA-LVC', 'A320', 'MSN-6284',  'Airbus',     'A320-200',     2018, 'Demo Airlines', 'DMA', TRUE, NOW(), NOW()),
    (v_ac4_id, v_org_id, 'HA-LVD', 'A320', 'MSN-6285',  'Airbus',     'A320-200',     2018, 'Demo Airlines', 'DMA', TRUE, NOW(), NOW()),
    (v_ac5_id, v_org_id, 'HA-LVE', 'DH8D', 'MSN-4427',  'Bombardier', 'Dash 8 Q400', 2010, 'Demo Airlines', 'DMA', TRUE, NOW(), NOW())
ON CONFLICT DO NOTHING;

-- ── Aircraft counters ─────────────────────────────────────────
INSERT INTO aircraft_counters (id, aircraft_id, total_hours, total_cycles, total_pressurisation_cycles, last_updated_at, created_at, updated_at)
VALUES
    (gen_random_uuid(), v_ac1_id, 18452.30, 12840, 12840, NOW(), NOW(), NOW()),
    (gen_random_uuid(), v_ac2_id, 17918.70, 12501, 12501, NOW(), NOW(), NOW()),
    (gen_random_uuid(), v_ac3_id, 21340.50, 14200, 14200, NOW(), NOW(), NOW()),
    (gen_random_uuid(), v_ac4_id, 20150.80, 13890, 13890, NOW(), NOW(), NOW()),
    (gen_random_uuid(), v_ac5_id,  8230.00,  9100,  8920, NOW(), NOW(), NOW())
ON CONFLICT (aircraft_id) DO NOTHING;

-- ── Aircraft status history ────────────────────────────────────
INSERT INTO aircraft_status_history (id, aircraft_id, status, previous_status, reason, effective_at, created_at, updated_at)
VALUES
    -- HA-LVA: serviceable
    (gen_random_uuid(), v_ac1_id, 'serviceable',    NULL,            'Entry into service',                            NOW() - INTERVAL '2 years',  NOW(), NOW()),
    -- HA-LVB: serviceable → in_maintenance
    (gen_random_uuid(), v_ac2_id, 'serviceable',    NULL,            'Entry into service',                            NOW() - INTERVAL '2 years',  NOW(), NOW()),
    (gen_random_uuid(), v_ac2_id, 'in_maintenance', 'serviceable',   'Scheduled C-check — 6Y/12000FH interval',       NOW() - INTERVAL '5 days',   NOW(), NOW()),
    -- HA-LVC: serviceable
    (gen_random_uuid(), v_ac3_id, 'serviceable',    NULL,            'Entry into service',                            NOW() - INTERVAL '18 months', NOW(), NOW()),
    -- HA-LVD: serviceable → unserviceable → aog
    (gen_random_uuid(), v_ac4_id, 'serviceable',    NULL,            'Entry into service',                            NOW() - INTERVAL '18 months', NOW(), NOW()),
    (gen_random_uuid(), v_ac4_id, 'unserviceable',  'serviceable',   'Hydraulic system leak detected on brakes',      NOW() - INTERVAL '2 days',   NOW(), NOW()),
    (gen_random_uuid(), v_ac4_id, 'aog',            'unserviceable', 'AOG declared — flight operations suspended',    NOW() - INTERVAL '1 day',    NOW(), NOW()),
    -- HA-LVE: serviceable → stored
    (gen_random_uuid(), v_ac5_id, 'serviceable',    NULL,            'Entry into service',                            NOW() - INTERVAL '3 years',  NOW(), NOW()),
    (gen_random_uuid(), v_ac5_id, 'stored',         'serviceable',   'Winter storage — seasonal route suspension',    NOW() - INTERVAL '90 days',  NOW(), NOW())
ON CONFLICT DO NOTHING;

-- ── Employees ─────────────────────────────────────────────────
INSERT INTO employees (id, organisation_id, user_id, first_name, last_name, email, date_of_birth, nationality_code, employment_status, created_at, updated_at)
VALUES
    (v_emp1_id, v_org_id, v_cert1_id, 'Péter',  'Nagy',    'nagy.peter@demo-mro.hu',    '1985-04-12', 'HU', 'active',   NOW(), NOW()),
    (v_emp2_id, v_org_id, v_cert2_id, 'Anna',   'Kovács',  'kovacs.anna@demo-mro.hu',   '1990-07-23', 'HU', 'active',   NOW(), NOW()),
    (v_emp3_id, v_org_id, v_mech1_id, 'Gábor',  'Szabó',   'szabo.gabor@demo-mro.hu',   '1982-11-05', 'HU', 'active',   NOW(), NOW()),
    (v_emp4_id, v_org_id, NULL,       'Mária',  'Tóth',    'toth.maria@demo-mro.hu',    '1978-03-18', 'HU', 'active',   NOW(), NOW()),
    (v_emp5_id, v_org_id, v_mgr_id,   'László', 'Fekete',  'fekete.laszlo@demo-mro.hu', '1975-09-30', 'HU', 'active',   NOW(), NOW()),
    (v_emp6_id, v_org_id, NULL,       'Eszter', 'Varga',   'varga.eszter@demo-mro.hu',  '1993-02-14', 'HU', 'active',   NOW(), NOW()),
    (v_emp7_id, v_org_id, NULL,       'Dániel', 'Horváth', 'horvath.daniel@demo-mro.hu','1988-06-22', 'HU', 'on_leave', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- ── Authorisations ────────────────────────────────────────────
-- Status legend: GREEN = valid >90 days, AMBER = valid 1-90 days, RED = expired / suspended
INSERT INTO authorisations (id, employee_id, category, aircraft_types, scope, issued_at, expires_at, created_at, updated_at)
VALUES
    -- Nagy Péter — B1.1 valid 3 years (GREEN)
    (gen_random_uuid(), v_emp1_id, 'B1.1', 'B737,B738', 'Turbine powered aeroplanes — Boeing 737 family',     '2022-01-15', '2027-01-14', NOW(), NOW()),
    -- Kovács Anna — B2 valid 2 years (GREEN)
    (gen_random_uuid(), v_emp2_id, 'B2',   'B737,A320', 'Avionics — Boeing 737 & Airbus A320 family',         '2021-06-01', '2026-05-31', NOW(), NOW()),
    -- Szabó Gábor — B1.1 expiring in ~45 days (AMBER)
    (gen_random_uuid(), v_emp3_id, 'B1.1', 'B737,B738', 'Turbine powered aeroplanes — Boeing 737 family',     '2022-04-30', (CURRENT_DATE + INTERVAL '45 days')::DATE, NOW(), NOW()),
    -- Tóth Mária — C expired 65 days ago (RED)
    (gen_random_uuid(), v_emp4_id, 'C',    NULL,        'Base maintenance release — all types on approval',   '2020-03-01', (CURRENT_DATE - INTERVAL '65 days')::DATE, NOW(), NOW()),
    -- Fekete László — B1.1 + C valid (GREEN)
    (gen_random_uuid(), v_emp5_id, 'B1.1', 'B737,A320', 'Turbine powered aeroplanes — multi-type',            '2019-09-01', '2027-08-31', NOW(), NOW()),
    (gen_random_uuid(), v_emp5_id, 'C',    NULL,        'Base maintenance release — all types on approval',   '2019-09-01', '2027-08-31', NOW(), NOW()),
    -- Varga Eszter — B2 valid (GREEN auth, but training AMBER)
    (gen_random_uuid(), v_emp6_id, 'B2',   'A320',      'Avionics — Airbus A320 family',                      '2023-02-14', '2027-02-13', NOW(), NOW()),
    -- Horváth Dániel — B1.2 valid (GREEN, on leave)
    (gen_random_uuid(), v_emp7_id, 'B1.2', 'DH8D',      'Turboprop aeroplanes — Bombardier Q series',         '2021-05-10', '2026-05-09', NOW(), NOW())
ON CONFLICT DO NOTHING;

-- ── Licences ──────────────────────────────────────────────────
INSERT INTO licences (id, employee_id, licence_type, licence_number, issuing_authority, issued_at, expires_at, is_active, created_at, updated_at)
VALUES
    -- Nagy Péter — EASA Part-66 B1 (GREEN)
    (gen_random_uuid(), v_emp1_id, 'EASA_PART66', 'HU.66.B1.00421', 'HU CAA', '2018-03-10', '2028-03-09', TRUE, NOW(), NOW()),
    -- Kovács Anna — EASA Part-66 B2 (GREEN)
    (gen_random_uuid(), v_emp2_id, 'EASA_PART66', 'HU.66.B2.00187', 'HU CAA', '2019-07-22', '2027-07-21', TRUE, NOW(), NOW()),
    -- Szabó Gábor — EASA Part-66 B1 expiring ~45 days (AMBER)
    (gen_random_uuid(), v_emp3_id, 'EASA_PART66', 'HU.66.B1.00356', 'HU CAA', '2020-04-15', (CURRENT_DATE + INTERVAL '45 days')::DATE, TRUE, NOW(), NOW()),
    -- Tóth Mária — EASA Part-66 C expired (RED)
    (gen_random_uuid(), v_emp4_id, 'EASA_PART66', 'HU.66.C.00098',  'HU CAA', '2016-02-28', (CURRENT_DATE - INTERVAL '65 days')::DATE, TRUE, NOW(), NOW()),
    -- Fekete László — EASA Part-66 B1 (GREEN)
    (gen_random_uuid(), v_emp5_id, 'EASA_PART66', 'HU.66.B1.00201', 'HU CAA', '2015-09-01', '2029-08-31', TRUE, NOW(), NOW()),
    -- Varga Eszter — EASA Part-66 B2 (GREEN)
    (gen_random_uuid(), v_emp6_id, 'EASA_PART66', 'HU.66.B2.00312', 'HU CAA', '2021-02-14', '2028-02-13', TRUE, NOW(), NOW()),
    -- Horváth Dániel — EASA Part-66 B1.2 (GREEN)
    (gen_random_uuid(), v_emp7_id, 'EASA_PART66', 'HU.66.B1.00498', 'HU CAA', '2020-05-10', '2027-05-09', TRUE, NOW(), NOW())
ON CONFLICT DO NOTHING;

-- ── Training records ──────────────────────────────────────────
INSERT INTO training_records (id, employee_id, course_name, provider, completed_at, expires_at, certificate_ref, created_at, updated_at)
VALUES
    (gen_random_uuid(), v_emp1_id, 'Human Factors (EASA Part-145)',      'AviaTrain Budapest', '2024-01-10', '2026-01-09', 'CERT-2024-HF-0421',  NOW(), NOW()),
    (gen_random_uuid(), v_emp1_id, 'Safety Management System',           'AviaTrain Budapest', '2023-06-15', '2025-06-14', 'CERT-2023-SMS-0421', NOW(), NOW()),
    (gen_random_uuid(), v_emp2_id, 'Human Factors (EASA Part-145)',      'AviaTrain Budapest', '2024-02-20', '2026-02-19', 'CERT-2024-HF-0187',  NOW(), NOW()),
    (gen_random_uuid(), v_emp3_id, 'Human Factors (EASA Part-145)',      'AviaTrain Budapest', '2023-11-05', '2025-11-04', 'CERT-2023-HF-0356',  NOW(), NOW()),
    (gen_random_uuid(), v_emp4_id, 'Human Factors (EASA Part-145)',      'AviaTrain Budapest', '2022-09-18', (CURRENT_DATE + INTERVAL '30 days')::DATE, 'CERT-2022-HF-0098', NOW(), NOW()),
    (gen_random_uuid(), v_emp5_id, 'Human Factors (EASA Part-145)',      'AviaTrain Budapest', '2024-03-01', '2026-02-28', 'CERT-2024-HF-0201',  NOW(), NOW()),
    (gen_random_uuid(), v_emp5_id, 'Dangerous Goods Awareness (Cat 6)', 'IATA Training',       '2023-12-10', '2025-12-09', 'CERT-2023-DGA-0201', NOW(), NOW()),
    -- Varga Eszter — Human Factors expiring in ~30 days (AMBER)
    (gen_random_uuid(), v_emp6_id, 'Human Factors (EASA Part-145)',      'AviaTrain Budapest', '2024-01-14', (CURRENT_DATE + INTERVAL '30 days')::DATE, 'CERT-2024-HF-0312', NOW(), NOW()),
    (gen_random_uuid(), v_emp7_id, 'Human Factors (EASA Part-145)',      'AviaTrain Budapest', '2023-08-22', '2025-08-21', 'CERT-2023-HF-0498',  NOW(), NOW())
ON CONFLICT DO NOTHING;

-- ── Work Orders ───────────────────────────────────────────────
INSERT INTO work_orders (id, organisation_id, aircraft_id, work_order_number, title, description, status, work_order_type, station_id, planned_start_date, planned_end_date, actual_start_date, created_at, updated_at)
VALUES
    (v_wo1_id, v_org_id, v_ac2_id, 'WO-2026-0001',
     'Scheduled C-Check — HA-LVB',
     '12Y / 24000FH base maintenance check. Includes structural inspection, systems functional test, and component changes per approved work scope.',
     'in_progress', 'base', v_station_id,
     CURRENT_DATE - INTERVAL '5 days', CURRENT_DATE + INTERVAL '15 days', CURRENT_DATE - INTERVAL '5 days',
     NOW(), NOW()),

    (v_wo2_id, v_org_id, v_ac4_id, 'WO-2026-0002',
     'AOG Repair — Hydraulic Brake System Leak',
     'Aircraft on ground: hydraulic system #1 brake line leak. Isolate system, replace affected component per AMM 29-31-01, functional test and return to service.',
     'in_progress', 'aog', v_station_id,
     CURRENT_DATE - INTERVAL '1 day', CURRENT_DATE + INTERVAL '2 days', CURRENT_DATE - INTERVAL '1 day',
     NOW(), NOW()),

    (v_wo3_id, v_org_id, v_ac3_id, 'WO-2026-0003',
     'Line Check — HA-LVC Pre-Season Inspection',
     'Annual line maintenance check before summer schedule. Walk-around, fluid levels, lights, safety equipment check.',
     'planned', 'line', v_station_id,
     CURRENT_DATE + INTERVAL '3 days', CURRENT_DATE + INTERVAL '4 days', NULL,
     NOW(), NOW()),

    (v_wo4_id, v_org_id, v_ac1_id, 'WO-2025-0047',
     'SATCOM Modification — Inmarsat SB 737-23-1479',
     'Installation of Inmarsat GX Aviation hardware. Modification completed per approved STC and SB. CRS issued.',
     'completed', 'modification', v_station_id,
     CURRENT_DATE - INTERVAL '45 days', CURRENT_DATE - INTERVAL '38 days', CURRENT_DATE - INTERVAL '45 days',
     NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- ── Defects ───────────────────────────────────────────────────
INSERT INTO defects (id, organisation_id, aircraft_id, defect_number, title, description, status, deferral_category, raised_at, raised_by, work_order_id, created_at, updated_at)
VALUES
    -- AOG — linked to WO-2026-0002
    (v_def1_id, v_org_id, v_ac4_id, 'DEF-2026-0012',
     'Hydraulic brake pressure low — system #1',
     'During pre-flight check brake pressure on system #1 found below minimum (1800 psi). No external leak visible. Aircraft declared AOG pending investigation.',
     'in_work', NULL,
     NOW() - INTERVAL '1 day', v_admin_id, v_wo2_id,
     NOW(), NOW()),

    -- Deferred via MEL — linked to WO-2026-0001
    (v_def2_id, v_org_id, v_ac2_id, 'DEF-2026-0009',
     'APU bleed valve slow response on ground start',
     'APU bleed valve taking >8 seconds to reach fully open position. Serviceable limit is 5 s. Deferred per DDG 49-11 item 08 for max 10 days. To be rectified during C-check.',
     'deferred', 'MEL',
     NOW() - INTERVAL '7 days', v_admin_id, v_wo1_id,
     NOW(), NOW()),

    -- Open, no WO yet
    (v_def3_id, v_org_id, v_ac1_id, 'DEF-2026-0007',
     'Aft lavatory smoke detector intermittent fault',
     'CMC log shows 3× intermittent smoke detector fault codes over past 14 days. No smoke event. Detector cleaned and tested serviceable. Monitoring in progress.',
     'open', NULL,
     NOW() - INTERVAL '10 days', v_admin_id, NULL,
     NOW(), NOW()),

    -- Open, no WO yet
    (v_def4_id, v_org_id, v_ac3_id, 'DEF-2026-0003',
     'Seat 14C recline mechanism inoperative',
     'Passenger complaint. Seat 14C recline seized. Temporary placard applied — seat locked upright. AMM repair required before next sector.',
     'open', NULL,
     NOW() - INTERVAL '14 days', v_admin_id, NULL,
     NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

END;
$$;
