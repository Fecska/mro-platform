/**
 * Shared TypeScript types used across all features.
 * Feature-specific types live in their own feature/types.ts files.
 */

// ── Pagination ───────────────────────────────────────────────────────────────

export interface PaginatedResponse<T> {
  data: T[];
  meta: {
    total: number;
    page: number;
    page_size: number;
  };
}

export interface PaginationParams {
  page?: number;
  page_size?: number;
}

// ── API list params ──────────────────────────────────────────────────────────

export type SortDirection = 'asc' | 'desc';

export interface SortParams {
  sort?: string;           // e.g. "created_at:desc"
}

// ── Common entity fields ─────────────────────────────────────────────────────

export interface BaseEntity {
  id: string;
  created_at: string;
  updated_at: string;
  created_by: string;
  organisation_id: string;
}

// ── Status types (match backend state machines) ───────────────────────────────

export type WorkOrderStatus =
  | 'draft'
  | 'planned'
  | 'issued'
  | 'in_progress'
  | 'waiting_parts'
  | 'waiting_tooling'
  | 'waiting_inspection'
  | 'waiting_certification'
  | 'completed'
  | 'closed'
  | 'cancelled';

export type DefectStatus =
  | 'reported'
  | 'triaged'
  | 'open'
  | 'deferred'
  | 'rectification_in_progress'
  | 'inspection_pending'
  | 'cleared'
  | 'closed';

export type ReleaseStatus =
  | 'not_required'
  | 'required'
  | 'inspection_pending'
  | 'signoff_pending'
  | 'issued'
  | 'superseded'
  | 'revoked';

export type DefectSeverity = 'critical' | 'high' | 'medium' | 'low';

// ── UI helpers ───────────────────────────────────────────────────────────────

export interface SelectOption<T = string> {
  value: T;
  label: string;
}
