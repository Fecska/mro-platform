import axios, { type AxiosError } from 'axios';
import { useAuthStore } from '@/features/auth/store/authStore';

/**
 * Central Axios instance for all API calls.
 * - Attaches JWT from auth store on every request
 * - Handles 401 by clearing auth and redirecting to login
 * - Normalises error shape to match api-standards.md
 */
export const apiClient = axios.create({
  baseURL: '/api/v1',
  headers: {
    'Content-Type': 'application/json',
  },
});

// ── Request interceptor — attach token ──────────────────────────────────────
apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ── Response interceptor — normalise errors ─────────────────────────────────
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiErrorResponse>) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().clearAuth();
      window.location.href = '/login';
    }
    return Promise.reject(normaliseError(error));
  },
);

// ── Types ────────────────────────────────────────────────────────────────────

export interface ApiErrorResponse {
  error: {
    code: string;
    message: string;
    hard_stop_rule?: string;
    fields?: { field: string; message: string }[];
  };
}

export class ApiError extends Error {
  constructor(
    public readonly code: string,
    message: string,
    public readonly hardStopRule?: string,
    public readonly fields?: { field: string; message: string }[],
  ) {
    super(message);
    this.name = 'ApiError';
  }

  get isHardStop() {
    return this.code === 'HARD_STOP';
  }

  get isValidation() {
    return this.code === 'VALIDATION_ERROR';
  }

  get isNotFound() {
    return this.code === 'NOT_FOUND';
  }
}

function normaliseError(error: AxiosError<ApiErrorResponse>): ApiError {
  const data = error.response?.data?.error;
  if (data) {
    return new ApiError(data.code, data.message, data.hard_stop_rule, data.fields);
  }
  return new ApiError('NETWORK_ERROR', error.message ?? 'Network error');
}
