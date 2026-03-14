import { useState, useCallback } from 'react';
import { ApiError } from '@/services/apiClient';

/**
 * Hook for handling API hard stop errors.
 * Hard stops are compliance blocks that must be displayed prominently
 * and cannot be dismissed without the user explicitly acknowledging them.
 *
 * Reference: docs/compliance/hard-stop-rules.md
 */
export function useHardStop() {
  const [hardStop, setHardStop] = useState<{
    ruleId: string;
    message: string;
  } | null>(null);

  const handleError = useCallback((error: unknown) => {
    if (error instanceof ApiError && error.isHardStop) {
      setHardStop({
        ruleId: error.hardStopRule ?? 'HS-???',
        message: error.message,
      });
      return true; // handled
    }
    return false; // not a hard stop
  }, []);

  const dismiss = useCallback(() => setHardStop(null), []);

  return { hardStop, handleError, dismiss };
}
