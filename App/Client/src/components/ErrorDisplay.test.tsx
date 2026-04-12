import { render } from '@testing-library/react';
import { beforeEach,describe, expect, it, vi } from 'vitest';

import { ApiError } from '../api/client';
import { AppError } from '../utils/errors';
import { ErrorDisplay } from './ErrorDisplay';

const mockShowToast = vi.fn().mockReturnValue('toast-id');
const mockRemoveToast = vi.fn();
vi.mock('../hooks/useToast', () => ({
  useToast: () => ({ showToast: mockShowToast, removeToast: mockRemoveToast, toasts: [] }),
}));

describe('ErrorDisplay', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('rendering', () => {
    it('renders nothing when error is null', () => {
      const { container } = render(<ErrorDisplay error={null} />);
      expect(container.firstChild).toBeNull();
      expect(mockShowToast).not.toHaveBeenCalled();
    });

    it('calls showToast when AppError is provided', () => {
      const appError: AppError = {
        type: 'general',
        title: 'Error',
        messages: ['Something went wrong']
      };
      render(<ErrorDisplay error={appError} />);
      expect(mockShowToast).toHaveBeenCalledWith({
        kind: 'error',
        title: 'Error',
        subtitle: 'Something went wrong',
      });
    });

    it('does not call showToast when messages array is empty', () => {
      const appError: AppError = {
        type: 'general',
        title: 'Error',
        messages: []
      };
      render(<ErrorDisplay error={appError} />);
      expect(mockShowToast).not.toHaveBeenCalled();
    });
  });

  describe('error normalization', () => {
    it('normalizes string error correctly', () => {
      render(<ErrorDisplay error="A simple error message" />);
      expect(mockShowToast).toHaveBeenCalledWith(
        expect.objectContaining({ subtitle: expect.stringContaining('A simple error message') })
      );
    });

    it('normalizes string array errors and joins with commas', () => {
      render(<ErrorDisplay error={['Error 1', 'Error 2', 'Error 3']} />);
      expect(mockShowToast).toHaveBeenCalledWith(
        expect.objectContaining({ subtitle: 'Error 1, Error 2, Error 3' })
      );
    });

    it('normalizes Error object correctly', () => {
      const error = new Error('JavaScript error message');
      render(<ErrorDisplay error={error} />);
      expect(mockShowToast).toHaveBeenCalledWith(
        expect.objectContaining({ subtitle: expect.stringContaining('JavaScript error message') })
      );
    });

    it('normalizes ApiError correctly', () => {
      const apiError = new ApiError(422, ['Title: has already been taken'], 'Bad Request');
      render(<ErrorDisplay error={apiError} />);
      expect(mockShowToast).toHaveBeenCalledWith(
        expect.objectContaining({ subtitle: expect.stringContaining('Title: has already been taken') })
      );
    });

    it('displays multiple ApiError errors joined with commas', () => {
      const apiError = new ApiError(422, ['Title: is required', 'Body: is required'], 'Bad Request');
      render(<ErrorDisplay error={apiError} />);
      expect(mockShowToast).toHaveBeenCalledWith(
        expect.objectContaining({ subtitle: 'Title: is required, Body: is required' })
      );
    });
  });

  describe('title handling', () => {
    it('uses title from AppError', () => {
      const appError: AppError = {
        type: 'validation',
        title: 'Custom Title',
        messages: ['Field is required']
      };
      render(<ErrorDisplay error={appError} />);
      expect(mockShowToast).toHaveBeenCalledWith(
        expect.objectContaining({ title: 'Custom Title' })
      );
    });

    it('uses title from ApiError when available', () => {
      const apiError = new ApiError(422, ['Field is required'], 'Bad Request');
      render(<ErrorDisplay error={apiError} />);
      expect(mockShowToast).toHaveBeenCalledWith(
        expect.objectContaining({ title: 'Bad Request' })
      );
    });

    it('falls back to error type title when ApiError has no title', () => {
      const apiError = new ApiError(401, ['Invalid credentials']);
      render(<ErrorDisplay error={apiError} />);
      expect(mockShowToast).toHaveBeenCalledWith(
        expect.objectContaining({ title: 'Authentication Error' })
      );
    });

    it('shows "Server Error" title for 500 status', () => {
      const apiError = new ApiError(500, ['Internal server error']);
      render(<ErrorDisplay error={apiError} />);
      expect(mockShowToast).toHaveBeenCalledWith(
        expect.objectContaining({ title: 'Server Error' })
      );
    });

    it('shows "Error" title for non-ApiError errors', () => {
      render(<ErrorDisplay error="Generic error" />);
      expect(mockShowToast).toHaveBeenCalledWith(
        expect.objectContaining({ title: 'Error' })
      );
    });
  });

  describe('cleanup', () => {
    it('removes toast when error changes to null', () => {
      const appError: AppError = {
        type: 'general',
        title: 'Error',
        messages: ['Some error']
      };
      const { rerender } = render(<ErrorDisplay error={appError} />);
      expect(mockShowToast).toHaveBeenCalled();

      rerender(<ErrorDisplay error={null} />);
      expect(mockRemoveToast).toHaveBeenCalledWith('toast-id');
    });

    it('removes toast on unmount', () => {
      const appError: AppError = {
        type: 'general',
        title: 'Error',
        messages: ['Some error']
      };
      const { unmount } = render(<ErrorDisplay error={appError} />);
      unmount();
      expect(mockRemoveToast).toHaveBeenCalledWith('toast-id');
    });
  });
});
