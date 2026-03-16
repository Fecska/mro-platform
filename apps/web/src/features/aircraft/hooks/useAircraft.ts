import { useQuery } from '@tanstack/react-query';
import { fetchAircraft, fetchAircraftById } from '../api/aircraftApi';
import type { AircraftListParams } from '../types';

export function useAircraftList(params: AircraftListParams = {}) {
  return useQuery({
    queryKey: ['aircraft', params],
    queryFn: () => fetchAircraft(params),
  });
}

export function useAircraftDetail(id: string) {
  return useQuery({
    queryKey: ['aircraft', id],
    queryFn: () => fetchAircraftById(id),
    enabled: !!id,
  });
}
