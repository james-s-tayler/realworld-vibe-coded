import { apiRequest } from './client';
import type {
  UpdateUserRequest,
  UserResponse,
} from '../types/user';

export const authApi = {
  login: async (email: string, password: string): Promise<void> => {
    const request = { email, password };
    await apiRequest<void>('/api/identity/login?useCookies=true', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    // No token returned - cookie is automatically set by browser
  },

  register: async (
    email: string,
    password: string
  ): Promise<void> => {
    const request = { email, password };
    await apiRequest<void>('/api/identity/register', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    // No token returned from Identity register
  },

  logout: async (): Promise<void> => {
    await apiRequest<void>('/api/identity/logout', {
      method: 'POST',
    });
    // Cookie is cleared by the server
  },

  getCurrentUser: async (): Promise<UserResponse> => {
    return apiRequest<UserResponse>('/api/user', {
      method: 'GET',
    });
  },

  updateUser: async (updates: {
    email?: string;
    username?: string;
    password?: string;
    bio?: string;
    image?: string;
  }): Promise<UserResponse> => {
    const request: UpdateUserRequest = {
      user: updates,
    };
    return apiRequest<UserResponse>('/api/user', {
      method: 'PUT',
      body: JSON.stringify(request),
    });
  },
};
