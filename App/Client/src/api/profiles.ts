import { apiRequest } from './client';
import type { ProfileResponse } from '../types/profile';

export const profilesApi = {
  getProfile: async (username: string): Promise<ProfileResponse> => {
    return apiRequest<ProfileResponse>(`/api/profiles/${username}`);
  },

  followUser: async (username: string): Promise<ProfileResponse> => {
    return apiRequest<ProfileResponse>(`/api/profiles/${username}/follow`, {
      method: 'POST',
    });
  },

  unfollowUser: async (username: string): Promise<ProfileResponse> => {
    return apiRequest<ProfileResponse>(`/api/profiles/${username}/follow`, {
      method: 'DELETE',
    });
  },
};
