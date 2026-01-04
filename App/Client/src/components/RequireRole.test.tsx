import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { RequireRole } from './RequireRole';
import { AuthContext } from '../context/AuthContextType';
import type { User } from '../types/user';

const renderWithAuth = (user: User | null, roles: string[], fallback?: React.ReactNode) => {
  return render(
    <AuthContext.Provider
      value={{
        user,
        loading: false,
        login: vi.fn(),
        register: vi.fn(),
        logout: vi.fn(),
        updateUser: vi.fn(),
      }}
    >
      <RequireRole roles={roles} fallback={fallback}>
        <div>Protected Content</div>
      </RequireRole>
    </AuthContext.Provider>
  );
};

describe('RequireRole', () => {
  it('renders children when user has required role', () => {
    const user: User = {
      email: 'admin@test.com',
      username: 'admin',
      bio: 'Admin user',
      image: null,
      roles: ['ADMIN', 'AUTHOR'],
    };

    renderWithAuth(user, ['ADMIN']);

    expect(screen.getByText('Protected Content')).toBeInTheDocument();
  });

  it('renders children when user has required role and no fallback provided', () => {
    const user: User = {
      email: 'admin@test.com',
      username: 'admin',
      bio: 'Admin user',
      image: null,
      roles: ['ADMIN', 'AUTHOR'],
    };

    render(
      <AuthContext.Provider
        value={{
          user,
          loading: false,
          login: vi.fn(),
          register: vi.fn(),
          logout: vi.fn(),
          updateUser: vi.fn(),
        }}
      >
        <RequireRole roles={['ADMIN']}>
          <div>Protected Content</div>
        </RequireRole>
      </AuthContext.Provider>
    );

    expect(screen.getByText('Protected Content')).toBeInTheDocument();
  });

  it('does not render children when user lacks required role', () => {
    const user: User = {
      email: 'user@test.com',
      username: 'user',
      bio: 'Regular user',
      image: null,
      roles: ['AUTHOR'],
    };

    renderWithAuth(user, ['ADMIN']);

    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  it('renders fallback when provided and user lacks required role', () => {
    const user: User = {
      email: 'user@test.com',
      username: 'user',
      bio: 'Regular user',
      image: null,
      roles: ['AUTHOR'],
    };

    renderWithAuth(user, ['ADMIN'], <div>Fallback Content</div>);

    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
    expect(screen.getByText('Fallback Content')).toBeInTheDocument();
  });

  it('renders children when user has at least one of multiple required roles', () => {
    const user: User = {
      email: 'user@test.com',
      username: 'user',
      bio: 'Regular user',
      image: null,
      roles: ['AUTHOR'],
    };

    renderWithAuth(user, ['ADMIN', 'AUTHOR']);

    expect(screen.getByText('Protected Content')).toBeInTheDocument();
  });

  it('does not render children when user is null', () => {
    renderWithAuth(null, ['ADMIN']);

    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  it('does not render children when user has empty roles', () => {
    const user: User = {
      email: 'user@test.com',
      username: 'user',
      bio: 'User with no roles',
      image: null,
      roles: [],
    };

    renderWithAuth(user, ['ADMIN']);

    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  it('does not render children when user lacks required role and no fallback provided', () => {
    const user: User = {
      email: 'user@test.com',
      username: 'user',
      bio: 'Regular user',
      image: null,
      roles: ['AUTHOR'],
    };

    render(
      <AuthContext.Provider
        value={{
          user,
          loading: false,
          login: vi.fn(),
          register: vi.fn(),
          logout: vi.fn(),
          updateUser: vi.fn(),
        }}
      >
        <RequireRole roles={['ADMIN']}>
          <div>Protected Content</div>
        </RequireRole>
      </AuthContext.Provider>
    );

    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });
});
