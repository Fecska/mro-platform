import { clsx } from 'clsx';
import type { WorkOrderStatus, DefectStatus, ReleaseStatus, AircraftStatus } from '@/types/common';

type AnyStatus = WorkOrderStatus | DefectStatus | ReleaseStatus | AircraftStatus | string;

const STATUS_STYLES: Record<string, string> = {
  // Aircraft
  Active:         'bg-green-100 text-green-700',
  Grounded:       'bg-red-100 text-red-700',
  InMaintenance:  'bg-yellow-100 text-yellow-800',
  Withdrawn:      'bg-gray-200 text-gray-600',
  WrittenOff:     'bg-gray-300 text-gray-500 line-through',

  // Work order
  draft:                   'bg-gray-100 text-gray-700',
  planned:                 'bg-blue-100 text-blue-700',
  issued:                  'bg-indigo-100 text-indigo-700',
  in_progress:             'bg-yellow-100 text-yellow-800',
  waiting_parts:           'bg-orange-100 text-orange-700',
  waiting_tooling:         'bg-orange-100 text-orange-700',
  waiting_inspection:      'bg-purple-100 text-purple-700',
  waiting_certification:   'bg-purple-100 text-purple-700',
  completed:               'bg-green-100 text-green-700',
  closed:                  'bg-gray-200 text-gray-600',
  cancelled:               'bg-gray-100 text-gray-500 line-through',

  // Defect
  reported:                'bg-red-100 text-red-700',
  triaged:                 'bg-orange-100 text-orange-700',
  open:                    'bg-red-100 text-red-800',
  deferred:                'bg-purple-100 text-purple-700',
  rectification_in_progress: 'bg-yellow-100 text-yellow-800',
  inspection_pending:      'bg-blue-100 text-blue-700',
  cleared:                 'bg-green-100 text-green-700',

  // Release (note: 'issued' is shared with work order above — same style)
  not_required:            'bg-gray-100 text-gray-500',
  required:                'bg-red-100 text-red-700',
  signoff_pending:         'bg-purple-100 text-purple-700',
  superseded:              'bg-gray-100 text-gray-500',
  revoked:                 'bg-red-200 text-red-800',
};

const STATUS_LABELS: Record<string, string> = {
  in_progress:             'In Progress',
  waiting_parts:           'Waiting Parts',
  waiting_tooling:         'Waiting Tooling',
  waiting_inspection:      'Waiting Inspection',
  waiting_certification:   'Waiting Cert.',
  rectification_in_progress: 'Rectification',
  not_required:            'Not Required',
  signoff_pending:         'Sign-off Pending',
};

interface StatusBadgeProps {
  status: AnyStatus;
  className?: string;
}

export function StatusBadge({ status, className }: StatusBadgeProps) {
  const style = STATUS_STYLES[status] ?? 'bg-gray-100 text-gray-600';
  const label = STATUS_LABELS[status] ?? status.replace(/_/g, ' ').replace(/\b\w/g, (c) => c.toUpperCase());

  return (
    <span className={clsx('status-badge', style, className)}>
      {label}
    </span>
  );
}
