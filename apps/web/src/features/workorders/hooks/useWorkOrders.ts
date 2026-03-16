import { useQuery } from '@tanstack/react-query';
import { fetchWorkOrders, fetchWorkOrderById } from '../api/workOrdersApi';
import type { WorkOrderListParams } from '../types';

export function useWorkOrderList(params: WorkOrderListParams = {}) {
  return useQuery({
    queryKey: ['work-orders', params],
    queryFn: () => fetchWorkOrders(params),
  });
}

export function useWorkOrderDetail(id: string) {
  return useQuery({
    queryKey: ['work-orders', id],
    queryFn: () => fetchWorkOrderById(id),
    enabled: !!id,
  });
}
