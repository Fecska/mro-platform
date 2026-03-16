import type { BaseEntity, WorkOrderStatus } from '@/types/common';

export type WorkOrderType =
  | 'LineMaintenance'
  | 'HeavyMaintenance'
  | 'Modification'
  | 'Inspection'
  | 'Overhaul';

export interface WorkOrder extends BaseEntity {
  work_order_number: string;
  title: string;
  aircraft_id: string;
  aircraft_registration: string;
  work_order_type: WorkOrderType;
  status: WorkOrderStatus;
  planned_start_date: string | null;
  planned_end_date: string | null;
  actual_start_date: string | null;
  actual_end_date: string | null;
  station_id: string | null;
  customer_reference: string | null;
  assigned_certifying_staff_id: string | null;
  assigned_certifying_staff_name: string | null;
}

export interface WorkOrderListParams {
  status?: WorkOrderStatus;
  aircraft_id?: string;
  page?: number;
  page_size?: number;
}
