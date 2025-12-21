import React, { useState, useEffect, type ReactNode } from 'react';
import { authApi } from '../api/auth';
import type { User } from '../types/user';
import { AuthContext } from './AuthContextType';

export { AuthContext };

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Try to fetch current user on mount (cookie-based auth)
    authApi
      .getCurrentUser()
      .then((response) => {
        setUser(response.user);
      })
      .catch(() => {
        // No valid session - user is not authenticated
        setUser(null);
      })
      .finally(() => {
        setLoading(false);
      });
  }, []);

  const login = async (email: string, password: string) => {
    await authApi.login(email, password);
    // Fetch user details after login
    const response = await authApi.getCurrentUser();
    setUser(response.user);
  };

  const register = async (email: string, password: string) => {
    await authApi.register(email, password);
    // Now login to get the session cookie
    await login(email, password);
  };

  const logout = async () => {
    await authApi.logout();
    setUser(null);
  };

  const updateUser = async (updates: {
    email?: string;
    username?: string;
    password?: string;
    bio?: string;
    image?: string;
  }) => {
    const response = await authApi.updateUser(updates);
    setUser(response.user);
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout, updateUser }}>
      {children}
    </AuthContext.Provider>
  );
};
