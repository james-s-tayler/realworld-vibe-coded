import './ToastContainer.scss';

import { ToastNotification } from '@carbon/react';
import React from 'react';

import { useToast } from '../hooks/useToast';

export const ToastContainer: React.FC = () => {
  const { toasts, removeToast } = useToast();

  if (toasts.length === 0) return null;

  return (
    <div className="toast-container" aria-live="polite">
      {toasts.map((toast) => (
        <ToastNotification
          key={toast.id}
          kind={toast.kind}
          title={toast.title}
          subtitle={toast.subtitle}
          lowContrast
          onCloseButtonClick={() => removeToast(toast.id)}
          timeout={8000}
          data-testid={`toast-${toast.kind}`}
        />
      ))}
    </div>
  );
};
