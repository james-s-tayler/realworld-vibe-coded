import './ArticlePreview.scss';

import { Favorite,FavoriteFilled } from '@carbon/icons-react';
import { Button,Tag } from '@carbon/react';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';

import { DEFAULT_PROFILE_IMAGE } from '../constants';
import type { Article } from '../types/article';

interface ArticlePreviewProps {
  article: Article;
  onFavorite?: (slug: string) => void;
  onUnfavorite?: (slug: string) => void;
}

export const ArticlePreview: React.FC<ArticlePreviewProps> = ({
  article,
  onFavorite,
  onUnfavorite,
}) => {
  const { t } = useTranslation();
  const handleFavoriteClick = (e: React.MouseEvent) => {
    e.preventDefault();
    if (article.favorited && onUnfavorite) {
      onUnfavorite(article.slug);
    } else if (!article.favorited && onFavorite) {
      onFavorite(article.slug);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  return (
    <div className="article-preview">
      <div className="article-meta">
        <Link to={`/profile/${article.author.username}`} className="author-info">
          <img
            src={article.author.image || DEFAULT_PROFILE_IMAGE}
            alt={article.author.username}
            className="author-image"
          />
          <div className="author-details">
            <span className="author-name cds--text-truncate-end" title={article.author.username}>{article.author.username}</span>
            <span className="article-date">{formatDate(article.createdAt)}</span>
          </div>
        </Link>
        <Button
          kind="ghost"
          size="sm"
          className={`favorite-button ${article.favorited ? 'favorited' : ''}`}
          onClick={handleFavoriteClick}
          renderIcon={article.favorited ? FavoriteFilled : Favorite}
        >
          {article.favoritesCount}
        </Button>
      </div>
      <Link to={`/article/${article.slug}`} className="article-link">
        <h2 className="article-title">{article.title}</h2>
        <p className="article-description">{article.description}</p>
        <div className="article-footer">
          <span className="read-more">{t('article.readMore')}</span>
          <div className="article-tags">
            {article.tagList.map((tag) => (
              <Tag key={tag} type="outline" size="sm">
                {tag}
              </Tag>
            ))}
          </div>
        </div>
      </Link>
    </div>
  );
};
