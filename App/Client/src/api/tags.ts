import type { TagsResponse } from '../types/tag';
import { getApiClient } from './clientFactory';
import { convertKiotaError } from './errors';

export const tagsApi = {
  getTags: async (): Promise<TagsResponse> => {
    try {
      const result = await getApiClient().api.tags.get();
      return result as unknown as TagsResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },
};
