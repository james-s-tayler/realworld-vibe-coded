import { createContext } from 'react';
import type { User } from '../types/user';

export interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string) => Promise<void>;
  logout: () => void;
  updateUser: (updates: {
    email?: string;
    username?: string;
    password?: string;
    bio?: string;
    image?: string;
  }) => Promise<void>;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);
