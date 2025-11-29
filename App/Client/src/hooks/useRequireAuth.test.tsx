import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { useRequireAuth } from './useRequireAuth';
import { AuthContext } from '../context/AuthContextType';
import { ApiError } from '../api/client';

// Mock useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router', async () => {
  const actual = await vi.importActual('react-router');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

const renderUseRequireAuth = (user: { username: string; email: string } | null = null) => {
  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <MemoryRouter>
      <AuthContext.Provider
        value={{
          user: user ? { ...user, bio: null, image: null, token: 'test-token' } : null,
          loading: false,
          login: vi.fn(),
          register: vi.fn(),
          logout: vi.fn(),
          updateUser: vi.fn(),
        }}
      >
        {children}
      </AuthContext.Provider>
    </MemoryRouter>
  );

  return renderHook(() => useRequireAuth(), { wrapper });
};

describe('useRequireAuth', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('redirects to login when user is not authenticated', async () => {
    const { result } = renderUseRequireAuth(null);

    const mockAction = vi.fn().mockResolvedValue({ success: true });

    let actionResult: unknown;
    await act(async () => {
      actionResult = await result.current.requireAuth(mockAction);
    });

    expect(mockNavigate).toHaveBeenCalledWith('/login');
    expect(mockAction).not.toHaveBeenCalled();
    expect(actionResult).toBeUndefined();
  });

  it('executes action when user is authenticated', async () => {
    const { result } = renderUseRequireAuth({ username: 'testuser', email: 'test@example.com' });

    const expectedResult = { success: true };
    const mockAction = vi.fn().mockResolvedValue(expectedResult);

    let actionResult: unknown;
    await act(async () => {
      actionResult = await result.current.requireAuth(mockAction);
    });

    expect(mockNavigate).not.toHaveBeenCalled();
    expect(mockAction).toHaveBeenCalled();
    expect(actionResult).toEqual(expectedResult);
  });

  it('redirects to login when action throws 401 error', async () => {
    const { result } = renderUseRequireAuth({ username: 'testuser', email: 'test@example.com' });

    const mockAction = vi.fn().mockRejectedValue(new ApiError(401, ['Unauthorized']));

    let actionResult: unknown;
    await act(async () => {
      actionResult = await result.current.requireAuth(mockAction);
    });

    expect(mockNavigate).toHaveBeenCalledWith('/login');
    expect(actionResult).toBeUndefined();
  });

  it('rethrows non-401 errors', async () => {
    const { result } = renderUseRequireAuth({ username: 'testuser', email: 'test@example.com' });

    const error = new ApiError(500, ['Server Error']);
    const mockAction = vi.fn().mockRejectedValue(error);

    await act(async () => {
      await expect(result.current.requireAuth(mockAction)).rejects.toThrow(error);
    });

    expect(mockNavigate).not.toHaveBeenCalled();
  });

  it('rethrows non-ApiError errors', async () => {
    const { result } = renderUseRequireAuth({ username: 'testuser', email: 'test@example.com' });

    const error = new Error('Generic Error');
    const mockAction = vi.fn().mockRejectedValue(error);

    await act(async () => {
      await expect(result.current.requireAuth(mockAction)).rejects.toThrow(error);
    });

    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
