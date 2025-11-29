import React from 'react';
import { InlineNotification } from '@carbon/react';
import { type AppError, normalizeError } from '../utils/errors';

export interface ErrorDisplayProps {
  /** The error to display - can be any error type that normalizeError handles, or a pre-normalized AppError */
  error: AppError | unknown | null;
  /** Callback when the close button is clicked */
  onClose?: () => void;
  /** Whether to show the notification (defaults to true if error is provided) */
  show?: boolean;
  /** Custom styles for the notification */
  style?: React.CSSProperties;
}

/**
 * A reusable error display component for showing error messages throughout the application.
 * 
 * Features:
 * - Accepts any error type and normalizes it to AppError format
 * - Uses title from ProblemDetails when available (from ApiError)
 * - Falls back to auto-detected titles based on error type
 * 
 * @example
 * // Basic usage - pass any error
 * <ErrorDisplay error={error} onClose={() => setError(null)} />
 * 
 * @example
 * // With pre-normalized AppError
 * <ErrorDisplay error={normalizeError(err)} onClose={() => setError(null)} />
 */
export const ErrorDisplay: React.FC<ErrorDisplayProps> = ({
  error,
  onClose,
  show = true,
  style,
}) => {
  // Don't render if there's no error or show is false
  if (!error || !show) {
    return null;
  }

  // Normalize the error - if it's already an AppError, normalizeError handles it
  const appError: AppError = isAppError(error) ? error : normalizeError(error);

  // Don't render if there are no meaningful messages
  if (appError.messages.length === 0) {
    return null;
  }

  // Format the subtitle - join multiple errors with commas
  const subtitle = appError.messages.join(', ');

  return (
    <InlineNotification
      kind="error"
      title={appError.title}
      subtitle={subtitle}
      onCloseButtonClick={onClose}
      style={{ marginBottom: '1rem', ...style }}
      data-testid="error-display"
      aria-live="polite"
    />
  );
};

/** Type guard to check if an error is already a normalized AppError */
function isAppError(error: unknown): error is AppError {
  return (
    typeof error === 'object' &&
    error !== null &&
    'type' in error &&
    'title' in error &&
    'messages' in error &&
    Array.isArray((error as AppError).messages)
  );
}
