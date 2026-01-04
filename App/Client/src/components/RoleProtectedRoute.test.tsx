import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router';
import { RoleProtectedRoute } from './RoleProtectedRoute';
import { AuthContext } from '../context/AuthContextType';
import type { User } from '../types/user';

describe('RoleProtectedRoute', () => {
  const mockUser: User = {
    email: 'admin@test.com',
    username: 'admin',
    bio: 'Admin user',
    image: null,
    roles: ['ADMIN', 'AUTHOR'],
  };

  const mockNonAdminUser: User = {
    email: 'user@test.com',
    username: 'user',
    bio: 'Regular user',
    image: null,
    roles: ['AUTHOR'],
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders children when user has required role', () => {
    render(
      <MemoryRouter initialEntries={['/admin']}>
        <AuthContext.Provider
          value={{
            user: mockUser,
            loading: false,
            login: vi.fn(),
            register: vi.fn(),
            logout: vi.fn(),
            updateUser: vi.fn(),
          }}
        >
          <Routes>
            <Route
              path="/admin"
              element={
                <RoleProtectedRoute requiredRoles={['ADMIN']}>
                  <div>Admin Content</div>
                </RoleProtectedRoute>
              }
            />
          </Routes>
        </AuthContext.Provider>
      </MemoryRouter>
    );

    expect(screen.getByText('Admin Content')).toBeInTheDocument();
  });

  it('redirects to forbidden when user lacks required role', () => {
    render(
      <MemoryRouter initialEntries={['/admin']}>
        <AuthContext.Provider
          value={{
            user: mockNonAdminUser,
            loading: false,
            login: vi.fn(),
            register: vi.fn(),
            logout: vi.fn(),
            updateUser: vi.fn(),
          }}
        >
          <Routes>
            <Route
              path="/admin"
              element={
                <RoleProtectedRoute requiredRoles={['ADMIN']}>
                  <div>Admin Content</div>
                </RoleProtectedRoute>
              }
            />
            <Route path="/forbidden" element={<div>Forbidden</div>} />
          </Routes>
        </AuthContext.Provider>
      </MemoryRouter>
    );

    expect(screen.getByText('Forbidden')).toBeInTheDocument();
  });

  it('redirects to login when user is not authenticated', () => {
    render(
      <MemoryRouter initialEntries={['/admin']}>
        <AuthContext.Provider
          value={{
            user: null,
            loading: false,
            login: vi.fn(),
            register: vi.fn(),
            logout: vi.fn(),
            updateUser: vi.fn(),
          }}
        >
          <Routes>
            <Route
              path="/admin"
              element={
                <RoleProtectedRoute requiredRoles={['ADMIN']}>
                  <div>Admin Content</div>
                </RoleProtectedRoute>
              }
            />
            <Route path="/login" element={<div>Login Page</div>} />
          </Routes>
        </AuthContext.Provider>
      </MemoryRouter>
    );

    expect(screen.getByText('Login Page')).toBeInTheDocument();
  });

  it('shows loading state while authentication is being checked', () => {
    render(
      <MemoryRouter initialEntries={['/admin']}>
        <AuthContext.Provider
          value={{
            user: null,
            loading: true,
            login: vi.fn(),
            register: vi.fn(),
            logout: vi.fn(),
            updateUser: vi.fn(),
          }}
        >
          <Routes>
            <Route
              path="/admin"
              element={
                <RoleProtectedRoute requiredRoles={['ADMIN']}>
                  <div>Admin Content</div>
                </RoleProtectedRoute>
              }
            />
          </Routes>
        </AuthContext.Provider>
      </MemoryRouter>
    );

    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  it('allows access when user has any of the required roles', () => {
    render(
      <MemoryRouter initialEntries={['/admin']}>
        <AuthContext.Provider
          value={{
            user: mockUser,
            loading: false,
            login: vi.fn(),
            register: vi.fn(),
            logout: vi.fn(),
            updateUser: vi.fn(),
          }}
        >
          <Routes>
            <Route
              path="/admin"
              element={
                <RoleProtectedRoute requiredRoles={['ADMIN', 'OWNER']}>
                  <div>Admin Content</div>
                </RoleProtectedRoute>
              }
            />
          </Routes>
        </AuthContext.Provider>
      </MemoryRouter>
    );

    expect(screen.getByText('Admin Content')).toBeInTheDocument();
  });
});
