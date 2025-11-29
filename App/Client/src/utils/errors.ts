import { ApiError } from '../api/client';

export type ErrorType = 'validation' | 'network' | 'auth' | 'server' | 'general';

export interface AppError {
  type: ErrorType;
  title: string;
  messages: string[];
  status?: number;
  /** Original error for logging/debugging */
  cause?: unknown;
}

function getErrorTypeFromStatus(status: number): ErrorType {
  if (status === 401 || status === 403) return 'auth';
  if (status === 400 || status === 422) return 'validation';
  if (status >= 500) return 'server';
  return 'general';
}

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

export function normalizeError(error: unknown): AppError {
  // ApiError from the API client
  if (error instanceof ApiError) {
    const type = getErrorTypeFromStatus(error.status);
    // Use API-provided title if it's not the default 'Error', otherwise use type-based default
    const title = (error.title && error.title !== 'Error') ? error.title : getDefaultTitle(type);
    return {
      type,
      title,
      messages: error.errors,
      status: error.status,
      cause: error,
    };
  }

  // Built-in JS Error (network, coding bug, etc.)
  if (error instanceof Error) {
    return {
      type: 'general',
      title: 'Error',
      messages: [error.message],
      cause: error,
    };
  }

  // Strings
  if (typeof error === 'string') {
    return {
      type: 'general',
      title: 'Error',
      messages: [error],
      cause: error,
    };
  }

  // Arrays of strings
  if (Array.isArray(error) && error.every((m) => typeof m === 'string')) {
    return {
      type: 'general',
      title: 'Error',
      messages: error as string[],
      cause: error,
    };
  }

  // Fallback
  return {
    type: 'general',
    title: 'Error',
    messages: ['An unexpected error occurred'],
    cause: error,
  };
}
