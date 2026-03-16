import { useState } from 'react';
import { clsx } from 'clsx';
import { useEmployeeList, useEmployeeDetail, useEmployeeCurrency } from '@/features/personnel/hooks/usePersonnel';
import type { EmployeeStatus, CurrencyStatus, EmployeeSummary, Licence, Authorisation } from '@/features/personnel/types';
import { PageHeader } from '@/components/common/PageHeader';
import { SkeletonTable } from '@/components/common/SkeletonTable';

// ── Currency dot ──────────────────────────────────────────────────────────────

const CURRENCY_CONFIG: Record<CurrencyStatus, { dot: string; label: string; text: string }> = {
  Green: { dot: 'bg-green-500',  label: 'Current',  text: 'text-green-700' },
  Amber: { dot: 'bg-amber-400',  label: 'Expiring', text: 'text-amber-700' },
  Red:   { dot: 'bg-red-500',    label: 'Non-Current', text: 'text-red-700' },
};

function CurrencyDot({ status }: { status: CurrencyStatus }) {
  const cfg = CURRENCY_CONFIG[status];
  return (
    <span className="inline-flex items-center gap-1.5">
      <span className={clsx('w-2.5 h-2.5 rounded-full shrink-0 ring-2 ring-white', cfg.dot)} />
      <span className={clsx('text-xs font-medium', cfg.text)}>{cfg.label}</span>
    </span>
  );
}

// ── Employee status badge ─────────────────────────────────────────────────────

const EMP_STATUS: Record<EmployeeStatus, string> = {
  Active:     'bg-green-100 text-green-700',
  Inactive:   'bg-gray-100 text-gray-600',
  Suspended:  'bg-red-100 text-red-700',
  Terminated: 'bg-gray-100 text-gray-400',
};

// ── Detail panel ──────────────────────────────────────────────────────────────

function LicenceRow({ lic }: { lic: Licence }) {
  return (
    <div className={clsx(
      'flex items-start justify-between py-2 px-3 rounded-lg text-sm',
      lic.is_expired ? 'bg-red-50' : 'bg-gray-50',
    )}>
      <div>
        <p className="font-mono font-medium text-gray-900">{lic.licence_number}</p>
        <p className="text-xs text-gray-500 mt-0.5">{lic.issuing_authority} · {lic.category}{lic.subcategory ? ` / ${lic.subcategory}` : ''}</p>
      </div>
      <div className="text-right shrink-0 ml-3">
        {lic.expires_at ? (
          <span className={clsx('text-xs font-medium', lic.is_expired ? 'text-red-600' : 'text-gray-600')}>
            {lic.is_expired ? 'Expired ' : 'Exp. '}
            {new Date(lic.expires_at).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })}
          </span>
        ) : (
          <span className="text-xs text-gray-400">No expiry</span>
        )}
      </div>
    </div>
  );
}

function AuthRow({ auth }: { auth: Authorisation }) {
  return (
    <div className={clsx(
      'flex items-start justify-between py-2 px-3 rounded-lg text-sm',
      auth.is_expired || auth.status === 'Suspended' ? 'bg-red-50' : 'bg-gray-50',
    )}>
      <div>
        <p className="font-mono font-medium text-gray-900">{auth.authorisation_number}</p>
        <p className="text-xs text-gray-500 mt-0.5">{auth.category} · {auth.scope}</p>
      </div>
      <div className="text-right shrink-0 ml-3">
        {auth.status === 'Suspended' && (
          <span className="text-xs font-semibold text-red-600">Suspended</span>
        )}
        {auth.valid_until && auth.status !== 'Suspended' ? (
          <span className={clsx('text-xs font-medium', auth.is_expired ? 'text-red-600' : 'text-gray-600')}>
            {auth.is_expired ? 'Expired ' : 'Valid to '}
            {new Date(auth.valid_until).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })}
          </span>
        ) : !auth.is_expired && auth.status !== 'Suspended' ? (
          <span className="text-xs text-gray-400">Open-ended</span>
        ) : null}
      </div>
    </div>
  );
}

