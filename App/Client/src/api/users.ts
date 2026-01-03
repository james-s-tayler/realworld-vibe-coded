import { apiRequest } from './client';

export interface User {
  email: string;
  username: string;
  bio: string;
  image: string | null;
}

export interface UsersResponse {
  users: User[];
}

export interface InviteRequest {
  email: string;
  password: string;
}

export const usersApi = {
  listUsers: async (): Promise<UsersResponse> => {
    return apiRequest<UsersResponse>('/api/users', {
      method: 'GET',
    });
  },

  inviteUser: async (email: string, password: string): Promise<void> => {
    const request: InviteRequest = { email, password };
    await apiRequest<void>('/api/identity/invite', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },
};
