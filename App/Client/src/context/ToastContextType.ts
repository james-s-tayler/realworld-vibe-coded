import { createContext } from 'react';

export interface Toast {
  id: string;
  kind: 'error' | 'success' | 'warning' | 'info';
  title: string;
  subtitle?: string;
}

export interface ToastContextType {
  toasts: Toast[];
  showToast: (toast: Omit<Toast, 'id'>) => string;
  removeToast: (id: string) => void;
}

export const ToastContext = createContext<ToastContextType | null>(null);
