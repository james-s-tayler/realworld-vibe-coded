import { apiRequest } from './client';
import type {
  LoginRequest,
  RegisterRequest,
  UpdateUserRequest,
  UserResponse,
} from '../types/user';

export const authApi = {
  login: async (email: string, password: string): Promise<UserResponse> => {
    const request: LoginRequest = {
      user: { email, password },
    };
    return apiRequest<UserResponse>('/api/users/login', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },

  register: async (
    email: string,
    password: string
  ): Promise<void> => {
    const request: RegisterRequest = {
      user: { email, password },
    };
    await apiRequest<UserResponse>('/api/users', {
      method: 'POST',
      body: JSON.stringify(request),
    });
    // No token extraction - will login separately
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
