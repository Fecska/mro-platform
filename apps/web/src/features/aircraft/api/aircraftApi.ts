import { apiClient } from '@/services/apiClient';
import type { PaginatedResponse } from '@/types/common';
import type { Aircraft, AircraftCounter, AircraftListParams } from '../types';

export async function fetchAircraft(params: AircraftListParams = {}): Promise<PaginatedResponse<Aircraft>> {
  const { data } = await apiClient.get<PaginatedResponse<Aircraft>>('/aircraft', { params });
  return data;
}

export async function fetchAircraftById(id: string): Promise<Aircraft> {
  const { data } = await apiClient.get<{ data: Aircraft }>(`/aircraft/${id}`);
  return data.data;
}

export async function fetchAircraftCounters(id: string): Promise<AircraftCounter[]> {
  const { data } = await apiClient.get<{ data: AircraftCounter[] }>(`/aircraft/${id}/counters`);
  return data.data;
}
