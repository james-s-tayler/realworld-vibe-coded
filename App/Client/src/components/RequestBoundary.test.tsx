import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { RequestBoundary } from './RequestBoundary';
import { ToastProvider } from '../context/ToastContext';
import { type AppError } from '../utils/errors';

// Wrap component with ToastProvider for all tests
const renderWithProvider = (ui: React.ReactElement) => {
  return render(<ToastProvider>{ui}</ToastProvider>);
};

describe('RequestBoundary', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('children rendering', () => {
    it('renders children when not loading and no error', () => {
      renderWithProvider(
        <RequestBoundary error={null} clearError={vi.fn()}>
          <p>Test content</p>
        </RequestBoundary>
      );
      expect(screen.getByText('Test content')).toBeInTheDocument();
    });

    it('hides children when loading', () => {
      renderWithProvider(
        <RequestBoundary error={null} clearError={vi.fn()} loading={true}>
          <p>Test content</p>
        </RequestBoundary>
      );
      expect(screen.queryByText('Test content')).not.toBeInTheDocument();
    });

    it('shows children when loading is false', () => {
      renderWithProvider(
        <RequestBoundary error={null} clearError={vi.fn()} loading={false}>
          <p>Test content</p>
        </RequestBoundary>
      );
      expect(screen.getByText('Test content')).toBeInTheDocument();
    });
  });

  describe('loading state', () => {
    it('shows default loading message when loading is true', () => {
      renderWithProvider(
        <RequestBoundary error={null} clearError={vi.fn()} loading={true}>
          <p>Content</p>
        </RequestBoundary>
      );
      expect(screen.getByTestId('loading-message')).toHaveTextContent('Loading...');
    });

    it('shows custom loading message when provided', () => {
      renderWithProvider(
        <RequestBoundary
          error={null}
          clearError={vi.fn()}
          loading={true}
          loadingMessage="Loading article..."
        >
          <p>Content</p>
        </RequestBoundary>
      );
      expect(screen.getByTestId('loading-message')).toHaveTextContent('Loading article...');
    });

    it('does not show loading message when loading is false', () => {
      renderWithProvider(
        <RequestBoundary error={null} clearError={vi.fn()} loading={false}>
          <p>Content</p>
        </RequestBoundary>
      );
      expect(screen.queryByTestId('loading-message')).not.toBeInTheDocument();
    });
  });

  describe('error handling with toast', () => {
    const mockError: AppError = {
      type: 'validation',
      title: 'Validation Error',
      messages: ['Title is required', 'Body is required'],
    };

    it('shows toast notification when error is provided', async () => {
      renderWithProvider(
        <RequestBoundary error={mockError} clearError={vi.fn()}>
          <p>Content</p>
        </RequestBoundary>
      );

      // Toast should appear in the toast container
      await waitFor(() => {
        expect(screen.getByText('Validation Error')).toBeInTheDocument();
        expect(screen.getByText('Title is required, Body is required')).toBeInTheDocument();
      });
    });

    it('calls clearError after showing toast', async () => {
      const mockClearError = vi.fn();
      
      renderWithProvider(
        <RequestBoundary error={mockError} clearError={mockClearError}>
          <p>Content</p>
        </RequestBoundary>
      );

      await waitFor(() => {
        expect(mockClearError).toHaveBeenCalledTimes(1);
      });
    });

    it('still shows children when error is displayed as toast', async () => {
      renderWithProvider(
        <RequestBoundary error={mockError} clearError={vi.fn()}>
          <p>Content</p>
        </RequestBoundary>
      );

      // Children should be visible (unlike inline error which could hide content)
      expect(screen.getByText('Content')).toBeInTheDocument();
    });

    it('shows loading message when both error and loading are present', async () => {
      renderWithProvider(
        <RequestBoundary error={mockError} clearError={vi.fn()} loading={true}>
          <p>Content</p>
        </RequestBoundary>
      );

      // Loading message should show instead of content
      expect(screen.getByTestId('loading-message')).toBeInTheDocument();
      expect(screen.queryByText('Content')).not.toBeInTheDocument();

      // Toast should still appear
      await waitFor(() => {
        expect(screen.getByText('Validation Error')).toBeInTheDocument();
      });
    });
  });

  describe('default props', () => {
    it('defaults loading to false', () => {
      renderWithProvider(
        <RequestBoundary error={null} clearError={vi.fn()}>
          <p>Content</p>
        </RequestBoundary>
      );
      expect(screen.getByText('Content')).toBeInTheDocument();
      expect(screen.queryByTestId('loading-message')).not.toBeInTheDocument();
    });
  });
});
