import { describe, it, expect } from 'vitest';
import { normalizeError, AppError } from './errors';
import { ApiError } from '../api/client';

describe('normalizeError', () => {
  describe('ApiError normalization', () => {
    it('normalizes ApiError with title', () => {
      const apiError = new ApiError(400, ['Field is required'], 'Bad Request');
      const result = normalizeError(apiError);

      expect(result).toEqual<AppError>({
        type: 'validation',
        title: 'Bad Request',
        messages: ['Field is required'],
        status: 400,
        cause: apiError,
      });
    });

    it('normalizes ApiError without title and uses default based on status', () => {
      const apiError = new ApiError(401, ['Invalid credentials']);
      const result = normalizeError(apiError);

      expect(result).toEqual<AppError>({
        type: 'auth',
        title: 'Authentication Error',
        messages: ['Invalid credentials'],
        status: 401,
        cause: apiError,
      });
    });

    it('detects validation error type for 422 status', () => {
      const apiError = new ApiError(422, ['Validation failed']);
      const result = normalizeError(apiError);
      expect(result.type).toBe('validation');
    });

    it('detects validation error type for 400 status', () => {
      const apiError = new ApiError(400, ['Bad request']);
      const result = normalizeError(apiError);
      expect(result.type).toBe('validation');
    });

    it('detects auth error type for 401 status', () => {
      const apiError = new ApiError(401, ['Unauthorized']);
      const result = normalizeError(apiError);
      expect(result.type).toBe('auth');
    });

    it('detects auth error type for 403 status', () => {
      const apiError = new ApiError(403, ['Forbidden']);
      const result = normalizeError(apiError);
      expect(result.type).toBe('auth');
    });

    it('detects server error type for 500 status', () => {
      const apiError = new ApiError(500, ['Internal server error']);
      const result = normalizeError(apiError);
      expect(result.type).toBe('server');
    });

    it('detects server error type for 503 status', () => {
      const apiError = new ApiError(503, ['Service unavailable']);
      const result = normalizeError(apiError);
      expect(result.type).toBe('server');
    });

    it('defaults to general error type for other status codes', () => {
      const apiError = new ApiError(404, ['Not found']);
      const result = normalizeError(apiError);
      expect(result.type).toBe('general');
    });
  });

  describe('Error normalization', () => {
    it('normalizes standard Error object', () => {
      const error = new Error('Something went wrong');
      const result = normalizeError(error);

      expect(result).toEqual<AppError>({
        type: 'general',
        title: 'Error',
        messages: ['Something went wrong'],
        cause: error,
      });
    });
  });

  describe('String normalization', () => {
    it('normalizes string error', () => {
      const error = 'A simple error message';
      const result = normalizeError(error);

      expect(result).toEqual<AppError>({
        type: 'general',
        title: 'Error',
        messages: ['A simple error message'],
        cause: error,
      });
    });
  });

  describe('String array normalization', () => {
    it('normalizes array of strings', () => {
      const errors = ['Error 1', 'Error 2', 'Error 3'];
      const result = normalizeError(errors);

      expect(result).toEqual<AppError>({
        type: 'general',
        title: 'Error',
        messages: errors,
        cause: errors,
      });
    });
  });

  describe('Fallback normalization', () => {
    it('handles null with fallback', () => {
      const result = normalizeError(null);
      expect(result.messages).toEqual(['An unexpected error occurred']);
    });

    it('handles undefined with fallback', () => {
      const result = normalizeError(undefined);
      expect(result.messages).toEqual(['An unexpected error occurred']);
    });

    it('handles number with fallback', () => {
      const result = normalizeError(42);
      expect(result.messages).toEqual(['An unexpected error occurred']);
    });

    it('handles object with fallback', () => {
      const result = normalizeError({ foo: 'bar' });
      expect(result.messages).toEqual(['An unexpected error occurred']);
    });

    it('handles mixed array with fallback', () => {
      const result = normalizeError(['Error', 123, 'Another']);
      expect(result.messages).toEqual(['An unexpected error occurred']);
    });
  });
});
