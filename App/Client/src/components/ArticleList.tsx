import React from 'react';
import { Loading } from '@carbon/react';
import { useTranslation } from 'react-i18next';
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
  const { t } = useTranslation();

  if (loading) {
    return (
      <div className="article-list-loading">
        <Loading description={t('articles.loading')} withOverlay={false} />
      </div>
    );
  }

  if (articles.length === 0) {
    return (
      <div className="article-list-empty">
        <p>{t('articles.empty')}</p>
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
