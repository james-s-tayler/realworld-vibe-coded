import React from 'react';
import { InlineNotification } from '@carbon/react';
import { ApiError } from '../api/client';

export type ErrorType = 'validation' | 'network' | 'auth' | 'server' | 'general';

export interface ErrorDisplayProps {
  /** The error to display - can be an ApiError, Error, string, or string array */
  error: ApiError | Error | string | string[] | null;
  /** Title to display in the notification */
  title?: string;
  /** Callback when the close button is clicked */
  onClose?: () => void;
  /** Whether to show the notification (defaults to true if error is provided) */
  show?: boolean;
  /** Custom styles for the notification */
  style?: React.CSSProperties;
  /** The type of error for proper styling (auto-detected if not provided) */
  errorType?: ErrorType;
}

/**
 * Determines the error type from an ApiError based on status code
 */
function getErrorTypeFromStatus(status: number): ErrorType {
  if (status === 401 || status === 403) {
    return 'auth';
  }
  if (status === 422 || status === 400) {
    return 'validation';
  }
  if (status >= 500) {
    return 'server';
  }
  return 'general';
}

/**
 * Gets a human-readable title for the error type
 */
function getDefaultTitle(errorType: ErrorType): string {
  switch (errorType) {
    case 'validation':
      return 'Validation Error';
    case 'network':
      return 'Network Error';
    case 'auth':
      return 'Authentication Error';
    case 'server':
      return 'Server Error';
    case 'general':
    default:
      return 'Error';
  }
}

/**
 * Extracts error messages from various error formats
 */
function extractErrorMessages(error: ApiError | Error | string | string[]): string[] {
  if (typeof error === 'string') {
    return [error];
  }
  
  if (Array.isArray(error)) {
    return error;
  }
  
  if (error instanceof ApiError) {
    return error.errors;
  }
  
  if (error instanceof Error) {
    return [error.message];
  }
  
  return ['An unexpected error occurred'];
}

/**
 * A reusable error display component for showing error messages throughout the application.
 * 
 * Features:
 * - Handles ApiError, Error, string, and string array error formats
 * - Auto-detects error type from ApiError status codes
 * - Provides appropriate titles and styling based on error type
 * - Supports custom titles and close callbacks
 * 
 * @example
 * // Basic usage with ApiError
 * <ErrorDisplay error={error} onClose={() => setError(null)} />
 * 
 * @example
 * // With custom title
 * <ErrorDisplay error={error} title="Registration Failed" onClose={() => setError(null)} />
 * 
 * @example
 * // With string error
 * <ErrorDisplay error="Something went wrong" />
 */
export const ErrorDisplay: React.FC<ErrorDisplayProps> = ({
  error,
  title,
  onClose,
  show = true,
  style,
  errorType,
}) => {
  // Don't render if there's no meaningful error content or show is false
  const hasError = error && 
    !(typeof error === 'string' && !error) && 
    !(Array.isArray(error) && error.length === 0);
  
  if (!hasError || !show) {
    return null;
  }

  // Extract error messages
  const messages = extractErrorMessages(error);
  
  // Determine error type
  let detectedErrorType: ErrorType = errorType ?? 'general';
  if (!errorType && error instanceof ApiError) {
    detectedErrorType = getErrorTypeFromStatus(error.status);
  }
  
  // Use provided title or generate default based on error type
  const displayTitle = title ?? getDefaultTitle(detectedErrorType);
  
  // Format the subtitle - join multiple errors with commas
  const subtitle = messages.join(', ');

  return (
    <InlineNotification
      kind="error"
      title={displayTitle}
      subtitle={subtitle}
      onCloseButtonClick={onClose}
      style={{ marginBottom: '1rem', ...style }}
      data-testid="error-display"
      aria-live="polite"
    />
  );
};
