import React from 'react';
import { Loading } from '@carbon/react';
import { ArticlePreview } from './ArticlePreview';
import type { Article } from '../types/article';
import './ArticleList.css';

interface ArticleListProps {
  articles: Article[];
  loading?: boolean;
  onFavorite?: (slug: string) => void;
  onUnfavorite?: (slug: string) => void;
}

export const ArticleList: React.FC<ArticleListProps> = ({
  articles,
  loading,
  onFavorite,
  onUnfavorite,
}) => {
  if (loading) {
    return (
      <div className="article-list-loading">
        <Loading description="Loading articles..." withOverlay={false} />
      </div>
    );
  }

  if (articles.length === 0) {
    return (
      <div className="article-list-empty">
        <p>No articles are here... yet.</p>
      </div>
    );
  }

  return (
    <div className="article-list">
      {articles.map((article) => (
        <ArticlePreview
          key={article.slug}
          article={article}
          onFavorite={onFavorite}
          onUnfavorite={onUnfavorite}
        />
      ))}
    </div>
  );
};
