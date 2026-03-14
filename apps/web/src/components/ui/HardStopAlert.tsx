/**
 * Hard stop alert component.
 * Renders a prominent compliance block message.
 * Must not be styled to look less serious — this is a regulatory block.
 *
 * Reference: docs/compliance/hard-stop-rules.md
 */
interface HardStopAlertProps {
  ruleId: string;
  message: string;
  onDismiss?: () => void;
}

export function HardStopAlert({ ruleId, message, onDismiss }: HardStopAlertProps) {
  return (
    <div
      role="alert"
      aria-live="assertive"
      className="hard-stop-alert flex items-start gap-3"
    >
      <span className="text-xl shrink-0" aria-hidden>⛔</span>
      <div className="flex-1">
        <p className="font-bold text-sm text-red-900 mb-0.5">
          Compliance Block — {ruleId}
        </p>
        <p className="text-sm text-red-800">{message}</p>
      </div>
      {onDismiss && (
        <button
          onClick={onDismiss}
          className="shrink-0 text-red-500 hover:text-red-700 text-lg leading-none"
          aria-label="Dismiss"
        >
          ×
        </button>
      )}
    </div>
  );
}
