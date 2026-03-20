import { getApiClient } from './clientFactory';
import { convertKiotaError } from './errors';
import type {
  ArticleResponse,
  ArticlesResponse,
  CreateArticleRequest,
  UpdateArticleRequest,
  ListArticlesRequest,
} from '../types/article';

export const articlesApi = {
  listArticles: async (params: ListArticlesRequest = {}): Promise<ArticlesResponse> => {
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

  getFeed: async (limit = 20, offset = 0): Promise<ArticlesResponse> => {
    try {
      const result = await getApiClient().api.articles.feed.get({
        queryParameters: { limit, offset },
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

  createArticle: async (article: CreateArticleRequest['article']): Promise<ArticleResponse> => {
    try {
      const result = await getApiClient().api.articles.post({ article });
      return result as unknown as ArticleResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  updateArticle: async (
    slug: string,
    article: UpdateArticleRequest['article']
  ): Promise<ArticleResponse> => {
    try {
      const result = await getApiClient().api.articles.bySlug(slug).put({ article });
      return result as unknown as ArticleResponse;
    } catch (error) {
      return convertKiotaError(error);
    }
  },

  deleteArticle: async (slug: string): Promise<void> => {
    try {
      await getApiClient().api.articles.bySlug(slug).delete();
    } catch (error) {
      convertKiotaError(error);
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
