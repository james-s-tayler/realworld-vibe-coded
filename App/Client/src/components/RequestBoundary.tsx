import React from 'react';
import { ErrorDisplay } from './ErrorDisplay';
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
  return (
    <>
      <ErrorDisplay error={error} onClose={clearError} />
      {loading ? (
        <p data-testid="loading-message">{loadingMessage ?? 'Loading...'}</p>
      ) : (
        children
      )}
    </>
  );
};
