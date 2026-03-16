import { useState } from 'react';
import { clsx } from 'clsx';
import { useWorkOrderList } from '@/features/workorders/hooks/useWorkOrders';
import type { WorkOrderStatus } from '@/types/common';
import type { WorkOrder, WorkOrderType } from '@/features/workorders/types';
import { PageHeader } from '@/components/common/PageHeader';
import { SkeletonTable } from '@/components/common/SkeletonTable';

// ── Status config ─────────────────────────────────────────────────────────────

const STATUS_CONFIG: Record<WorkOrderStatus, { label: string; badge: string }> = {
  draft:                  { label: 'Draft',                  badge: 'bg-gray-100 text-gray-600'    },
  planned:                { label: 'Planned',                badge: 'bg-blue-50 text-blue-600'     },
  issued:                 { label: 'Issued',                 badge: 'bg-blue-100 text-blue-700'    },
  in_progress:            { label: 'In Progress',            badge: 'bg-indigo-100 text-indigo-700'},
  waiting_parts:          { label: 'Waiting Parts',          badge: 'bg-amber-100 text-amber-700'  },
  waiting_tooling:        { label: 'Waiting Tooling',        badge: 'bg-amber-100 text-amber-700'  },
  waiting_inspection:     { label: 'Waiting Inspection',     badge: 'bg-amber-100 text-amber-700'  },
  waiting_certification:  { label: 'Waiting Certification',  badge: 'bg-purple-100 text-purple-700'},
  completed:              { label: 'Completed',              badge: 'bg-green-100 text-green-700'  },
  closed:                 { label: 'Closed',                 badge: 'bg-gray-100 text-gray-500'    },
  cancelled:              { label: 'Cancelled',              badge: 'bg-red-100 text-red-600'      },
};

const TYPE_LABELS: Record<WorkOrderType, string> = {
  LineMaintenance:  'Line',
  HeavyMaintenance: 'Heavy',
  Modification:     'Modification',
  Inspection:       'Inspection',
  Overhaul:         'Overhaul',
};

const STATUS_FILTERS: { value: WorkOrderStatus | ''; label: string }[] = [
  { value: '',                    label: 'All'       },
  { value: 'in_progress',         label: 'In Progress' },
  { value: 'waiting_parts',       label: 'Blocked'   },
  { value: 'waiting_certification', label: 'Pending CRS' },
  { value: 'completed',           label: 'Completed' },
];

// ── Row ───────────────────────────────────────────────────────────────────────

function WorkOrderRow({ wo }: { wo: WorkOrder }) {
  const cfg = STATUS_CONFIG[wo.status] ?? STATUS_CONFIG.draft;
  const planned = wo.planned_start_date
    ? new Date(wo.planned_start_date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
    : '—';

  return (
    <tr className="border-b border-gray-100 hover:bg-gray-50 transition-colors">
      <td className="py-3 px-4">
        <span className="font-mono text-sm font-semibold text-gray-900">{wo.work_order_number}</span>
      </td>
      <td className="py-3 px-4">
        <p className="text-sm font-medium text-gray-900 leading-tight">{wo.title}</p>
        {wo.customer_reference && (
          <p className="text-xs text-gray-400 mt-0.5">Ref: {wo.customer_reference}</p>
        )}
      </td>
      <td className="py-3 px-4">
        <span className="font-mono text-sm text-gray-700">{wo.aircraft_registration}</span>
      </td>
      <td className="py-3 px-4">
        <span className="text-xs px-2 py-0.5 rounded bg-slate-100 text-slate-600 font-medium">
          {TYPE_LABELS[wo.work_order_type] ?? wo.work_order_type}
        </span>
      </td>
      <td className="py-3 px-4">
        <span className={clsx('inline-block px-2.5 py-0.5 rounded-full text-xs font-medium', cfg.badge)}>
          {cfg.label}
        </span>
      </td>
      <td className="py-3 px-4 text-sm text-gray-500">{planned}</td>
      <td className="py-3 px-4 text-sm text-gray-500 max-w-[140px] truncate">
        {wo.assigned_certifying_staff_name ?? <span className="text-gray-300">—</span>}
      </td>
    </tr>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function WorkOrdersPage() {
  const [statusFilter, setStatusFilter] = useState<WorkOrderStatus | ''>('');

  const { data, isLoading, isError } = useWorkOrderList({
    ...(statusFilter ? { status: statusFilter } : {}),
    page_size: 50,
  });

  const total = data?.meta.total ?? 0;

  return (
    <div>
      <PageHeader
        title="Work Orders"
        subtitle={isLoading ? 'Loading…' : `${total} work orders`}
      />

      <div className="flex items-center gap-2 mb-4 flex-wrap">
        {STATUS_FILTERS.map(({ value, label }) => (
          <button
            key={value}
            onClick={() => setStatusFilter(value as WorkOrderStatus | '')}
            className={clsx(
              'px-3 py-1.5 rounded-lg text-sm font-medium border transition-colors',
              statusFilter === value
                ? 'bg-gray-900 text-white border-gray-900'
                : 'bg-white text-gray-600 border-gray-200 hover:border-gray-300 hover:bg-gray-50',
            )}
          >
            {label}
          </button>
        ))}
      </div>

      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        {isError ? (
          <div className="px-6 py-12 text-center text-sm text-red-500">
            Failed to load work orders.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-200 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                  <th className="py-3 px-4">WO #</th>
                  <th className="py-3 px-4">Title</th>
                  <th className="py-3 px-4">Aircraft</th>
                  <th className="py-3 px-4">Type</th>
                  <th className="py-3 px-4">Status</th>
                  <th className="py-3 px-4">Planned Start</th>
                  <th className="py-3 px-4">Certifying Staff</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  <SkeletonTable rows={6} cols={7} />
                ) : data && data.data.length > 0 ? (
                  data.data.map((wo) => <WorkOrderRow key={wo.id} wo={wo} />)
                ) : (
                  <tr>
                    <td colSpan={7} className="py-12 text-center text-sm text-gray-400">
                      No work orders found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}

        {data && data.data.length > 0 && (
          <div className="px-4 py-2.5 border-t border-gray-100 text-xs text-gray-400">
            Showing {data.data.length} of {data.meta.total}
          </div>
        )}
      </div>
    </div>
  );
}
