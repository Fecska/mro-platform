import type { Config } from 'tailwindcss';

export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        // MRO status colours — derived from operational convention, not competitor UI
        status: {
          pending:    '#f59e0b',  // amber
          active:     '#3b82f6',  // blue
          complete:   '#22c55e',  // green
          blocked:    '#ef4444',  // red
          deferred:   '#a855f7',  // purple
          cancelled:  '#6b7280',  // gray
        },
        // Severity colours
        severity: {
          critical: '#dc2626',
          high:     '#ea580c',
          medium:   '#ca8a04',
          low:      '#16a34a',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        mono: ['JetBrains Mono', 'Consolas', 'monospace'],
      },
    },
  },
  plugins: [],
} satisfies Config;
