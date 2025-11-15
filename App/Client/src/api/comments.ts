import { apiRequest } from './client';
import type {
  CommentResponse,
  CommentsResponse,
  CreateCommentRequest,
} from '../types/comment';

export const commentsApi = {
  getComments: async (slug: string): Promise<CommentsResponse> => {
    return apiRequest<CommentsResponse>(`/api/articles/${slug}/comments`);
  },

  createComment: async (slug: string, body: string): Promise<CommentResponse> => {
    const request: CreateCommentRequest = {
      comment: { body },
    };
    return apiRequest<CommentResponse>(`/api/articles/${slug}/comments`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },

  deleteComment: async (slug: string, id: number): Promise<void> => {
    await apiRequest<void>(`/api/articles/${slug}/comments/${id}`, {
      method: 'DELETE',
    });
  },
};
