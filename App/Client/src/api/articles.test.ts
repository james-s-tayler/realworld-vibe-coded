import { describe, it, expect, vi, beforeEach } from 'vitest';
import { articlesApi } from './articles';
import { getApiClient } from './clientFactory';

vi.mock('./clientFactory');

function createMockClient() {
  const mockArticleGet = vi.fn();
  const mockArticlePost = vi.fn();
  const mockArticlePut = vi.fn();
  const mockArticleDelete = vi.fn();
  const mockFavoritePost = vi.fn();
  const mockFavoriteDelete = vi.fn();
  const mockFeedGet = vi.fn();
  const mockBySlug = vi.fn();

  const slugMethods = {
    get: mockArticleGet,
    put: mockArticlePut,
    delete: mockArticleDelete,
    favorite: {
      post: mockFavoritePost,
      delete: mockFavoriteDelete,
    },
  };

  mockBySlug.mockReturnValue(slugMethods);

  const client = {
    api: {
      articles: {
        get: vi.fn(),
        post: mockArticlePost,
        feed: { get: mockFeedGet },
        bySlug: mockBySlug,
      },
    },
  };

  return {
    client,
    mocks: {
      listGet: client.api.articles.get,
      articleGet: mockArticleGet,
      articlePost: mockArticlePost,
      articlePut: mockArticlePut,
      articleDelete: mockArticleDelete,
      favoritePost: mockFavoritePost,
      favoriteDelete: mockFavoriteDelete,
      feedGet: mockFeedGet,
      bySlug: mockBySlug,
    },
  };
}

describe('articlesApi', () => {
  let mocks: ReturnType<typeof createMockClient>['mocks'];

  beforeEach(() => {
    vi.clearAllMocks();
    const mock = createMockClient();
    mocks = mock.mocks;
    vi.mocked(getApiClient).mockReturnValue(mock.client as ReturnType<typeof getApiClient>);
  });

  describe('listArticles', () => {
    it('should fetch articles without params', async () => {
      const mockResponse = { articles: [], articlesCount: 0 };
      mocks.listGet.mockResolvedValue(mockResponse);

      const result = await articlesApi.listArticles();

      expect(mocks.listGet).toHaveBeenCalledWith({
        queryParameters: {
          tag: undefined,
          author: undefined,
          favorited: undefined,
          limit: undefined,
          offset: undefined,
        },
      });
      expect(result).toEqual(mockResponse);
    });

    it('should fetch articles with tag filter', async () => {
      const mockResponse = { articles: [], articlesCount: 0 };
      mocks.listGet.mockResolvedValue(mockResponse);

      await articlesApi.listArticles({ tag: 'react' });

      expect(mocks.listGet).toHaveBeenCalledWith(
        expect.objectContaining({
          queryParameters: expect.objectContaining({ tag: 'react' }),
        }),
      );
    });

    it('should fetch articles with pagination', async () => {
      mocks.listGet.mockResolvedValue({ articles: [], articlesCount: 0 });

      await articlesApi.listArticles({ limit: 10, offset: 20 });

      expect(mocks.listGet).toHaveBeenCalledWith(
        expect.objectContaining({
          queryParameters: expect.objectContaining({ limit: 10, offset: 20 }),
        }),
      );
    });
  });

  describe('getFeed', () => {
    it('should fetch user feed with default pagination', async () => {
      const mockResponse = { articles: [], articlesCount: 0 };
      mocks.feedGet.mockResolvedValue(mockResponse);

      const result = await articlesApi.getFeed();

      expect(mocks.feedGet).toHaveBeenCalledWith({
        queryParameters: { limit: 20, offset: 0 },
      });
      expect(result).toEqual(mockResponse);
    });

    it('should fetch user feed with custom pagination', async () => {
      mocks.feedGet.mockResolvedValue({ articles: [], articlesCount: 0 });

      await articlesApi.getFeed(10, 5);

      expect(mocks.feedGet).toHaveBeenCalledWith({
        queryParameters: { limit: 10, offset: 5 },
      });
    });
  });

  describe('getArticle', () => {
    it('should fetch a single article by slug', async () => {
      const mockResponse = { article: { slug: 'test-article', title: 'Test' } };
      mocks.articleGet.mockResolvedValue(mockResponse);

      const result = await articlesApi.getArticle('test-article');

      expect(mocks.bySlug).toHaveBeenCalledWith('test-article');
      expect(result).toEqual(mockResponse);
    });
  });

  describe('createArticle', () => {
    it('should create a new article', async () => {
      const mockResponse = { article: { slug: 'new-article', title: 'New' } };
      mocks.articlePost.mockResolvedValue(mockResponse);

      const articleData = { title: 'New', description: 'Desc', body: 'Body', tagList: [] };
      const result = await articlesApi.createArticle(articleData);

      expect(mocks.articlePost).toHaveBeenCalledWith({ article: articleData });
      expect(result).toEqual(mockResponse);
    });
  });

  describe('updateArticle', () => {
    it('should update an existing article', async () => {
      const mockResponse = { article: { slug: 'updated', title: 'Updated' } };
      mocks.articlePut.mockResolvedValue(mockResponse);

      const updates = { title: 'Updated' };
      const result = await articlesApi.updateArticle('test-slug', updates);

      expect(mocks.bySlug).toHaveBeenCalledWith('test-slug');
      expect(mocks.articlePut).toHaveBeenCalledWith({ article: updates });
      expect(result).toEqual(mockResponse);
    });
  });

  describe('deleteArticle', () => {
    it('should delete an article', async () => {
      mocks.articleDelete.mockResolvedValue(undefined);

      await articlesApi.deleteArticle('test-slug');

      expect(mocks.bySlug).toHaveBeenCalledWith('test-slug');
      expect(mocks.articleDelete).toHaveBeenCalled();
    });
  });

  describe('favoriteArticle', () => {
    it('should favorite an article', async () => {
      const mockResponse = { article: { slug: 'test', favorited: true } };
      mocks.favoritePost.mockResolvedValue(mockResponse);

      const result = await articlesApi.favoriteArticle('test-slug');

      expect(mocks.bySlug).toHaveBeenCalledWith('test-slug');
      expect(mocks.favoritePost).toHaveBeenCalled();
      expect(result).toEqual(mockResponse);
    });
  });

  describe('unfavoriteArticle', () => {
    it('should unfavorite an article', async () => {
      const mockResponse = { article: { slug: 'test', favorited: false } };
      mocks.favoriteDelete.mockResolvedValue(mockResponse);

      const result = await articlesApi.unfavoriteArticle('test-slug');

      expect(mocks.bySlug).toHaveBeenCalledWith('test-slug');
      expect(mocks.favoriteDelete).toHaveBeenCalled();
      expect(result).toEqual(mockResponse);
    });
  });
});
