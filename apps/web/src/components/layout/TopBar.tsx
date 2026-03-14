import { useAuthStore } from '@/features/auth/store/authStore';

export function TopBar() {
  const user = useAuthStore((s) => s.user);
  const clearAuth = useAuthStore((s) => s.clearAuth);

  return (
    <header className="h-14 shrink-0 bg-white border-b border-gray-200 flex items-center justify-between px-6">
      <div className="text-sm text-gray-500">
        {/* Breadcrumb placeholder — individual pages inject their own */}
      </div>

      <div className="flex items-center gap-4">
        {user && (
          <>
            <span className="text-sm text-gray-700 font-medium">{user.name}</span>
            <button
              onClick={clearAuth}
              className="text-xs text-gray-500 hover:text-gray-800 transition-colors"
            >
              Sign out
            </button>
          </>
        )}
      </div>
    </header>
  );
}
