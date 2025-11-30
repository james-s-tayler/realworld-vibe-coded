import React, { useState, useCallback, useMemo } from 'react';
import { ToastNotification } from '@carbon/react';
import { type AppError } from '../utils/errors';
import { type Toast, type ToastContextType, ToastContext } from './ToastContextType';

export { ToastContext };

const DEFAULT_TIMEOUT = 5000;

let toastIdCounter = 0;
function generateToastId(): string {
  return `toast-${++toastIdCounter}`;
}

interface ToastProviderProps {
  children: React.ReactNode;
}

export const ToastProvider: React.FC<ToastProviderProps> = ({ children }) => {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((toast) => toast.id !== id));
  }, []);

  const addToast = useCallback((toast: Omit<Toast, 'id'>): string => {
    const id = generateToastId();
    setToasts((prev) => [...prev, { ...toast, id }]);
    return id;
  }, []);

  const showError = useCallback((error: AppError | null) => {
    if (!error) return;
    addToast({
      kind: 'error',
      title: error.title,
      subtitle: error.messages.join(', '),
      timeout: 0, // Errors don't auto-dismiss
    });
  }, [addToast]);

  const showSuccess = useCallback((title: string, subtitle?: string) => {
    addToast({
      kind: 'success',
      title,
      subtitle,
      timeout: DEFAULT_TIMEOUT,
    });
  }, [addToast]);

  const showInfo = useCallback((title: string, subtitle?: string) => {
    addToast({
      kind: 'info',
      title,
      subtitle,
      timeout: DEFAULT_TIMEOUT,
    });
  }, [addToast]);

  const showWarning = useCallback((title: string, subtitle?: string) => {
    addToast({
      kind: 'warning',
      title,
      subtitle,
      timeout: DEFAULT_TIMEOUT,
    });
  }, [addToast]);

  const clearToasts = useCallback(() => {
    setToasts([]);
  }, []);

  const value = useMemo<ToastContextType>(() => ({
    toasts,
    showError,
    showSuccess,
    showInfo,
    showWarning,
    removeToast,
    clearToasts,
  }), [toasts, showError, showSuccess, showInfo, showWarning, removeToast, clearToasts]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div
        className="toast-container"
        style={{
          position: 'fixed',
          top: 'calc(var(--header-height, 3rem) + 1rem)',
          right: '1rem',
          zIndex: 9999,
          display: 'flex',
          flexDirection: 'column',
          gap: '0.5rem',
          maxWidth: '400px',
        }}
        data-testid="toast-container"
      >
        {toasts.map((toast) => (
          <ToastNotification
            key={toast.id}
            kind={toast.kind}
            title={toast.title}
            subtitle={toast.subtitle}
            timeout={toast.timeout}
            onClose={() => removeToast(toast.id)}
            onCloseButtonClick={() => removeToast(toast.id)}
            data-testid={toast.kind === 'error' ? 'error-display' : `toast-${toast.id}`}
            style={{ marginBottom: 0 }}
          />
        ))}
      </div>
    </ToastContext.Provider>
  );
};
