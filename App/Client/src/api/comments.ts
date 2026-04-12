import type {
  CommentResponse,
  CommentsResponse,
} from '../types/comment';
import { getApiClient } from './clientFactory';
import { convertKiotaError } from './errors';

export const commentsApi = {
  getComments: async (slug: string): Promise<CommentsResponse> => {
    try {
      const result = await getApiClient().api.articles.bySlug(slug).comments.get();
      return result as unknown as CommentsResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  createComment: async (slug: string, body: string): Promise<CommentResponse> => {
    try {
      const result = await getApiClient().api.articles.bySlug(slug).comments.post({
        comment: { body },
      });
      return result as unknown as CommentResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  deleteComment: async (slug: string, id: string): Promise<void> => {
    try {
      await getApiClient().api.articles.bySlug(slug).comments.byId(id).delete();
    } catch (error) {
      convertKiotaError(error);
    }
  },
};
