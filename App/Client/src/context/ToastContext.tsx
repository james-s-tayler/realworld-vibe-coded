import React, { useState, useCallback, type ReactNode } from 'react';
import { ToastContext, type Toast } from './ToastContextType';

let nextId = 0;

interface ToastProviderProps {
  children: ReactNode;
}

export const ToastProvider: React.FC<ToastProviderProps> = ({ children }) => {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const showToast = useCallback(
    (toast: Omit<Toast, 'id'>) => {
      const id = String(nextId++);
      setToasts((prev) => [...prev, { ...toast, id }]);
      return id;
    },
    [],
  );

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  return (
    <ToastContext.Provider value={{ toasts, showToast, removeToast }}>
      {children}
    </ToastContext.Provider>
  );
};