function DetailPanel({ employeeId, onClose }: { employeeId: string; onClose: () => void }) {
  const { data: emp, isLoading: empLoading } = useEmployeeDetail(employeeId);
  const { data: currency, isLoading: currLoading } = useEmployeeCurrency(employeeId);

  const isLoading = empLoading || currLoading;

  return (
    <div className="fixed inset-0 z-40 flex justify-end" onClick={onClose}>
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/20 backdrop-blur-[1px]" />

      {/* Panel */}
      <div
        className="relative z-50 w-full max-w-md bg-white shadow-2xl flex flex-col h-full overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-gray-200 bg-gray-50">
          {isLoading ? (
            <div className="h-5 w-40 bg-gray-200 rounded animate-pulse" />
          ) : emp ? (
            <div>
              <p className="font-bold text-gray-900">{emp.full_name}</p>
              <p className="text-xs text-gray-500 font-mono mt-0.5">{emp.employee_number} · {emp.email}</p>
            </div>
          ) : null}
          <button
            onClick={onClose}
            className="ml-3 shrink-0 w-8 h-8 flex items-center justify-center rounded-lg hover:bg-gray-200 text-gray-500 transition-colors"
            aria-label="Close"
          >
            ✕
          </button>
        </div>

        {isLoading ? (
          <div className="flex-1 p-5 space-y-3">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="h-10 bg-gray-100 rounded-lg animate-pulse" />
            ))}
          </div>
        ) : emp ? (
          <div className="flex-1 p-5 space-y-6">

            {/* Currency status */}
            {currency && (
              <div>
                <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2">Currency Status</p>
                <div className={clsx(
                  'rounded-xl p-4 border',
                  currency.status === 'Green' ? 'bg-green-50 border-green-200' :
                  currency.status === 'Amber' ? 'bg-amber-50 border-amber-200' :
                  'bg-red-50 border-red-200',
                )}>
                  <div className="flex items-center gap-2 mb-2">
                    <span className={clsx(
                      'w-3 h-3 rounded-full',
                      currency.status === 'Green' ? 'bg-green-500' :
                      currency.status === 'Amber' ? 'bg-amber-400' : 'bg-red-500',
                    )} />
                    <span className="font-semibold text-sm text-gray-900">
                      {CURRENCY_CONFIG[currency.status].label}
                    </span>
                  </div>
                  {currency.red_reasons.length > 0 && (
                    <ul className="space-y-1">
                      {currency.red_reasons.map((r, i) => (
                        <li key={i} className="text-xs text-red-700 flex gap-1.5">
                          <span className="shrink-0 mt-0.5">⚠</span>{r}
                        </li>
                      ))}
                    </ul>
                  )}
                  {currency.amber_reasons.length > 0 && (
                    <ul className="space-y-1 mt-1">
                      {currency.amber_reasons.map((r, i) => (
                        <li key={i} className="text-xs text-amber-700 flex gap-1.5">
                          <span className="shrink-0 mt-0.5">⚡</span>{r}
                        </li>
                      ))}
                    </ul>
                  )}
                </div>
              </div>
            )}

            {/* Licences */}
            <div>
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2">
                Licences ({emp.licences.length})
              </p>
              {emp.licences.length > 0 ? (
                <div className="space-y-1.5">
                  {emp.licences.map((lic) => <LicenceRow key={lic.id} lic={lic} />)}
                </div>
              ) : (
                <p className="text-sm text-gray-400 italic">No licences recorded.</p>
              )}
            </div>

            {/* Authorisations */}
            <div>
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2">
                Authorisations ({emp.authorisations.length})
              </p>
              {emp.authorisations.length > 0 ? (
                <div className="space-y-1.5">
                  {emp.authorisations.map((auth) => <AuthRow key={auth.id} auth={auth} />)}
                </div>
              ) : (
                <p className="text-sm text-gray-400 italic">No authorisations granted.</p>
              )}
            </div>
          </div>
        ) : (
          <div className="flex-1 flex items-center justify-center text-sm text-red-500">
            Failed to load employee details.
          </div>
        )}
      </div>
    </div>
  );
}

// ── Row (with currency — uses separate hook per row, React Query caches it) ───

