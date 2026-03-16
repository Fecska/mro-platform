import { apiClient } from '@/services/apiClient';
import type { PaginatedResponse } from '@/types/common';
import type {
  ExpiringCredentialsDto,
  EmployeeSummary,
  EmployeeDetail,
  EmployeeCurrencyStatus,
  EmployeeListParams,
} from '../types';

export async function fetchExpiringCredentials(days: 30 | 60 | 90): Promise<ExpiringCredentialsDto> {
  const { data } = await apiClient.get<ExpiringCredentialsDto>('/employees/expiring-credentials', {
    params: { days },
  });
  return data;
}

export async function fetchEmployees(params: EmployeeListParams = {}): Promise<PaginatedResponse<EmployeeSummary>> {
  const { data } = await apiClient.get<PaginatedResponse<EmployeeSummary>>('/employees', { params });
  return data;
}

export async function fetchEmployeeById(id: string): Promise<EmployeeDetail> {
  const { data } = await apiClient.get<{ data: EmployeeDetail }>(`/employees/${id}`);
  return data.data;
}

export async function fetchEmployeeCurrencyStatus(id: string): Promise<EmployeeCurrencyStatus> {
  const { data } = await apiClient.get<{ data: EmployeeCurrencyStatus }>(`/employees/${id}/currency-status`);
  return data.data;
}
