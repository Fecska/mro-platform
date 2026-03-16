import { useState } from 'react';
import { clsx } from 'clsx';
import { useExpiringCredentials } from '@/features/personnel/hooks/useExpiringCredentials';
import type { ExpiringCredentialDto } from '@/features/personnel/types';

type Window = 30 | 60 | 90;

const WINDOWS: Window[] = [30, 60, 90];

// ── Colour helpers ────────────────────────────────────────────────────────────

function rowClass(daysRemaining: number): string {
  if (daysRemaining <= 30) return 'bg-red-50';
  if (daysRemaining <= 60) return 'bg-amber-50';
  return '';
}

function badgeClass(daysRemaining: number): string {
  if (daysRemaining < 0)   return 'bg-red-100 text-red-700';
  if (daysRemaining <= 30) return 'bg-red-100 text-red-700';
  if (daysRemaining <= 60) return 'bg-amber-100 text-amber-700';
  return 'bg-gray-100 text-gray-600';
}

function daysLabel(daysRemaining: number): string {
  if (daysRemaining < 0) return `${Math.abs(daysRemaining)}n lejárt`;
  if (daysRemaining === 0) return 'Ma jár le';
  return `${daysRemaining} nap`;
}

// ── Row ───────────────────────────────────────────────────────────────────────

function CredentialRow({ item }: { item: ExpiringCredentialDto }) {
  return (
    <tr className={clsx('border-b border-gray-100 last:border-0', rowClass(item.daysRemaining))}>
      <td className="py-2.5 px-3 text-sm font-medium text-gray-900 whitespace-nowrap">
        {item.fullName}
        <span className="ml-1.5 text-xs text-gray-400 font-normal">{item.employeeNumber}</span>
      </td>
      <td className="py-2.5 px-3 text-xs">
        <span className={clsx(
          'inline-block px-2 py-0.5 rounded-full font-medium',
          item.credentialType === 'Licence'
            ? 'bg-blue-100 text-blue-700'
            : 'bg-purple-100 text-purple-700',
        )}>
          {item.credentialType === 'Licence' ? 'Licenc' : 'Jogosítás'}
        </span>
      </td>
      <td className="py-2.5 px-3 text-sm text-gray-700 font-mono">{item.identifier}</td>
      <td className="py-2.5 px-3 text-sm text-gray-700">{item.category}</td>
      <td className="py-2.5 px-3 text-sm text-gray-600 whitespace-nowrap">{item.expiresOn}</td>
      <td className="py-2.5 px-3 text-right">
        <span className={clsx('inline-block px-2 py-0.5 rounded-full text-xs font-semibold', badgeClass(item.daysRemaining))}>
          {daysLabel(item.daysRemaining)}
        </span>
      </td>
    </tr>
  );
}

// ── Skeleton ──────────────────────────────────────────────────────────────────

function SkeletonRows() {
  return (
    <>
      {Array.from({ length: 4 }).map((_, i) => (
        <tr key={i} className="border-b border-gray-100">
          {Array.from({ length: 6 }).map((_, j) => (
            <td key={j} className="py-3 px-3">
              <div className="h-3 bg-gray-200 rounded animate-pulse" style={{ width: `${60 + (j * 10) % 40}%` }} />
            </td>
          ))}
        </tr>
      ))}
    </>
  );
}

// ── Card ──────────────────────────────────────────────────────────────────────

export function ExpiringCredentialsCard() {
  const [window, setWindow] = useState<Window>(30);
  const { data, isLoading, isError } = useExpiringCredentials(window);

  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100">
        <div>
          <h2 className="text-sm font-semibold text-gray-900">Lejáró jogosultságok</h2>
          {data && (
            <p className="text-xs text-gray-500 mt-0.5">
              {data.expiredCount > 0 && (
                <span className="text-red-600 font-medium">{data.expiredCount} lejárt</span>
              )}
              {data.expiredCount > 0 && data.totalCount - data.expiredCount > 0 && ' · '}
              {data.totalCount - data.expiredCount > 0 && (
                <span>{data.totalCount - data.expiredCount} hamarosan lejár</span>
              )}
              {data.totalCount === 0 && (
                <span className="text-green-600">Nincs lejáró jogosultság</span>
              )}
            </p>
          )}
        </div>

        {/* 30 / 60 / 90 day toggle */}
        <div className="flex rounded-lg border border-gray-200 overflow-hidden text-xs">
          {WINDOWS.map((w) => (
            <button
              key={w}
              onClick={() => setWindow(w)}
              className={clsx(
                'px-3 py-1.5 font-medium transition-colors',
                w === window
                  ? 'bg-gray-900 text-white'
                  : 'bg-white text-gray-600 hover:bg-gray-50',
              )}
            >
              {w}n
            </button>
          ))}
        </div>
      </div>

      {/* Table */}
      {isError ? (
        <div className="px-4 py-8 text-center text-sm text-red-500">
          Nem sikerült betölteni az adatokat.
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead>
              <tr className="bg-gray-50 text-xs font-medium text-gray-500 uppercase tracking-wide">
                <th className="py-2 px-3">Alkalmazott</th>
                <th className="py-2 px-3">Típus</th>
                <th className="py-2 px-3">Azonosító</th>
                <th className="py-2 px-3">Kategória</th>
                <th className="py-2 px-3">Lejárat</th>
                <th className="py-2 px-3 text-right">Hátralévő</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <SkeletonRows />
              ) : data && data.items.length > 0 ? (
                data.items.map((item) => (
                  <CredentialRow
                    key={`${item.employeeId}-${item.credentialType}-${item.identifier}`}
                    item={item}
                  />
                ))
              ) : (
                <tr>
                  <td colSpan={6} className="py-10 text-center text-sm text-gray-400">
                    Nincs lejáró jogosultság a következő {window} napban.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
