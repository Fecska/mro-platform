import { clsx } from 'clsx';
import type { ReactNode } from 'react';

interface StatCardProps {
  label: string;
  value: string | number;
  sub?: string;
  icon: ReactNode;
  accent?: 'blue' | 'green' | 'amber' | 'red' | 'slate';
}

const ACCENT: Record<NonNullable<StatCardProps['accent']>, { icon: string; value: string }> = {
  blue:  { icon: 'bg-blue-100 text-blue-600',   value: 'text-blue-700'  },
  green: { icon: 'bg-green-100 text-green-600',  value: 'text-green-700' },
  amber: { icon: 'bg-amber-100 text-amber-600',  value: 'text-amber-700' },
  red:   { icon: 'bg-red-100 text-red-600',      value: 'text-red-700'   },
  slate: { icon: 'bg-slate-100 text-slate-600',  value: 'text-slate-700' },
};

export function StatCard({ label, value, sub, icon, accent = 'slate' }: StatCardProps) {
  const colors = ACCENT[accent];
  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm px-5 py-4 flex items-center gap-4">
      <div className={clsx('w-10 h-10 rounded-lg flex items-center justify-center shrink-0', colors.icon)}>
        {icon}
      </div>
      <div className="min-w-0">
        <p className="text-xs text-gray-500 font-medium truncate">{label}</p>
        <p className={clsx('text-2xl font-bold leading-tight', colors.value)}>{value}</p>
        {sub && <p className="text-xs text-gray-400 mt-0.5">{sub}</p>}
      </div>
    </div>
  );
}
