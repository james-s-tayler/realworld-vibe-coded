import React from 'react';
import { Link } from 'react-router';
import { Tag, Button } from '@carbon/react';
import { FavoriteFilled, Favorite } from '@carbon/icons-react';
import type { Article } from '../types/article';
import './ArticlePreview.css';

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
            src={article.author.image || '/default-avatar.png'}
            alt={article.author.username}
            className="author-image"
          />
          <div className="author-details">
            <span className="author-name">{article.author.username}</span>
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
          <span className="read-more">Read more...</span>
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
