import { getApiClient } from './clientFactory';
import { convertKiotaError } from './errors';

export interface User {
  email: string;
  username: string;
  bio: string;
  image: string | null;
  roles: string[];
}

export interface UsersResponse {
  users: User[];
}

export const usersApi = {
  listUsers: async (): Promise<UsersResponse> => {
    try {
      const result = await getApiClient().api.users.get();
      return result as unknown as UsersResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  inviteUser: async (email: string, password: string): Promise<void> => {
    try {
      await getApiClient().api.identity.invite.post({ email, password });
    } catch (error) {
      convertKiotaError(error);
    }
  },
};
