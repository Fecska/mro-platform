import type { BaseEntity, DefectStatus, DefectSeverity } from '@/types/common';

export type DefectSource =
  | 'PilotReport'
  | 'MaintenanceDiscovery'
  | 'AdMandate'
  | 'InternalAudit'
  | 'CustomerReport';

export interface Defect extends BaseEntity {
  defect_number: string;
  aircraft_id: string;
  aircraft_registration: string;
  title: string;
  description: string;
  severity: DefectSeverity;
  source: DefectSource;
  status: DefectStatus;
  ata_chapter: string | null;
  assigned_to_id: string | null;
  assigned_to_name: string | null;
  linked_work_order_id: string | null;
  reported_at: string;
  deferred_until: string | null;
  mel_reference: string | null;
}

export interface DefectListParams {
  status?: DefectStatus;
  severity?: DefectSeverity;
  aircraft_id?: string;
  page?: number;
  page_size?: number;
}
