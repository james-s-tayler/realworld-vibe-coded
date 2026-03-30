import { getApiClient } from './clientFactory';
import { convertKiotaError } from './errors';

export interface User {
  id: string;
  email: string;
  username: string;
  bio: string;
  image: string | null;
  roles: string[];
  isActive: boolean;
}

export interface UsersResponse {
  users: User[];
  usersCount: number;
}

export const usersApi = {
  listUsers: async (limit?: number, offset?: number): Promise<UsersResponse> => {
    try {
      const result = await getApiClient().api.users.get({
        queryParameters: { limit, offset },
      });
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

  deactivateUser: async (userId: string): Promise<void> => {
    try {
      await getApiClient().api.users.byUserId(userId).deactivate.put();
    } catch (error) {
      convertKiotaError(error);
    }
  },

  reactivateUser: async (userId: string): Promise<void> => {
    try {
      await getApiClient().api.users.byUserId(userId).reactivate.put();
    } catch (error) {
      convertKiotaError(error);
    }
  },

  updateUserRoles: async (userId: string, roles: string[]): Promise<void> => {
    try {
      await getApiClient().api.users.byUserId(userId).roles.put({ roles });
    } catch (error) {
      convertKiotaError(error);
    }
  },
};
