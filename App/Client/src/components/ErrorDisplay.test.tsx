import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ErrorDisplay } from './ErrorDisplay';
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
      const { container } = render(<ErrorDisplay error="Some error" show={false} />);
      expect(container.firstChild).toBeNull();
    });

    it('renders error notification when error is provided', () => {
      render(<ErrorDisplay error="Something went wrong" />);
      expect(screen.getByTestId('error-display')).toBeInTheDocument();
      expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
    });
  });

  describe('error message extraction', () => {
    it('displays string error correctly', () => {
      render(<ErrorDisplay error="A simple error message" />);
      expect(screen.getByText(/a simple error message/i)).toBeInTheDocument();
    });

    it('displays string array errors joined with commas', () => {
      render(<ErrorDisplay error={['Error 1', 'Error 2', 'Error 3']} />);
      expect(screen.getByText(/error 1, error 2, error 3/i)).toBeInTheDocument();
    });

    it('displays Error object message', () => {
      const error = new Error('JavaScript error message');
      render(<ErrorDisplay error={error} />);
      expect(screen.getByText(/javascript error message/i)).toBeInTheDocument();
    });

    it('displays ApiError errors correctly', () => {
      const apiError = new ApiError(422, ['Title: has already been taken']);
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText(/title: has already been taken/i)).toBeInTheDocument();
    });

    it('displays multiple ApiError errors joined with commas', () => {
      const apiError = new ApiError(422, ['Title: is required', 'Body: is required']);
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText(/title: is required, body: is required/i)).toBeInTheDocument();
    });
  });

  describe('error type detection', () => {
    it('shows "Validation Error" title for 422 status', () => {
      const apiError = new ApiError(422, ['Field is required']);
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText(/validation error/i)).toBeInTheDocument();
    });

    it('shows "Validation Error" title for 400 status', () => {
      const apiError = new ApiError(400, ['Bad request']);
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText(/validation error/i)).toBeInTheDocument();
    });

    it('shows "Authentication Error" title for 401 status', () => {
      const apiError = new ApiError(401, ['Invalid credentials']);
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText(/authentication error/i)).toBeInTheDocument();
    });

    it('shows "Authentication Error" title for 403 status', () => {
      const apiError = new ApiError(403, ['Access denied']);
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText(/authentication error/i)).toBeInTheDocument();
    });

    it('shows "Server Error" title for 500 status', () => {
      const apiError = new ApiError(500, ['Internal server error']);
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText('Server Error')).toBeInTheDocument();
    });

    it('shows "Error" title for other status codes', () => {
      const apiError = new ApiError(404, ['Not found']);
      render(<ErrorDisplay error={apiError} />);
      expect(screen.getByText('Error')).toBeInTheDocument();
    });

    it('shows "Error" title for non-ApiError errors', () => {
      render(<ErrorDisplay error="Generic error" />);
      expect(screen.getByText('Error')).toBeInTheDocument();
    });
  });

  describe('custom title', () => {
    it('uses custom title when provided', () => {
      const apiError = new ApiError(422, ['Field is required']);
      render(<ErrorDisplay error={apiError} title="Registration Failed" />);
      expect(screen.getByText(/registration failed/i)).toBeInTheDocument();
    });

    it('uses custom title over auto-detected type', () => {
      const apiError = new ApiError(401, ['Invalid credentials']);
      render(<ErrorDisplay error={apiError} title="Login Failed" />);
      expect(screen.getByText(/login failed/i)).toBeInTheDocument();
      expect(screen.queryByText(/authentication error/i)).not.toBeInTheDocument();
    });
  });

  describe('close button', () => {
    it('calls onClose callback when close button is clicked', async () => {
      const user = userEvent.setup();
      const mockOnClose = vi.fn();
      render(<ErrorDisplay error="Some error" onClose={mockOnClose} />);
      
      const closeButton = screen.getByRole('button', { name: /close/i });
      await user.click(closeButton);
      
      expect(mockOnClose).toHaveBeenCalledTimes(1);
    });
  });

  describe('accessibility', () => {
    it('has aria-live attribute for screen readers', () => {
      render(<ErrorDisplay error="Some error" />);
      const notification = screen.getByTestId('error-display');
      expect(notification).toHaveAttribute('aria-live', 'polite');
    });

    it('has data-testid for testing', () => {
      render(<ErrorDisplay error="Some error" />);
      expect(screen.getByTestId('error-display')).toBeInTheDocument();
    });
  });

  describe('styling', () => {
    it('applies custom styles', () => {
      render(<ErrorDisplay error="Some error" style={{ marginTop: '2rem' }} />);
      const notification = screen.getByTestId('error-display');
      expect(notification).toHaveStyle({ marginTop: '2rem' });
    });

    it('always includes marginBottom style', () => {
      render(<ErrorDisplay error="Some error" />);
      const notification = screen.getByTestId('error-display');
      expect(notification).toHaveStyle({ marginBottom: '1rem' });
    });
  });

  describe('errorType override', () => {
    it('allows manual override of error type', () => {
      const apiError = new ApiError(422, ['Something failed']);
      render(<ErrorDisplay error={apiError} errorType="network" />);
      expect(screen.getByText(/network error/i)).toBeInTheDocument();
    });
  });
});
