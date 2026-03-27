import { getApiClient } from './clientFactory';
import { convertKiotaError } from './errors';
import type {
  ArticleResponse,
  ArticlesResponse,
  CreateArticleData,
  UpdateArticleData,
} from './generated/models/index.js';

export interface ListArticlesParams {
  tag?: string;
  author?: string;
  favorited?: string;
  limit?: number;
  offset?: number;
}

export interface FeedParams {
  limit?: number;
  offset?: number;
}

export const articlesApi = {
  listArticles: async (params: ListArticlesParams = {}): Promise<ArticlesResponse> => {
    try {
      const result = await getApiClient().api.articles.get({
        queryParameters: {
          tag: params.tag,
          author: params.author,
          favorited: params.favorited,
          limit: params.limit,
          offset: params.offset,
        },
      });
      return result as unknown as ArticlesResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  getFeed: async (params: FeedParams = {}): Promise<ArticlesResponse> => {
    try {
      const result = await getApiClient().api.articles.feed.get({
        queryParameters: {
          limit: params.limit,
          offset: params.offset,
        },
      });
      return result as unknown as ArticlesResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  getArticle: async (slug: string): Promise<ArticleResponse> => {
    try {
      const result = await getApiClient().api.articles.bySlug(slug).get();
      return result as unknown as ArticleResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  createArticle: async (data: CreateArticleData): Promise<ArticleResponse> => {
    try {
      const result = await getApiClient().api.articles.post({ article: data });
      return result as unknown as ArticleResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  updateArticle: async (slug: string, data: UpdateArticleData): Promise<ArticleResponse> => {
    try {
      const result = await getApiClient().api.articles.bySlug(slug).put({ article: data });
      return result as unknown as ArticleResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  deleteArticle: async (slug: string): Promise<void> => {
    try {
      await getApiClient().api.articles.bySlug(slug).delete();
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  favoriteArticle: async (slug: string): Promise<ArticleResponse> => {
    try {
      const result = await getApiClient().api.articles.bySlug(slug).favorite.post();
      return result as unknown as ArticleResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  unfavoriteArticle: async (slug: string): Promise<ArticleResponse> => {
    try {
      const result = await getApiClient().api.articles.bySlug(slug).favorite.delete();
      return result as unknown as ArticleResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },
};
