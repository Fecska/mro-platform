import { useState, type FormEvent } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '@/features/auth/store/authStore';
import { apiClient } from '@/services/apiClient';

const DEMO_USER = {
  id: 'demo-00000001',
  email: 'admin@demo-mro.hu',
  name: 'Admin',
  organisationId: 'a0000000-0000-0000-0000-000000000001',
  stationIds: ['a0000000-0000-0000-0000-000000000002'],
  roles: ['admin'],
};

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const setAuth = useAuthStore((s) => s.setAuth);
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: Location })?.from?.pathname ?? '/dashboard';

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const { data } = await apiClient.post<{
        data: { access_token: string; user: import('@/features/auth/store/authStore').AuthUser };
      }>('/auth/login', { email, password });
      setAuth(data.data.access_token, data.data.user);
      navigate(from, { replace: true });
    } catch {
      setError('Invalid email or password. Try Demo Login instead.');
    } finally {
      setLoading(false);
    }
  };

  const handleDemoLogin = () => {
    setAuth('demo-token', DEMO_USER);
    navigate(from, { replace: true });
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-900 px-4">
      <div className="w-full max-w-sm space-y-4">

        {/* Demo banner */}
        <div className="bg-blue-600/20 border border-blue-500/40 rounded-xl px-4 py-3 text-center">
          <p className="text-blue-200 text-xs font-medium uppercase tracking-wide mb-1">Demo environment</p>
          <p className="text-white text-sm">Click the Demo Login button to explore the platform</p>
        </div>

        <div className="bg-white rounded-2xl shadow-xl p-8">
          {/* Logo + Title */}
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-blue-600 rounded-xl flex items-center justify-center flex-shrink-0">
              <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
              </svg>
            </div>
            <div>
              <h1 className="text-lg font-bold text-gray-900 leading-tight">MRO Platform</h1>
              <p className="text-xs text-gray-500">Aircraft Maintenance Management</p>
            </div>
          </div>

          {error && (
            <p className="mb-4 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {error}
            </p>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Email address</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@example.com"
                autoComplete="email"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                autoComplete="current-password"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <button
              type="submit"
              disabled={loading}
              className="w-full bg-gray-800 text-white rounded-lg py-2.5 text-sm font-semibold hover:bg-gray-900 disabled:opacity-50 transition-colors"
            >
              {loading ? 'Signing in…' : 'Sign in'}
            </button>
          </form>

          <div className="relative my-4">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-200" />
            </div>
            <div className="relative flex justify-center">
              <span className="bg-white px-3 text-xs text-gray-400">or</span>
            </div>
          </div>

          <button
            onClick={handleDemoLogin}
            className="w-full bg-blue-600 text-white rounded-lg py-2.5 text-sm font-semibold hover:bg-blue-700 transition-colors flex items-center justify-center gap-2"
          >
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
            Demo Login
          </button>

          <p className="mt-4 text-center text-xs text-gray-400">
            Demo account: admin@demo-mro.hu
          </p>
        </div>
      </div>
    </div>
  );
}
