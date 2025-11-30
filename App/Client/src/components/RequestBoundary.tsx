import React, { useEffect, useRef } from 'react';
import { useToast } from '../hooks/useToast';
import { type AppError } from '../utils/errors';

interface RequestBoundaryProps {
  error: AppError | null;
  clearError: () => void;
  loading?: boolean;
  loadingMessage?: string;
  children: React.ReactNode;
}

export const RequestBoundary: React.FC<RequestBoundaryProps> = ({
  error,
  clearError,
  loading = false,
  loadingMessage,
  children,
}) => {
  const { showError } = useToast();
  const lastErrorRef = useRef<AppError | null>(null);

  // Show toast when error changes (and is not null)
  useEffect(() => {
    if (error && error !== lastErrorRef.current) {
      showError(error);
      lastErrorRef.current = error;
      // Clear the error from the component state after showing toast
      clearError();
    }
  }, [error, showError, clearError]);

  // Reset lastErrorRef when error is cleared externally
  useEffect(() => {
    if (!error) {
      lastErrorRef.current = null;
    }
  }, [error]);

  return (
    <>
      {loading ? (
        <p data-testid="loading-message">{loadingMessage ?? 'Loading...'}</p>
      ) : (
        children
      )}
    </>
  );
};