function EmployeeRow({
  emp,
  isSelected,
  onClick,
}: {
  emp: EmployeeSummary;
  isSelected: boolean;
  onClick: () => void;
}) {
  const { data: currency } = useEmployeeCurrency(emp.id);
  const statusBadge = EMP_STATUS[emp.status] ?? EMP_STATUS.Inactive;

  return (
    <tr
      className={clsx(
        'border-b border-gray-100 cursor-pointer transition-colors',
        isSelected ? 'bg-blue-50' : 'hover:bg-gray-50',
      )}
      onClick={onClick}
    >
      <td className="py-3 px-4">
        <span className="font-mono text-xs text-gray-500">{emp.employee_number}</span>
      </td>
      <td className="py-3 px-4">
        <p className="text-sm font-semibold text-gray-900">{emp.full_name}</p>
        <p className="text-xs text-gray-400 mt-0.5">{emp.email}</p>
      </td>
      <td className="py-3 px-4">
        <span className={clsx('inline-block px-2.5 py-0.5 rounded-full text-xs font-medium', statusBadge)}>
          {emp.status}
        </span>
      </td>
      <td className="py-3 px-4 text-center">
        {currency ? (
          <CurrencyDot status={currency.status} />
        ) : (
          <span className="w-2.5 h-2.5 rounded-full bg-gray-200 inline-block animate-pulse" />
        )}
      </td>
    </tr>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────

const STATUS_FILTERS: { value: EmployeeStatus | ''; label: string }[] = [
  { value: '',           label: 'All'       },
  { value: 'Active',     label: 'Active'    },
  { value: 'Inactive',   label: 'Inactive'  },
  { value: 'Suspended',  label: 'Suspended' },
];

export default function PersonnelPage() {
  const [statusFilter, setStatusFilter] = useState<EmployeeStatus | ''>('Active');
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data, isLoading, isError } = useEmployeeList({
    ...(statusFilter ? { status: statusFilter } : {}),
    page_size: 50,
  });

  const total = data?.meta.total ?? 0;

  return (
    <div>
      <PageHeader
        title="Personnel"
        subtitle={isLoading ? 'Loading…' : `${total} employees`}
      />

      <div className="flex items-center gap-2 mb-4 flex-wrap">
        {STATUS_FILTERS.map(({ value, label }) => (
          <button
            key={value}
            onClick={() => setStatusFilter(value as EmployeeStatus | '')}
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

        {/* Currency legend */}
        <div className="ml-auto flex items-center gap-4 text-xs text-gray-500">
          {(['Green', 'Amber', 'Red'] as CurrencyStatus[]).map((s) => (
            <span key={s} className="flex items-center gap-1.5">
              <span className={clsx('w-2 h-2 rounded-full', CURRENCY_CONFIG[s].dot)} />
              {CURRENCY_CONFIG[s].label}
            </span>
          ))}
        </div>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        {isError ? (
          <div className="px-6 py-12 text-center text-sm text-red-500">
            Failed to load personnel data.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-200 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                  <th className="py-3 px-4">Emp #</th>
                  <th className="py-3 px-4">Name</th>
                  <th className="py-3 px-4">Status</th>
                  <th className="py-3 px-4 text-center">Currency</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  <SkeletonTable rows={6} cols={4} />
                ) : data && data.data.length > 0 ? (
                  data.data.map((emp) => (
                    <EmployeeRow
                      key={emp.id}
                      emp={emp}
                      isSelected={selectedId === emp.id}
                      onClick={() => setSelectedId(emp.id === selectedId ? null : emp.id)}
                    />
                  ))
                ) : (
                  <tr>
                    <td colSpan={4} className="py-12 text-center text-sm text-gray-400">
                      No employees found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}

        {data && data.data.length > 0 && (
          <div className="px-4 py-2.5 border-t border-gray-100 text-xs text-gray-400">
            Showing {data.data.length} of {data.meta.total} · Click a row to see details
          </div>
        )}
      </div>

      {/* Detail panel */}
      {selectedId && (
        <DetailPanel
          employeeId={selectedId}
          onClose={() => setSelectedId(null)}
        />
      )}
    </div>
  );
}
