import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useApiCall } from './useApiCall';
import { ApiError } from '../api/client';

describe('useApiCall', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('initial state', () => {
    it('initializes with null data, null error, and loading false', () => {
      const mockApi = vi.fn();
      const { result } = renderHook(() => useApiCall(mockApi));

      expect(result.current.data).toBeNull();
      expect(result.current.error).toBeNull();
      expect(result.current.loading).toBe(false);
    });
  });

  describe('successful API calls', () => {
    it('sets loading to true during execution', async () => {
      let resolvePromise: (value: string) => void;
      const mockApi = vi.fn(
        () => new Promise<string>((resolve) => { resolvePromise = resolve; })
      );
      
      const { result } = renderHook(() => useApiCall(mockApi));

      act(() => {
        result.current.execute();
      });

      expect(result.current.loading).toBe(true);

      await act(async () => {
        resolvePromise!('success');
      });

      expect(result.current.loading).toBe(false);
    });

    it('returns data on successful API call', async () => {
      const mockData = { user: { username: 'test' } };
      const mockApi = vi.fn().mockResolvedValue(mockData);
      
      const { result } = renderHook(() => useApiCall(mockApi));

      await act(async () => {
        await result.current.execute();
      });

      expect(result.current.data).toEqual(mockData);
      expect(result.current.error).toBeNull();
    });

    it('calls onSuccess callback with data', async () => {
      const mockData = { user: { username: 'test' } };
      const mockApi = vi.fn().mockResolvedValue(mockData);
      const onSuccess = vi.fn();
      
      const { result } = renderHook(() => useApiCall(mockApi, { onSuccess }));

      await act(async () => {
        await result.current.execute();
      });

      expect(onSuccess).toHaveBeenCalledWith(mockData);
    });

    it('returns the result from execute', async () => {
      const mockData = { article: { slug: 'test-article' } };
      const mockApi = vi.fn().mockResolvedValue(mockData);
      
      const { result } = renderHook(() => useApiCall(mockApi));

      let executeResult: unknown;
      await act(async () => {
        executeResult = await result.current.execute();
      });

      expect(executeResult).toEqual(mockData);
    });
  });

  describe('failed API calls', () => {
    it('sets error on API failure', async () => {
      const mockApi = vi.fn().mockRejectedValue(new Error('Network error'));
      
      const { result } = renderHook(() => useApiCall(mockApi));

      await act(async () => {
        await result.current.execute();
      });

      expect(result.current.error).not.toBeNull();
      expect(result.current.error?.messages).toContain('Network error');
      expect(result.current.data).toBeNull();
    });

    it('normalizes ApiError correctly', async () => {
      const apiError = new ApiError(422, ['Title: is required', 'Body: is required'], 'Validation Error');
      const mockApi = vi.fn().mockRejectedValue(apiError);
      
      const { result } = renderHook(() => useApiCall(mockApi));

      await act(async () => {
        await result.current.execute();
      });

      expect(result.current.error?.type).toBe('validation');
      expect(result.current.error?.messages).toEqual(['Title: is required', 'Body: is required']);
      expect(result.current.error?.status).toBe(422);
    });

    it('calls onError callback with normalized error', async () => {
      const mockApi = vi.fn().mockRejectedValue(new Error('Something went wrong'));
      const onError = vi.fn();
      
      const { result } = renderHook(() => useApiCall(mockApi, { onError }));

      await act(async () => {
        await result.current.execute();
      });

      expect(onError).toHaveBeenCalled();
      expect(onError.mock.calls[0][0].messages).toContain('Something went wrong');
    });

    it('returns null from execute on failure', async () => {
      const mockApi = vi.fn().mockRejectedValue(new Error('Failed'));
      
      const { result } = renderHook(() => useApiCall(mockApi));

      let executeResult: unknown;
      await act(async () => {
        executeResult = await result.current.execute();
      });

      expect(executeResult).toBeNull();
    });

    it('sets loading to false after failure', async () => {
      const mockApi = vi.fn().mockRejectedValue(new Error('Failed'));
      
      const { result } = renderHook(() => useApiCall(mockApi));

      await act(async () => {
        await result.current.execute();
      });

      expect(result.current.loading).toBe(false);
    });
  });

  describe('clearError', () => {
    it('clears the error state', async () => {
      const mockApi = vi.fn().mockRejectedValue(new Error('Failed'));
      
      const { result } = renderHook(() => useApiCall(mockApi));

      await act(async () => {
        await result.current.execute();
      });

      expect(result.current.error).not.toBeNull();

      act(() => {
        result.current.clearError();
      });

      expect(result.current.error).toBeNull();
    });
  });

  describe('reset', () => {
    it('resets all state', async () => {
      const mockApi = vi.fn().mockResolvedValue({ data: 'test' });
      
      const { result } = renderHook(() => useApiCall(mockApi));

      await act(async () => {
        await result.current.execute();
      });

      expect(result.current.data).not.toBeNull();

      act(() => {
        result.current.reset();
      });

      expect(result.current.data).toBeNull();
      expect(result.current.error).toBeNull();
      expect(result.current.loading).toBe(false);
    });
  });

  describe('clears previous error on new call', () => {
    it('clears error when executing a new call', async () => {
      const mockApi = vi.fn()
        .mockRejectedValueOnce(new Error('First failure'))
        .mockResolvedValueOnce({ data: 'success' });
      
      const { result } = renderHook(() => useApiCall(mockApi));

      // First call fails
      await act(async () => {
        await result.current.execute();
      });
      expect(result.current.error).not.toBeNull();

      // Second call succeeds and clears error
      await act(async () => {
        await result.current.execute();
      });
      expect(result.current.error).toBeNull();
      expect(result.current.data).toEqual({ data: 'success' });
    });
  });

  describe('passes arguments to API function', () => {
    it('forwards arguments to the API function', async () => {
      const mockApi = vi.fn().mockResolvedValue({ success: true });
      
      const { result } = renderHook(() => useApiCall(mockApi));

      await act(async () => {
        await result.current.execute('arg1', 'arg2', { key: 'value' });
      });

      expect(mockApi).toHaveBeenCalledWith('arg1', 'arg2', { key: 'value' });
    });
  });
});
