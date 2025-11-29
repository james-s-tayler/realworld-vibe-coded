import { useCallback } from 'react';
import { useNavigate } from 'react-router';
import { useAuth } from './useAuth';
import { ApiError } from '../api/client';

/**
 * A hook that provides a wrapper function for authenticated API actions.
 * When an unauthenticated user triggers such an action, they are redirected to the login page.
 * If the action results in a 401 error, the user is also redirected to login.
 *
 * @returns A function that wraps an async action and handles authentication requirements.
 */
export const useRequireAuth = () => {
  const navigate = useNavigate();
  const { user } = useAuth();

  /**
   * Wraps an authenticated action. If the user is not logged in, redirects to login.
   * If the action throws a 401 error, redirects to login.
   *
   * @param action The async action to perform
   * @returns The result of the action if successful, or undefined if redirected to login
   */
  const requireAuth = useCallback(
    async <T>(action: () => Promise<T>): Promise<T | undefined> => {
      // If user is not authenticated, redirect to login immediately
      if (!user) {
        navigate('/login');
        return undefined;
      }

      try {
        return await action();
      } catch (error) {
        // If we get a 401 error, redirect to login
        if (error instanceof ApiError && error.status === 401) {
          navigate('/login');
          return undefined;
        }
        // Re-throw other errors for the caller to handle
        throw error;
      }
    },
    [navigate, user]
  );

  return { requireAuth };
};
