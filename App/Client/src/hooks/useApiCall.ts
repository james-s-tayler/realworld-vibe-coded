import { useState, useCallback } from 'react';
import { type AppError, normalizeError } from '../utils/errors';

export interface UseApiCallResult<T> {
  /** The data returned from the API call */
  data: T | null;
  /** The normalized error if the call failed */
  error: AppError | null;
  /** Whether the call is in progress */
  loading: boolean;
  /** Execute the API call */
  execute: (...args: unknown[]) => Promise<T | null>;
  /** Clear the error state */
  clearError: () => void;
  /** Reset all state (data, error, loading) */
  reset: () => void;
}

/**
 * A reusable hook for making API calls with automatic error handling.
 * 
 * This hook encapsulates the common pattern of:
 * - Managing loading state
 * - Catching and normalizing errors
 * - Storing the result data
 * 
 * @example
 * // Basic usage
 * const { data, error, loading, execute, clearError } = useApiCall(
 *   async () => await articlesApi.createArticle(articleData)
 * );
 * 
 * // Execute the call
 * const result = await execute();
 * if (result) {
 *   navigate(`/article/${result.article.slug}`);
 * }
 * 
 * @example
 * // With onSuccess callback
 * const { error, loading, execute, clearError } = useApiCall(
 *   async () => await login(email, password),
 *   { onSuccess: () => navigate('/') }
 * );
 */
export function useApiCall<T>(
  apiFunction: (...args: unknown[]) => Promise<T>,
  options?: {
    /** Called when the API call succeeds */
    onSuccess?: (data: T) => void;
    /** Called when the API call fails */
    onError?: (error: AppError) => void;
  }
): UseApiCallResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [error, setError] = useState<AppError | null>(null);
  const [loading, setLoading] = useState(false);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const reset = useCallback(() => {
    setData(null);
    setError(null);
    setLoading(false);
  }, []);

  const execute = useCallback(
    async (...args: unknown[]): Promise<T | null> => {
      setError(null);
      setLoading(true);

      try {
        const result = await apiFunction(...args);
        setData(result);
        options?.onSuccess?.(result);
        return result;
      } catch (err) {
        const normalizedError = normalizeError(err);
        setError(normalizedError);
        options?.onError?.(normalizedError);
        return null;
      } finally {
        setLoading(false);
      }
    },
    [apiFunction, options]
  );

  return {
    data,
    error,
    loading,
    execute,
    clearError,
    reset,
  };
}
