import { getApiClient } from './clientFactory';
import { convertKiotaError } from './errors';
import type { ProfileResponse } from '../types/profile';

export const profilesApi = {
  getProfile: async (username: string): Promise<ProfileResponse> => {
    try {
      const result = await getApiClient().api.profiles.byUsername(username).get();
      return result as unknown as ProfileResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },
};
