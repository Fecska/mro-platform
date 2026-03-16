import { Suspense, useEffect } from 'react';
import { AppRoutes } from './routes/AppRoutes';
import { useAuthStore } from './features/auth/store/authStore';

export function App() {
  const isInitialised = useAuthStore((s) => s.isInitialised);
  const initialise = useAuthStore((s) => s.initialise);

  useEffect(() => {
    initialise();
  }, [initialise]);

  // Wait for auth state to be resolved before rendering routes
  if (!isInitialised) {
    return (
      <div className="flex h-screen items-center justify-center bg-gray-50">
        <div className="text-gray-500 text-sm">Loading…</div>
      </div>
    );
  }

  return (
    <Suspense
      fallback={
        <div className="flex h-screen items-center justify-center bg-gray-50">
          <div className="text-gray-500 text-sm">Loading…</div>
        </div>
      }
    >
      <AppRoutes />
    </Suspense>
  );
}
