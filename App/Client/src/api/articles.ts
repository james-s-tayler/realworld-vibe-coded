import { apiRequest } from './client';
import type {
  ArticleResponse,
  ArticlesResponse,
  CreateArticleRequest,
  UpdateArticleRequest,
  ListArticlesRequest,
} from '../types/article';

export const articlesApi = {
  listArticles: async (params: ListArticlesRequest = {}): Promise<ArticlesResponse> => {
    const queryParams = new URLSearchParams();
    if (params.tag) queryParams.append('tag', params.tag);
    if (params.author) queryParams.append('author', params.author);
    if (params.favorited) queryParams.append('favorited', params.favorited);
    if (params.limit !== undefined) queryParams.append('limit', params.limit.toString());
    if (params.offset !== undefined) queryParams.append('offset', params.offset.toString());

    const query = queryParams.toString();
    return apiRequest<ArticlesResponse>(`/api/articles${query ? `?${query}` : ''}`);
  },

  getFeed: async (limit = 20, offset = 0): Promise<ArticlesResponse> => {
    return apiRequest<ArticlesResponse>(`/api/articles/feed?limit=${limit}&offset=${offset}`);
  },

  getArticle: async (slug: string): Promise<ArticleResponse> => {
    return apiRequest<ArticleResponse>(`/api/articles/${slug}`);
  },

  createArticle: async (article: CreateArticleRequest['article']): Promise<ArticleResponse> => {
    const request: CreateArticleRequest = { article };
    return apiRequest<ArticleResponse>('/api/articles', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },

  updateArticle: async (
    slug: string,
    article: UpdateArticleRequest['article']
  ): Promise<ArticleResponse> => {
    const request: UpdateArticleRequest = { article };
    return apiRequest<ArticleResponse>(`/api/articles/${slug}`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
  },

  deleteArticle: async (slug: string): Promise<void> => {
    await apiRequest<void>(`/api/articles/${slug}`, {
      method: 'DELETE',
    });
  },

  favoriteArticle: async (slug: string): Promise<ArticleResponse> => {
    return apiRequest<ArticleResponse>(`/api/articles/${slug}/favorite`, {
      method: 'POST',
    });
  },

  unfavoriteArticle: async (slug: string): Promise<ArticleResponse> => {
    return apiRequest<ArticleResponse>(`/api/articles/${slug}/favorite`, {
      method: 'DELETE',
    });
  },
};
