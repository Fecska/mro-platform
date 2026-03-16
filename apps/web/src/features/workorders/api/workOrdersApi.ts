import { apiClient } from '@/services/apiClient';
import type { PaginatedResponse } from '@/types/common';
import type { WorkOrder, WorkOrderListParams } from '../types';

export async function fetchWorkOrders(params: WorkOrderListParams = {}): Promise<PaginatedResponse<WorkOrder>> {
  const { data } = await apiClient.get<PaginatedResponse<WorkOrder>>('/work-orders', { params });
  return data;
}

export async function fetchWorkOrderById(id: string): Promise<WorkOrder> {
  const { data } = await apiClient.get<{ data: WorkOrder }>(`/work-orders/${id}`);
  return data.data;
}
