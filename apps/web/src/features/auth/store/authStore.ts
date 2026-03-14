import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface AuthUser {
  id: string;
  email: string;
  name: string;
  organisationId: string;
  stationIds: string[];
  roles: string[];
}

interface AuthState {
  isInitialised: boolean;
  isAuthenticated: boolean;
  accessToken: string | null;
  user: AuthUser | null;

  // Actions
  setAuth: (token: string, user: AuthUser) => void;
  clearAuth: () => void;
  initialise: () => void;
}

/**
 * Global auth state.
 * accessToken is persisted to localStorage (short TTL handled by API interceptor).
 * Sensitive user data is kept in memory only.
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      isInitialised: false,
      isAuthenticated: false,
      accessToken: null,
      user: null,

      setAuth: (token, user) =>
        set({
          isAuthenticated: true,
          accessToken: token,
          user,
          isInitialised: true,
        }),

      clearAuth: () =>
        set({
          isAuthenticated: false,
          accessToken: null,
          user: null,
          isInitialised: true,
        }),

      initialise: () =>
        set((state) => ({
          isInitialised: true,
          isAuthenticated: !!state.accessToken,
        })),
    }),
    {
      name: 'mro-auth',
      // Only persist token — user object is re-fetched on app load
      partialize: (state) => ({ accessToken: state.accessToken }),
    },
  ),
);
