import { NavLink } from 'react-router-dom';
import { clsx } from 'clsx';

const NAV_ITEMS = [
  { to: '/dashboard',   label: 'Dashboard',    icon: '▦' },
  { to: '/aircraft',    label: 'Aircraft',      icon: '✈' },
  { to: '/defects',     label: 'Defects',       icon: '⚠' },
  { to: '/work-orders', label: 'Work Orders',   icon: '🔧' },
  { to: '/inventory',   label: 'Inventory',     icon: '📦' },
  { to: '/personnel',   label: 'Personnel',     icon: '👤' },
] as const;

export function Sidebar() {
  return (
    <aside className="w-56 shrink-0 bg-slate-900 text-slate-200 flex flex-col">
      {/* Logo / product name */}
      <div className="px-5 py-5 border-b border-slate-700">
        <span className="font-bold text-white tracking-wide text-sm">MRO Platform</span>
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-3 py-4 space-y-1">
        {NAV_ITEMS.map(({ to, label, icon }) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) =>
              clsx(
                'flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors',
                isActive
                  ? 'bg-slate-700 text-white'
                  : 'text-slate-400 hover:bg-slate-800 hover:text-slate-100',
              )
            }
          >
            <span className="w-5 text-center text-base" aria-hidden>{icon}</span>
            {label}
          </NavLink>
        ))}
      </nav>

      {/* Version stamp */}
      <div className="px-5 py-3 border-t border-slate-700 text-xs text-slate-500">
        v0.1.0-dev
      </div>
    </aside>
  );
}
