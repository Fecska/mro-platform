interface SkeletonTableProps {
  rows?: number;
  cols?: number;
}

export function SkeletonTable({ rows = 5, cols = 5 }: SkeletonTableProps) {
  return (
    <>
      {Array.from({ length: rows }).map((_, i) => (
        <tr key={i} className="border-b border-gray-100">
          {Array.from({ length: cols }).map((_, j) => (
            <td key={j} className="py-3 px-4">
              <div
                className="h-3.5 bg-gray-100 rounded animate-pulse"
                style={{ width: `${50 + ((i + j) * 17) % 40}%` }}
              />
            </td>
          ))}
        </tr>
      ))}
    </>
  );
}
