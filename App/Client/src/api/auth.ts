import { getApiClient } from './clientFactory';
import { convertKiotaError } from './errors';
import type { UserResponse } from '../types/user';

export const authApi = {
  login: async (email: string, password: string): Promise<void> => {
    try {
      await getApiClient().api.identity.login.post(
        { email, password },
        { queryParameters: { useCookies: true } },
      );
    } catch (error) {
      convertKiotaError(error);
    }
  },

  register: async (email: string, password: string): Promise<void> => {
    try {
      await getApiClient().api.identity.register.post({ email, password });
    } catch (error) {
      convertKiotaError(error);
    }
  },

  logout: async (): Promise<void> => {
    try {
      await getApiClient().api.identity.logout.post();
    } catch (error) {
      convertKiotaError(error);
    }
  },

  getCurrentUser: async (): Promise<UserResponse> => {
    try {
      const result = await getApiClient().api.user.get();
      return result as unknown as UserResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  updateUser: async (updates: {
    email?: string;
    username?: string;
    password?: string;
    bio?: string;
    image?: string;
  }): Promise<UserResponse> => {
    try {
      const result = await getApiClient().api.user.put({ user: updates });
      return result as unknown as UserResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },
};
