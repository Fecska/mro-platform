import { apiClient } from '@/services/apiClient';
import type { PaginatedResponse } from '@/types/common';
import type { Defect, DefectListParams } from '../types';

export async function fetchDefects(params: DefectListParams = {}): Promise<PaginatedResponse<Defect>> {
  const { data } = await apiClient.get<PaginatedResponse<Defect>>('/defects', { params });
  return data;
}

export async function fetchDefectById(id: string): Promise<Defect> {
  const { data } = await apiClient.get<{ data: Defect }>(`/defects/${id}`);
  return data.data;
}
