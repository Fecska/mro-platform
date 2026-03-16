import { useAircraftList } from '@/features/aircraft/hooks/useAircraft';
import { useWorkOrderList } from '@/features/workorders/hooks/useWorkOrders';
import { useDefectList } from '@/features/defects/hooks/useDefects';
import { useEmployeeList } from '@/features/personnel/hooks/usePersonnel';
import { ExpiringCredentialsCard } from '@/components/dashboard/ExpiringCredentialsCard';
import { StatCard } from '@/components/common/StatCard';
import { PageHeader } from '@/components/common/PageHeader';

// ── Icons (inline SVG, no extra dep) ─────────────────────────────────────────

function PlaneIcon() {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className="w-5 h-5">
      <path d="M17.8 19.2 16 11l3.5-3.5C21 6 21 4 19 2c-2-2-4-2-5.5-.5L10 5 1.8 6.2a.8.8 0 0 0-.4 1.4l3 3L6 15l-2 2 1 1 2-2 4.4 1.6 3 3a.8.8 0 0 0 1.4-.4z" />
    </svg>
  );
}

function WrenchIcon() {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className="w-5 h-5">
      <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z" />
    </svg>
  );
}

function AlertIcon() {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className="w-5 h-5">
      <path d="M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
      <line x1="12" y1="9" x2="12" y2="13" />
      <line x1="12" y1="17" x2="12.01" y2="17" />
    </svg>
  );
}

function UsersIcon() {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className="w-5 h-5">
      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
      <circle cx="9" cy="7" r="4" />
      <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
      <path d="M16 3.13a4 4 0 0 1 0 7.75" />
    </svg>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function DashboardPage() {
  const { data: aircraft } = useAircraftList({ page_size: 1 });
  const { data: workOrders } = useWorkOrderList({ status: 'in_progress', page_size: 1 });
  const { data: defects } = useDefectList({ status: 'open', page_size: 1 });
  const { data: employees } = useEmployeeList({ status: 'Active', page_size: 1 });

  const totalAircraft    = aircraft?.meta.total ?? '—';
  const activeWorkOrders = workOrders?.meta.total ?? '—';
  const openDefects      = defects?.meta.total ?? '—';
  const activePersonnel  = employees?.meta.total ?? '—';

  return (
    <div>
      <PageHeader title="Dashboard" subtitle="Fleet & operations overview" />

      {/* Stats row */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <StatCard
          label="Total Aircraft"
          value={totalAircraft}
          sub="in fleet"
          icon={<PlaneIcon />}
          accent="blue"
        />
        <StatCard
          label="Active Work Orders"
          value={activeWorkOrders}
          sub="in progress"
          icon={<WrenchIcon />}
          accent="amber"
        />
        <StatCard
          label="Open Defects"
          value={openDefects}
          sub="requiring action"
          icon={<AlertIcon />}
          accent="red"
        />
        <StatCard
          label="Active Personnel"
          value={activePersonnel}
          sub="certifying staff"
          icon={<UsersIcon />}
          accent="green"
        />
      </div>

      {/* Expiring credentials */}
      <ExpiringCredentialsCard />
    </div>
  );
}
