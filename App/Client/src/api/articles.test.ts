import { describe, it, expect, vi, beforeEach } from 'vitest';
import { articlesApi } from './articles';
import * as client from './client';

vi.mock('./client', () => ({
  apiRequest: vi.fn(),
}));

describe('articlesApi', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('listArticles', () => {
    it('should fetch articles without params', async () => {
      const mockResponse = { articles: [], articlesCount: 0 };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const result = await articlesApi.listArticles();

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles');
      expect(result).toEqual(mockResponse);
    });

    it('should fetch articles with tag filter', async () => {
      const mockResponse = { articles: [], articlesCount: 0 };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      await articlesApi.listArticles({ tag: 'react' });

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles?tag=react');
    });

    it('should fetch articles with author filter', async () => {
      vi.mocked(client.apiRequest).mockResolvedValue({ articles: [], articlesCount: 0 });

      await articlesApi.listArticles({ author: 'johndoe' });

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles?author=johndoe');
    });

    it('should fetch articles with favorited filter', async () => {
      vi.mocked(client.apiRequest).mockResolvedValue({ articles: [], articlesCount: 0 });

      await articlesApi.listArticles({ favorited: 'janesmith' });

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles?favorited=janesmith');
    });

    it('should fetch articles with pagination', async () => {
      vi.mocked(client.apiRequest).mockResolvedValue({ articles: [], articlesCount: 0 });

      await articlesApi.listArticles({ limit: 10, offset: 20 });

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles?limit=10&offset=20');
    });
  });

  describe('getFeed', () => {
    it('should fetch user feed with default pagination', async () => {
      const mockResponse = { articles: [], articlesCount: 0 };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const result = await articlesApi.getFeed();

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles/feed?limit=20&offset=0');
      expect(result).toEqual(mockResponse);
    });

    it('should fetch user feed with custom pagination', async () => {
      vi.mocked(client.apiRequest).mockResolvedValue({ articles: [], articlesCount: 0 });

      await articlesApi.getFeed(10, 5);

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles/feed?limit=10&offset=5');
    });
  });

  describe('getArticle', () => {
    it('should fetch a single article by slug', async () => {
      const mockResponse = { article: { slug: 'test-article', title: 'Test' } };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const result = await articlesApi.getArticle('test-article');

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles/test-article');
      expect(result).toEqual(mockResponse);
    });
  });

  describe('createArticle', () => {
    it('should create a new article', async () => {
      const mockResponse = { article: { slug: 'new-article', title: 'New' } };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const articleData = { title: 'New', description: 'Desc', body: 'Body', tagList: [] };
      const result = await articlesApi.createArticle(articleData);

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles', {
        method: 'POST',
        body: JSON.stringify({ article: articleData }),
      });
      expect(result).toEqual(mockResponse);
    });
  });

  describe('updateArticle', () => {
    it('should update an existing article', async () => {
      const mockResponse = { article: { slug: 'updated', title: 'Updated' } };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const updates = { title: 'Updated' };
      const result = await articlesApi.updateArticle('test-slug', updates);

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles/test-slug', {
        method: 'PUT',
        body: JSON.stringify({ article: updates }),
      });
      expect(result).toEqual(mockResponse);
    });
  });

  describe('deleteArticle', () => {
    it('should delete an article', async () => {
      vi.mocked(client.apiRequest).mockResolvedValue(undefined);

      await articlesApi.deleteArticle('test-slug');

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles/test-slug', {
        method: 'DELETE',
      });
    });
  });

  describe('favoriteArticle', () => {
    it('should favorite an article', async () => {
      const mockResponse = { article: { slug: 'test', favorited: true } };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const result = await articlesApi.favoriteArticle('test-slug');

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles/test-slug/favorite', {
        method: 'POST',
      });
      expect(result).toEqual(mockResponse);
    });
  });

  describe('unfavoriteArticle', () => {
    it('should unfavorite an article', async () => {
      const mockResponse = { article: { slug: 'test', favorited: false } };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const result = await articlesApi.unfavoriteArticle('test-slug');

      expect(client.apiRequest).toHaveBeenCalledWith('/api/articles/test-slug/favorite', {
        method: 'DELETE',
      });
      expect(result).toEqual(mockResponse);
    });
  });
});
