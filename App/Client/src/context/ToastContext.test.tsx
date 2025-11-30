import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ToastProvider } from './ToastContext';
import { useToast } from '../hooks/useToast';

// Test component that uses the toast hook
const TestComponent: React.FC<{
  onMount?: (toast: ReturnType<typeof useToast>) => void;
}> = ({ onMount }) => {
  const toast = useToast();

  // Call onMount callback so tests can access toast methods
  if (onMount) {
    onMount(toast);
  }

  return (
    <div>
      <button onClick={() => toast.showSuccess('Success!', 'Operation completed')}>
        Show Success
      </button>
      <button onClick={() => toast.showError({ type: 'validation', title: 'Error', messages: ['Something went wrong'] })}>
        Show Error
      </button>
      <button onClick={() => toast.showInfo('Info', 'FYI message')}>
        Show Info
      </button>
      <button onClick={() => toast.showWarning('Warning', 'Be careful')}>
        Show Warning
      </button>
      <button onClick={() => toast.clearToasts()}>
        Clear All
      </button>
    </div>
  );
};

describe('ToastContext', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('ToastProvider', () => {
    it('renders children', () => {
      render(
        <ToastProvider>
          <div data-testid="child">Child content</div>
        </ToastProvider>
      );
      expect(screen.getByTestId('child')).toBeInTheDocument();
    });

    it('renders toast container', () => {
      render(
        <ToastProvider>
          <div>Content</div>
        </ToastProvider>
      );
      expect(screen.getByTestId('toast-container')).toBeInTheDocument();
    });
  });

  describe('useToast hook', () => {
    it('throws error when used outside ToastProvider', () => {
      // Suppress console error for this test
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      expect(() => render(<TestComponent />)).toThrow('useToast must be used within a ToastProvider');
      
      consoleSpy.mockRestore();
    });
  });

  describe('showSuccess', () => {
    it('shows success toast when showSuccess is called', async () => {
      const user = userEvent.setup();
      render(
        <ToastProvider>
          <TestComponent />
        </ToastProvider>
      );

      await user.click(screen.getByText('Show Success'));

      await waitFor(() => {
        expect(screen.getByText('Success!')).toBeInTheDocument();
        expect(screen.getByText('Operation completed')).toBeInTheDocument();
      });
    });
  });

  describe('showError', () => {
    it('shows error toast when showError is called', async () => {
      const user = userEvent.setup();
      render(
        <ToastProvider>
          <TestComponent />
        </ToastProvider>
      );

      await user.click(screen.getByText('Show Error'));

      await waitFor(() => {
        expect(screen.getByText('Error')).toBeInTheDocument();
        expect(screen.getByText('Something went wrong')).toBeInTheDocument();
      });
    });

    it('does not show toast when error is null', async () => {
      let toastRef: ReturnType<typeof useToast> | null = null;
      
      render(
        <ToastProvider>
          <TestComponent onMount={(t) => { toastRef = t; }} />
        </ToastProvider>
      );

      // Call showError with null
      toastRef?.showError(null);

      // No toast should appear
      await waitFor(() => {
        const container = screen.getByTestId('toast-container');
        expect(container.children.length).toBe(0);
      });
    });
  });

  describe('showInfo', () => {
    it('shows info toast when showInfo is called', async () => {
      const user = userEvent.setup();
      render(
        <ToastProvider>
          <TestComponent />
        </ToastProvider>
      );

      await user.click(screen.getByText('Show Info'));

      await waitFor(() => {
        expect(screen.getByText('Info')).toBeInTheDocument();
        expect(screen.getByText('FYI message')).toBeInTheDocument();
      });
    });
  });

  describe('showWarning', () => {
    it('shows warning toast when showWarning is called', async () => {
      const user = userEvent.setup();
      render(
        <ToastProvider>
          <TestComponent />
        </ToastProvider>
      );

      await user.click(screen.getByText('Show Warning'));

      await waitFor(() => {
        expect(screen.getByText('Warning')).toBeInTheDocument();
        expect(screen.getByText('Be careful')).toBeInTheDocument();
      });
    });
  });

  describe('clearToasts', () => {
    it('removes all toasts when clearToasts is called', async () => {
      const user = userEvent.setup();
      render(
        <ToastProvider>
          <TestComponent />
        </ToastProvider>
      );

      // Show multiple toasts
      await user.click(screen.getByText('Show Success'));
      await user.click(screen.getByText('Show Info'));

      await waitFor(() => {
        expect(screen.getByText('Success!')).toBeInTheDocument();
        expect(screen.getByText('Info')).toBeInTheDocument();
      });

      // Clear all toasts
      await user.click(screen.getByText('Clear All'));

      await waitFor(() => {
        expect(screen.queryByText('Success!')).not.toBeInTheDocument();
        expect(screen.queryByText('Info')).not.toBeInTheDocument();
      });
    });
  });

  describe('multiple toasts', () => {
    it('can show multiple toasts at once', async () => {
      const user = userEvent.setup();
      render(
        <ToastProvider>
          <TestComponent />
        </ToastProvider>
      );

      await user.click(screen.getByText('Show Success'));
      await user.click(screen.getByText('Show Error'));
      await user.click(screen.getByText('Show Info'));

      await waitFor(() => {
        expect(screen.getByText('Success!')).toBeInTheDocument();
        expect(screen.getByText('Error')).toBeInTheDocument();
        expect(screen.getByText('Info')).toBeInTheDocument();
      });
    });
  });

  describe('toast dismissal', () => {
    it('removes toast when close button is clicked', async () => {
      const user = userEvent.setup();
      render(
        <ToastProvider>
          <TestComponent />
        </ToastProvider>
      );

      await user.click(screen.getByText('Show Success'));

      await waitFor(() => {
        expect(screen.getByText('Success!')).toBeInTheDocument();
      });

      // Find and click the close button on the toast
      const closeButton = screen.getByRole('button', { name: /close/i });
      await user.click(closeButton);

      await waitFor(() => {
        expect(screen.queryByText('Success!')).not.toBeInTheDocument();
      });
    });
  });
});
