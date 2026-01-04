import { describe, it, expect, vi } from 'vitest';
import { renderHook } from '@testing-library/react';
import { useHasRole } from './useHasRole';
import { AuthContext } from '../context/AuthContextType';
import type { User } from '../types/user';

const wrapper = (user: User | null) => ({ children }: { children: React.ReactNode }) => (
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
    {children}
  </AuthContext.Provider>
);

describe('useHasRole', () => {
  it('returns true when user has the required role', () => {
    const user: User = {
      email: 'admin@test.com',
      username: 'admin',
      bio: 'Admin user',
      image: null,
      token: 'test-token',
      roles: ['ADMIN', 'AUTHOR'],
    };

    const { result } = renderHook(() => useHasRole(['ADMIN']), {
      wrapper: wrapper(user),
    });

    expect(result.current).toBe(true);
  });

  it('returns false when user does not have the required role', () => {
    const user: User = {
      email: 'user@test.com',
      username: 'user',
      bio: 'Regular user',
      image: null,
      token: 'test-token',
      roles: ['AUTHOR'],
    };

    const { result } = renderHook(() => useHasRole(['ADMIN']), {
      wrapper: wrapper(user),
    });

    expect(result.current).toBe(false);
  });

  it('returns true when user has at least one of multiple required roles', () => {
    const user: User = {
      email: 'user@test.com',
      username: 'user',
      bio: 'Regular user',
      image: null,
      token: 'test-token',
      roles: ['AUTHOR'],
    };

    const { result } = renderHook(() => useHasRole(['ADMIN', 'AUTHOR']), {
      wrapper: wrapper(user),
    });

    expect(result.current).toBe(true);
  });

  it('returns false when user is null', () => {
    const { result } = renderHook(() => useHasRole(['ADMIN']), {
      wrapper: wrapper(null),
    });

    expect(result.current).toBe(false);
  });

  it('returns false when user has no roles', () => {
    const user: User = {
      email: 'user@test.com',
      username: 'user',
      bio: 'User with no roles',
      image: null,
      token: 'test-token',
      roles: [],
    };

    const { result } = renderHook(() => useHasRole(['ADMIN']), {
      wrapper: wrapper(user),
    });

    expect(result.current).toBe(false);
  });
});
