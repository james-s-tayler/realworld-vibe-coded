import { beforeEach,describe, expect, it, vi } from 'vitest';

import { getApiClient } from './clientFactory';
import { commentsApi } from './comments';

vi.mock('./clientFactory');

function createMockClient() {
  const mockCommentsGet = vi.fn();
  const mockCommentsPost = vi.fn();
  const mockCommentDelete = vi.fn();
  const mockBySlug = vi.fn();
  const mockById = vi.fn();

  mockById.mockReturnValue({ delete: mockCommentDelete });

  mockBySlug.mockReturnValue({
    comments: {
      get: mockCommentsGet,
      post: mockCommentsPost,
      byId: mockById,
    },
  });

  const client = {
    api: {
      articles: {
        bySlug: mockBySlug,
      },
    },
  };

  return {
    client,
    mocks: {
      commentsGet: mockCommentsGet,
      commentsPost: mockCommentsPost,
      commentDelete: mockCommentDelete,
      bySlug: mockBySlug,
      byId: mockById,
    },
  };
}

describe('commentsApi', () => {
  let mocks: ReturnType<typeof createMockClient>['mocks'];

  beforeEach(() => {
    vi.clearAllMocks();
    const mock = createMockClient();
    mocks = mock.mocks;
    vi.mocked(getApiClient).mockReturnValue(mock.client as ReturnType<typeof getApiClient>);
  });

  describe('getComments', () => {
    it('should fetch comments for an article', async () => {
      const mockResponse = { comments: [{ id: 'abc-123', body: 'Test comment' }] };
      mocks.commentsGet.mockResolvedValue(mockResponse);

      const result = await commentsApi.getComments('test-article');

      expect(mocks.bySlug).toHaveBeenCalledWith('test-article');
      expect(result).toEqual(mockResponse);
    });
  });

  describe('createComment', () => {
    it('should create a comment on an article', async () => {
      const mockResponse = { comment: { id: 'abc-123', body: 'New comment' } };
      mocks.commentsPost.mockResolvedValue(mockResponse);

      const result = await commentsApi.createComment('test-article', 'New comment');

      expect(mocks.bySlug).toHaveBeenCalledWith('test-article');
      expect(mocks.commentsPost).toHaveBeenCalledWith({
        comment: { body: 'New comment' },
      });
      expect(result).toEqual(mockResponse);
    });
  });

  describe('deleteComment', () => {
    it('should delete a comment', async () => {
      mocks.commentDelete.mockResolvedValue(undefined);

      await commentsApi.deleteComment('test-article', 'abc-123');

      expect(mocks.bySlug).toHaveBeenCalledWith('test-article');
      expect(mocks.byId).toHaveBeenCalledWith('abc-123');
      expect(mocks.commentDelete).toHaveBeenCalled();
    });
  });
});
