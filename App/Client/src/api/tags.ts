import { apiRequest } from './client';
import type { TagsResponse } from '../types/tag';

export const tagsApi = {
  getTags: async (): Promise<TagsResponse> => {
    return apiRequest<TagsResponse>('/api/tags');
  },
};
