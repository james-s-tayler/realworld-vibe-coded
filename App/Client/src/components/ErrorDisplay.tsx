import { useEffect, useRef } from 'react';

import { useToast } from '../hooks/useToast';
import { type AppError, normalizeError } from '../utils/errors';

export interface ErrorDisplayProps {
  error: AppError | unknown | null;
  onClose?: () => void;
}

export const ErrorDisplay: React.FC<ErrorDisplayProps> = ({
  error,
  onClose,
}) => {
  const { showToast, removeToast } = useToast();
  const toastIdRef = useRef<string | null>(null);

  useEffect(() => {
    if (!error) {
      if (toastIdRef.current) {
        removeToast(toastIdRef.current);
        toastIdRef.current = null;
      }
      return;
    }

    const appError: AppError = isAppError(error) ? error : normalizeError(error);

    if (appError.messages.length === 0) return;

    const subtitle = appError.messages.join(', ');
    toastIdRef.current = showToast({
      kind: 'error',
      title: appError.title,
      subtitle,
    });

    return () => {
      if (toastIdRef.current) {
        removeToast(toastIdRef.current);
        toastIdRef.current = null;
      }
      onClose?.();
    };
  }, [error, showToast, removeToast, onClose]);

  return null;
};

function isAppError(error: unknown): error is AppError {
  if (typeof error !== 'object' || error === null) {
    return false;
  }

  const candidate = error as Record<string, unknown>;
  return (
    'type' in candidate &&
    'title' in candidate &&
    'messages' in candidate &&
    Array.isArray(candidate.messages)
  );
}
