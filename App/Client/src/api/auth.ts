import { apiRequest } from './client';
import type {
  UpdateUserRequest,
  UserResponse,
} from '../types/user';

export const authApi = {
  login: async (email: string, password: string): Promise<string> => {
    const request = { email, password };
    const response = await apiRequest<{ accessToken: string }>('/api/identity/login?useCookies=false', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    return response.accessToken;
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
