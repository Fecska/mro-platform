import { useState } from 'react';
import { clsx } from 'clsx';
import { useAircraftList } from '@/features/aircraft/hooks/useAircraft';
import type { AircraftStatus } from '@/types/common';
import type { Aircraft } from '@/features/aircraft/types';
import { PageHeader } from '@/components/common/PageHeader';
import { SkeletonTable } from '@/components/common/SkeletonTable';

// ── Status config ─────────────────────────────────────────────────────────────

const STATUS_CONFIG: Record<AircraftStatus, { label: string; dot: string; badge: string }> = {
  Active:        { label: 'Active',         dot: 'bg-green-500', badge: 'bg-green-100 text-green-700' },
  InMaintenance: { label: 'In Maintenance', dot: 'bg-amber-500', badge: 'bg-amber-100 text-amber-700' },
  Grounded:      { label: 'Grounded',       dot: 'bg-red-500',   badge: 'bg-red-100 text-red-700'    },
  Withdrawn:     { label: 'Withdrawn',      dot: 'bg-gray-400',  badge: 'bg-gray-100 text-gray-600'  },
  WrittenOff:    { label: 'Written Off',    dot: 'bg-gray-300',  badge: 'bg-gray-100 text-gray-400'  },
};

const STATUS_FILTERS: { value: AircraftStatus | ''; label: string }[] = [
  { value: '',              label: 'All'           },
  { value: 'Active',        label: 'Active'        },
  { value: 'InMaintenance', label: 'In Maintenance'},
  { value: 'Grounded',      label: 'Grounded'      },
  { value: 'Withdrawn',     label: 'Withdrawn'     },
];

// ── Row ───────────────────────────────────────────────────────────────────────

function AircraftRow({ aircraft }: { aircraft: Aircraft }) {
  const cfg = STATUS_CONFIG[aircraft.status] ?? STATUS_CONFIG.Withdrawn;
  return (
    <tr className="border-b border-gray-100 hover:bg-gray-50 transition-colors">
      <td className="py-3 px-4">
        <span className="font-mono font-bold text-gray-900 text-sm">{aircraft.registration}</span>
      </td>
      <td className="py-3 px-4 text-sm text-gray-700">{aircraft.aircraft_type}</td>
      <td className="py-3 px-4 text-sm text-gray-500 font-mono">{aircraft.serial_number}</td>
      <td className="py-3 px-4 text-sm text-gray-600">{aircraft.manufacturer}</td>
      <td className="py-3 px-4">
        <span className={clsx('inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium', cfg.badge)}>
          <span className={clsx('w-1.5 h-1.5 rounded-full shrink-0', cfg.dot)} />
          {cfg.label}
        </span>
      </td>
      <td className="py-3 px-4 text-sm text-gray-400">
        {aircraft.manufacture_date ? new Date(aircraft.manufacture_date).getFullYear() : '—'}
      </td>
    </tr>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function AircraftPage() {
  const [statusFilter, setStatusFilter] = useState<AircraftStatus | ''>('');

  const { data, isLoading, isError } = useAircraftList({
    ...(statusFilter ? { status: statusFilter } : {}),
    page_size: 50,
  });

  const total = data?.meta.total ?? 0;

  return (
    <div>
      <PageHeader
        title="Aircraft"
        subtitle={isLoading ? 'Loading…' : `${total} aircraft in fleet`}
      />

      {/* Status filter bar */}
      <div className="flex items-center gap-2 mb-4 flex-wrap">
        {STATUS_FILTERS.map(({ value, label }) => (
          <button
            key={value}
            onClick={() => setStatusFilter(value as AircraftStatus | '')}
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

      {/* Table card */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        {isError ? (
          <div className="px-6 py-12 text-center text-sm text-red-500">
            Failed to load aircraft data.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-200 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                  <th className="py-3 px-4">Registration</th>
                  <th className="py-3 px-4">Type</th>
                  <th className="py-3 px-4">Serial No.</th>
                  <th className="py-3 px-4">Manufacturer</th>
                  <th className="py-3 px-4">Status</th>
                  <th className="py-3 px-4">Year</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  <SkeletonTable rows={6} cols={6} />
                ) : data && data.data.length > 0 ? (
                  data.data.map((ac) => <AircraftRow key={ac.id} aircraft={ac} />)
                ) : (
                  <tr>
                    <td colSpan={6} className="py-12 text-center text-sm text-gray-400">
                      No aircraft found.
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
