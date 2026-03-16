import type { BaseEntity } from '@/types/common';

// ── Expiring credentials (existing) ─────────────────────────────────────────

export interface ExpiringCredentialDto {
  employeeId: string;
  employeeNumber: string;
  fullName: string;
  /** "Licence" | "Authorisation" */
  credentialType: 'Licence' | 'Authorisation';
  /** Licence number or authorisation number */
  identifier: string;
  /** e.g. "B1", "B2", "C" */
  category: string;
  /** ISO date string "yyyy-MM-dd" */
  expiresOn: string;
  /** Negative when already expired */
  daysRemaining: number;
}

export interface ExpiringCredentialsDto {
  days: number;
  totalCount: number;
  expiredCount: number;
  items: ExpiringCredentialDto[];
}

// ── Employee list ─────────────────────────────────────────────────────────────

export type EmployeeStatus = 'Active' | 'Inactive' | 'Suspended' | 'Terminated';
export type CurrencyStatus = 'Green' | 'Amber' | 'Red';

export interface EmployeeSummary extends BaseEntity {
  employee_number: string;
  first_name: string;
  last_name: string;
  full_name: string;
  email: string;
  status: EmployeeStatus;
  default_station_id: string | null;
}

export interface EmployeeCurrencyStatus {
  employee_id: string;
  status: CurrencyStatus;
  amber_reasons: string[];
  red_reasons: string[];
  has_active_shift_today: boolean;
  evaluated_at: string;
}

export interface Licence {
  id: string;
  licence_number: string;
  category: string;
  subcategory: string | null;
  issuing_authority: string;
  issued_at: string;
  expires_at: string | null;
  is_expired: boolean;
  is_current: boolean;
}

export interface Authorisation {
  id: string;
  authorisation_number: string;
  category: string;
  scope: string;
  valid_from: string;
  valid_until: string | null;
  is_expired: boolean;
  is_current: boolean;
  status: string;
}

export interface EmployeeDetail extends EmployeeSummary {
  phone: string | null;
  date_of_birth: string;
  nationality_code: string | null;
  licences: Licence[];
  authorisations: Authorisation[];
}

export interface EmployeeListParams {
  status?: EmployeeStatus;
  page?: number;
  page_size?: number;
}
