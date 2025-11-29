import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ErrorDisplay } from './ErrorDisplay';
import { AppError } from '../utils/errors';
import { ApiError } from '../api/client';

describe('ErrorDisplay', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('rendering', () => {
    it('renders nothing when error is null', () => {
      const { container } = render(<ErrorDisplay error={null} />);
      expect(container.firstChild).toBeNull();
    });

    it('renders nothing when show is false', () => {
      const appError: AppError = {
        type: 'general',
        title: 'Error',
        messages: ['Some error']
      };
      const { container } = render(<ErrorDisplay error={appError} show={false} />);
      expect(container.firstChild).toBeNull();
    });

    it('renders error notification when AppError is provided', () => {
      const appError: AppError = {
        type: 'general',
        title: 'Error',
        messages: ['Something went wrong']
      };
      render(<ErrorDisplay error={appError} />);
      expect(screen.getByTestId('error-display')).toBeInTheDocument();
      expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
    });

    it('renders nothing when messages array is empty', () => {
      const appError: AppError = {
        type: 'general',
        title: 'Error',
        messages: []
      };
      const { container } = render(<ErrorDisplay error={appError} />);
      expect(container.firstChild).toBeNull();
    });
  });

  describe('error normalization', () => {
    it('normalizes string error correctly', () => {
      render(<ErrorDisplay error="A simple error message" />);
      expect(screen.getByText(/a simple error message/i)).toBeInTheDocument();
    });

    it('normalizes string array errors and joins with commas', () => {
      render(<ErrorDisplay error={['Error 1', 'Error 2', 'Error 3']} />);
      expect(screen.getByText(/error 1, error 2, error 3/i)).toBeInTheDocument();
    });

    it('normalizes Error object correctly', () => {
      const error = new Error('JavaScript error message');
      render(<ErrorDisplay error={error} />);
      expect(screen.getByText(/javascript error message/i)).toBeInTheDocument();
    });

    it('normalizes ApiError correctly', () => {
      const apiError = new ApiError(422, ['Title: has already been taken'], 'Bad Request');
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText(/title: has already been taken/i)).toBeInTheDocument();
    });

    it('displays multiple ApiError errors joined with commas', () => {
      const apiError = new ApiError(422, ['Title: is required', 'Body: is required'], 'Bad Request');
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText(/title: is required, body: is required/i)).toBeInTheDocument();
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
      expect(screen.getByText(/custom title/i)).toBeInTheDocument();
    });

    it('uses title from ApiError when available', () => {
      const apiError = new ApiError(422, ['Field is required'], 'Bad Request');
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText(/bad request/i)).toBeInTheDocument();
    });

    it('falls back to error type title when ApiError has no title', () => {
      const apiError = new ApiError(401, ['Invalid credentials']);
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText(/authentication error/i)).toBeInTheDocument();
    });

    it('shows "Server Error" title for 500 status', () => {
      const apiError = new ApiError(500, ['Internal server error']);
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText('Server Error')).toBeInTheDocument();
    });

    it('shows "Error" title for non-ApiError errors', () => {
      render(<ErrorDisplay error="Generic error" />);
      expect(screen.getByText('Error')).toBeInTheDocument();
    });
  });

  describe('close button', () => {
    it('calls onClose callback when close button is clicked', async () => {
      const user = userEvent.setup();
      const mockOnClose = vi.fn();
      const appError: AppError = {
        type: 'general',
        title: 'Error',
        messages: ['Some error']
      };
      render(<ErrorDisplay error={appError} onClose={mockOnClose} />);
      
      const closeButton = screen.getByRole('button', { name: /close/i });
      await user.click(closeButton);
      
      expect(mockOnClose).toHaveBeenCalledTimes(1);
    });
  });

  describe('accessibility', () => {
    it('has aria-live attribute for screen readers', () => {
      const appError: AppError = {
        type: 'general',
        title: 'Error',
        messages: ['Some error']
      };
      render(<ErrorDisplay error={appError} />);
      const notification = screen.getByTestId('error-display');
      expect(notification).toHaveAttribute('aria-live', 'polite');
    });

    it('has data-testid for testing', () => {
      const appError: AppError = {
        type: 'general',
        title: 'Error',
        messages: ['Some error']
      };
      render(<ErrorDisplay error={appError} />);
      expect(screen.getByTestId('error-display')).toBeInTheDocument();
    });
  });

  describe('styling', () => {
    it('applies custom styles', () => {
      const appError: AppError = {
        type: 'general',
        title: 'Error',
        messages: ['Some error']
      };
      render(<ErrorDisplay error={appError} style={{ marginTop: '2rem' }} />);
      const notification = screen.getByTestId('error-display');
      expect(notification).toHaveStyle({ marginTop: '2rem' });
    });

    it('always includes marginBottom style', () => {
      const appError: AppError = {
        type: 'general',
        title: 'Error',
        messages: ['Some error']
      };
      render(<ErrorDisplay error={appError} />);
      const notification = screen.getByTestId('error-display');
      expect(notification).toHaveStyle({ marginBottom: '1rem' });
    });
  });
});
