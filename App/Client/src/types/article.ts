export interface Article {
  slug: string;
  title: string;
  description: string;
  body: string;
  tagList: string[];
  createdAt: string;
  updatedAt: string;
  favorited: boolean;
  favoritesCount: number;
  author: Profile;
}

export interface Profile {
  username: string;
  bio: string;
  image: string | null;
  following: boolean;
}

export interface ArticleResponse {
  article: Article;
}

export interface ArticlesResponse {
  articles: Article[];
  articlesCount: number;
}

export interface CreateArticleRequest {
  article: {
    title: string;
    description: string;
    body: string;
    tagList?: string[];
  };
}

export interface UpdateArticleRequest {
  article: {
    title?: string;
    description?: string;
    body?: string;
    tagList?: string[];
  };
}

export interface ListArticlesRequest {
  tag?: string;
  author?: string;
  favorited?: string;
  limit?: number;
  offset?: number;
}
