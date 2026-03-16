import { useQuery } from '@tanstack/react-query';
import { fetchExpiringCredentials } from '../api/personnelApi';

export function useExpiringCredentials(days: 30 | 60 | 90) {
  return useQuery({
    queryKey: ['expiring-credentials', days],
    queryFn: () => fetchExpiringCredentials(days),
  });
}
