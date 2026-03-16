import { useQuery } from '@tanstack/react-query';
import { fetchDefects } from '../api/defectsApi';
import type { DefectListParams } from '../types';

export function useDefectList(params: DefectListParams = {}) {
  return useQuery({
    queryKey: ['defects', params],
    queryFn: () => fetchDefects(params),
  });
}
