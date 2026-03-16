import { useQuery } from '@tanstack/react-query';
import { fetchEmployees, fetchEmployeeById, fetchEmployeeCurrencyStatus } from '../api/personnelApi';
import type { EmployeeListParams } from '../types';

export function useEmployeeList(params: EmployeeListParams = {}) {
  return useQuery({
    queryKey: ['employees', params],
    queryFn: () => fetchEmployees(params),
  });
}

export function useEmployeeDetail(id: string) {
  return useQuery({
    queryKey: ['employees', id],
    queryFn: () => fetchEmployeeById(id),
    enabled: !!id,
  });
}

export function useEmployeeCurrency(id: string) {
  return useQuery({
    queryKey: ['employees', id, 'currency'],
    queryFn: () => fetchEmployeeCurrencyStatus(id),
    enabled: !!id,
  });
}
