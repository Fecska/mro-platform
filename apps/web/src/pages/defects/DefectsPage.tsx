import { useState } from 'react';
import { clsx } from 'clsx';
import { useDefectList } from '@/features/defects/hooks/useDefects';
import type { DefectStatus, DefectSeverity } from '@/types/common';
import type { Defect } from '@/features/defects/types';
import { PageHeader } from '@/components/common/PageHeader';
import { SkeletonTable } from '@/components/common/SkeletonTable';

// ── Config ────────────────────────────────────────────────────────────────────

const SEVERITY_CONFIG: Record<DefectSeverity, { label: string; badge: string; dot: string }> = {
  critical: { label: 'Critical', badge: 'bg-red-100 text-red-700',    dot: 'bg-red-500'    },
  high:     { label: 'High',     badge: 'bg-orange-100 text-orange-700', dot: 'bg-orange-500' },
  medium:   { label: 'Medium',   badge: 'bg-amber-100 text-amber-700', dot: 'bg-amber-500'  },
  low:      { label: 'Low',      badge: 'bg-green-100 text-green-700', dot: 'bg-green-500'  },
};

const STATUS_CONFIG: Record<DefectStatus, { label: string; badge: string }> = {
  reported:                  { label: 'Reported',       badge: 'bg-blue-50 text-blue-600'     },
  triaged:                   { label: 'Triaged',        badge: 'bg-blue-100 text-blue-700'    },
  open:                      { label: 'Open',           badge: 'bg-amber-100 text-amber-700'  },
  deferred:                  { label: 'Deferred',       badge: 'bg-purple-100 text-purple-700'},
  rectification_in_progress: { label: 'In Progress',    badge: 'bg-indigo-100 text-indigo-700'},
  inspection_pending:        { label: 'Insp. Pending',  badge: 'bg-amber-100 text-amber-700'  },
  cleared:                   { label: 'Cleared',        badge: 'bg-green-100 text-green-700'  },
  closed:                    { label: 'Closed',         badge: 'bg-gray-100 text-gray-500'    },
};

const STATUS_FILTERS: { value: DefectStatus | ''; label: string }[] = [
  { value: '',       label: 'All'        },
  { value: 'open',   label: 'Open'       },
  { value: 'triaged', label: 'Triaged'   },
  { value: 'deferred', label: 'Deferred' },
  { value: 'cleared', label: 'Cleared'  },
];

const SEVERITY_FILTERS: { value: DefectSeverity | ''; label: string }[] = [
  { value: '',         label: 'All Severities' },
  { value: 'critical', label: 'Critical'       },
  { value: 'high',     label: 'High'           },
  { value: 'medium',   label: 'Medium'         },
  { value: 'low',      label: 'Low'            },
];

// ── Row ───────────────────────────────────────────────────────────────────────

function DefectRow({ defect }: { defect: Defect }) {
  const sev = SEVERITY_CONFIG[defect.severity] ?? SEVERITY_CONFIG.low;
  const sts = STATUS_CONFIG[defect.status] ?? STATUS_CONFIG.open;
  const reported = new Date(defect.reported_at).toLocaleDateString('en-GB', {
    day: '2-digit', month: 'short', year: 'numeric',
  });

  return (
    <tr className="border-b border-gray-100 hover:bg-gray-50 transition-colors">
      <td className="py-3 px-4">
        <span className="font-mono text-sm font-semibold text-gray-900">{defect.defect_number}</span>
      </td>
      <td className="py-3 px-4">
        <p className="text-sm font-medium text-gray-900 leading-tight line-clamp-1">{defect.title}</p>
        {defect.ata_chapter && (
          <p className="text-xs text-gray-400 mt-0.5">ATA {defect.ata_chapter}</p>
        )}
      </td>
      <td className="py-3 px-4">
        <span className="font-mono text-sm text-gray-700">{defect.aircraft_registration}</span>
      </td>
      <td className="py-3 px-4">
        <span className={clsx('inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium', sev.badge)}>
          <span className={clsx('w-1.5 h-1.5 rounded-full shrink-0', sev.dot)} />
          {sev.label}
        </span>
      </td>
      <td className="py-3 px-4">
        <span className={clsx('inline-block px-2.5 py-0.5 rounded-full text-xs font-medium', sts.badge)}>
          {sts.label}
        </span>
      </td>
      <td className="py-3 px-4 text-sm text-gray-500 whitespace-nowrap">{reported}</td>
      <td className="py-3 px-4 text-sm text-gray-500 max-w-[130px] truncate">
        {defect.assigned_to_name ?? <span className="text-gray-300">Unassigned</span>}
      </td>
    </tr>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function DefectsPage() {
  const [statusFilter, setStatusFilter] = useState<DefectStatus | ''>('');
  const [severityFilter, setSeverityFilter] = useState<DefectSeverity | ''>('');

  const { data, isLoading, isError } = useDefectList({
    ...(statusFilter   ? { status:   statusFilter   } : {}),
    ...(severityFilter ? { severity: severityFilter } : {}),
    page_size: 50,
  });

  const total = data?.meta.total ?? 0;

  return (
    <div>
      <PageHeader
        title="Defects"
        subtitle={isLoading ? 'Loading…' : `${total} defect reports`}
      />

      {/* Filter bars */}
      <div className="flex flex-wrap items-center gap-2 mb-2">
        {STATUS_FILTERS.map(({ value, label }) => (
          <button
            key={value}
            onClick={() => setStatusFilter(value as DefectStatus | '')}
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
      <div className="flex flex-wrap items-center gap-2 mb-4">
        {SEVERITY_FILTERS.map(({ value, label }) => (
          <button
            key={value}
            onClick={() => setSeverityFilter(value as DefectSeverity | '')}
            className={clsx(
              'px-3 py-1.5 rounded-lg text-sm font-medium border transition-colors',
              severityFilter === value
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
            Failed to load defect data.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-200 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                  <th className="py-3 px-4">Defect #</th>
                  <th className="py-3 px-4">Title</th>
                  <th className="py-3 px-4">Aircraft</th>
                  <th className="py-3 px-4">Severity</th>
                  <th className="py-3 px-4">Status</th>
                  <th className="py-3 px-4">Reported</th>
                  <th className="py-3 px-4">Assigned To</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  <SkeletonTable rows={6} cols={7} />
                ) : data && data.data.length > 0 ? (
                  data.data.map((d) => <DefectRow key={d.id} defect={d} />)
                ) : (
                  <tr>
                    <td colSpan={7} className="py-12 text-center text-sm text-gray-400">
                      No defects found.
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
