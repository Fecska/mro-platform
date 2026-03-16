import { NavLink } from 'react-router-dom';
import { clsx } from 'clsx';

// ── Inline SVG icons (no extra dependency) ────────────────────────────────────

function DashboardIcon() {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" className="w-4 h-4">
      <rect x="3" y="3" width="7" height="7" rx="1" />
      <rect x="14" y="3" width="7" height="7" rx="1" />
      <rect x="3" y="14" width="7" height="7" rx="1" />
      <rect x="14" y="14" width="7" height="7" rx="1" />
    </svg>
  );
}

function AircraftIcon() {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" className="w-4 h-4">
      <path d="M17.8 19.2 16 11l3.5-3.5C21 6 21 4 19 2c-2-2-4-2-5.5-.5L10 5 1.8 6.2a.8.8 0 0 0-.4 1.4l3 3L6 15l-2 2 1 1 2-2 4.4 1.6 3 3a.8.8 0 0 0 1.4-.4z" />
    </svg>
  );
}

function DefectIcon() {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" className="w-4 h-4">
      <path d="M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
      <line x1="12" y1="9" x2="12" y2="13" />
      <line x1="12" y1="17" x2="12.01" y2="17" />
    </svg>
  );
}

function WorkOrderIcon() {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" className="w-4 h-4">
      <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z" />
    </svg>
  );
}

function InventoryIcon() {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" className="w-4 h-4">
      <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z" />
      <polyline points="3.27 6.96 12 12.01 20.73 6.96" />
      <line x1="12" y1="22.08" x2="12" y2="12" />
    </svg>
  );
}

function PersonnelIcon() {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" className="w-4 h-4">
      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
      <circle cx="9" cy="7" r="4" />
      <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
      <path d="M16 3.13a4 4 0 0 1 0 7.75" />
    </svg>
  );
}

// ── Nav items ─────────────────────────────────────────────────────────────────

const NAV_ITEMS = [
  { to: '/dashboard',   label: 'Dashboard',   icon: DashboardIcon  },
  { to: '/aircraft',    label: 'Aircraft',     icon: AircraftIcon   },
  { to: '/defects',     label: 'Defects',      icon: DefectIcon     },
  { to: '/work-orders', label: 'Work Orders',  icon: WorkOrderIcon  },
  { to: '/inventory',   label: 'Inventory',    icon: InventoryIcon  },
  { to: '/personnel',   label: 'Personnel',    icon: PersonnelIcon  },
] as const;

// ── Component ─────────────────────────────────────────────────────────────────

export function Sidebar() {
  return (
    <aside className="w-56 shrink-0 bg-slate-900 text-slate-200 flex flex-col">
      {/* Logo */}
      <div className="px-5 py-5 border-b border-slate-700/60">
        <div className="flex items-center gap-2.5">
          <div className="w-6 h-6 bg-blue-500 rounded-md flex items-center justify-center shrink-0">
            <svg viewBox="0 0 24 24" fill="white" className="w-3.5 h-3.5">
              <path d="M17.8 19.2 16 11l3.5-3.5C21 6 21 4 19 2c-2-2-4-2-5.5-.5L10 5 1.8 6.2a.8.8 0 0 0-.4 1.4l3 3L6 15l-2 2 1 1 2-2 4.4 1.6 3 3a.8.8 0 0 0 1.4-.4z" />
            </svg>
          </div>
          <div>
            <p className="font-bold text-white text-sm tracking-tight leading-none">MRO Platform</p>
            <p className="text-xs text-slate-500 mt-0.5 leading-none">Maintenance Management</p>
          </div>
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-3 py-4 space-y-0.5">
        {NAV_ITEMS.map(({ to, label, icon: Icon }) => (
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
            <span className="shrink-0" aria-hidden>
              <Icon />
            </span>
            {label}
          </NavLink>
        ))}
      </nav>

      {/* Version */}
      <div className="px-5 py-3 border-t border-slate-700/60 text-xs text-slate-600">
        v0.1.0
      </div>
    </aside>
  );
}
