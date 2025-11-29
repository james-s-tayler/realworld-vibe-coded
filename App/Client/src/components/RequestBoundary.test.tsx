import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { RequestBoundary } from './RequestBoundary';
import { type AppError } from '../utils/errors';

describe('RequestBoundary', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('children rendering', () => {
    it('renders children when not loading and no error', () => {
      render(
        <RequestBoundary error={null} clearError={vi.fn()}>
          <p>Test content</p>
        </RequestBoundary>
      );
      expect(screen.getByText('Test content')).toBeInTheDocument();
    });

    it('hides children when loading', () => {
      render(
        <RequestBoundary error={null} clearError={vi.fn()} loading={true}>
          <p>Test content</p>
        </RequestBoundary>
      );
      expect(screen.queryByText('Test content')).not.toBeInTheDocument();
    });

    it('shows children when loading is false', () => {
      render(
        <RequestBoundary error={null} clearError={vi.fn()} loading={false}>
          <p>Test content</p>
        </RequestBoundary>
      );
      expect(screen.getByText('Test content')).toBeInTheDocument();
    });
  });

  describe('loading state', () => {
    it('shows default loading message when loading is true', () => {
      render(
        <RequestBoundary error={null} clearError={vi.fn()} loading={true}>
          <p>Content</p>
        </RequestBoundary>
      );
      expect(screen.getByTestId('loading-message')).toHaveTextContent('Loading...');
    });

    it('shows custom loading message when provided', () => {
      render(
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
      render(
        <RequestBoundary error={null} clearError={vi.fn()} loading={false}>
          <p>Content</p>
        </RequestBoundary>
      );
      expect(screen.queryByTestId('loading-message')).not.toBeInTheDocument();
    });
  });

  describe('error handling', () => {
    const mockError: AppError = {
      type: 'validation',
      title: 'Validation Error',
      messages: ['Title is required', 'Body is required'],
    };

    it('displays error when error prop is provided', () => {
      render(
        <RequestBoundary error={mockError} clearError={vi.fn()}>
          <p>Content</p>
        </RequestBoundary>
      );
      expect(screen.getByTestId('error-display')).toBeInTheDocument();
      expect(screen.getByText(/title is required, body is required/i)).toBeInTheDocument();
    });

    it('does not display error when error prop is null', () => {
      render(
        <RequestBoundary error={null} clearError={vi.fn()}>
          <p>Content</p>
        </RequestBoundary>
      );
      expect(screen.queryByTestId('error-display')).not.toBeInTheDocument();
    });

    it('shows both error and children (error above content)', () => {
      render(
        <RequestBoundary error={mockError} clearError={vi.fn()}>
          <p>Content</p>
        </RequestBoundary>
      );
      expect(screen.getByTestId('error-display')).toBeInTheDocument();
      expect(screen.getByText('Content')).toBeInTheDocument();
    });

    it('shows error with loading message when both error and loading are present', () => {
      render(
        <RequestBoundary error={mockError} clearError={vi.fn()} loading={true}>
          <p>Content</p>
        </RequestBoundary>
      );
      expect(screen.getByTestId('error-display')).toBeInTheDocument();
      expect(screen.getByTestId('loading-message')).toBeInTheDocument();
      expect(screen.queryByText('Content')).not.toBeInTheDocument();
    });

    it('calls clearError when close button is clicked', async () => {
      const user = userEvent.setup();
      const mockClearError = vi.fn();

      render(
        <RequestBoundary error={mockError} clearError={mockClearError}>
          <p>Content</p>
        </RequestBoundary>
      );

      const closeButton = screen.getByRole('button', { name: /close/i });
      await user.click(closeButton);

      expect(mockClearError).toHaveBeenCalledTimes(1);
    });
  });

  describe('default props', () => {
    it('defaults loading to false', () => {
      render(
        <RequestBoundary error={null} clearError={vi.fn()}>
          <p>Content</p>
        </RequestBoundary>
      );
      expect(screen.getByText('Content')).toBeInTheDocument();
      expect(screen.queryByTestId('loading-message')).not.toBeInTheDocument();
    });
  });
});
