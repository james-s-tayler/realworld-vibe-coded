import { describe, it, expect, vi, beforeEach } from 'vitest';
import { commentsApi } from './comments';
import * as client from './client';

vi.mock('./client', () => ({
  apiRequest: vi.fn(),
}));

describe('commentsApi', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getComments', () => {
    it('should fetch comments for an article', async () => {
      const mockResponse = { comments: [{ id: 1, body: 'Test comment' }] };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const result = await commentsApi.getComments('test-article');

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles/test-article/comments');
      expect(result).toEqual(mockResponse);
    });
  });

  describe('createComment', () => {
    it('should create a comment on an article', async () => {
      const mockResponse = { comment: { id: 1, body: 'New comment' } };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const result = await commentsApi.createComment('test-article', 'New comment');

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles/test-article/comments', {
        method: 'POST',
        body: JSON.stringify({ comment: { body: 'New comment' } }),
      });
      expect(result).toEqual(mockResponse);
    });
  });

  describe('deleteComment', () => {
    it('should delete a comment', async () => {
      vi.mocked(client.apiRequest).mockResolvedValue(undefined);

      await commentsApi.deleteComment('test-article', 123);

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles/test-article/comments/123', {
        method: 'DELETE',
      });
    });
  });
});
