import './ArticlePreview.scss';

import { Favorite,FavoriteFilled } from '@carbon/icons-react';
import { Button,Tag } from '@carbon/react';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';

import type { Article } from '../types/article';
import { AuthorMeta } from './AuthorMeta';

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

  return (
    <div className="article-preview">
      <div className="article-meta">
        <AuthorMeta
          username={article.author.username}
          image={article.author.image}
          date={article.createdAt}
        />
        <Button
          kind={article.favorited ? 'primary' : 'tertiary'}
          size="sm"
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
          <div className="tag-list">
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
