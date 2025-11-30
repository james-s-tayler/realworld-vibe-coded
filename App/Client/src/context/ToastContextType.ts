import { createContext } from 'react';
import { type AppError } from '../utils/errors';

export interface Toast {
  id: string;
  kind: 'error' | 'info' | 'success' | 'warning';
  title: string;
  subtitle?: string;
  timeout?: number;
}

export interface ToastContextType {
  /** Currently displayed toasts */
  toasts: Toast[];
  /** Show an error toast from an AppError */
  showError: (error: AppError | null) => void;
  /** Show a success toast */
  showSuccess: (title: string, subtitle?: string) => void;
  /** Show an info toast */
  showInfo: (title: string, subtitle?: string) => void;
  /** Show a warning toast */
  showWarning: (title: string, subtitle?: string) => void;
  /** Remove a toast by id */
  removeToast: (id: string) => void;
  /** Clear all toasts */
  clearToasts: () => void;
}

export const ToastContext = createContext<ToastContextType | undefined>(undefined);
