import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router';
import { Button, TextArea, Loading, InlineNotification } from '@carbon/react';
import { FavoriteFilled, Favorite, Edit, TrashCan } from '@carbon/icons-react';
import { useAuth } from '../hooks/useAuth';
import { useRequireAuth } from '../hooks/useRequireAuth';
import { articlesApi } from '../api/articles';
import { commentsApi } from '../api/comments';
import { profilesApi } from '../api/profiles';
import { PageShell } from '../components/PageShell';
import { ApiError } from '../api/client';
import type { Article } from '../types/article';
import type { Comment } from '../types/comment';
import { DEFAULT_PROFILE_IMAGE, COMMENT_CONSTRAINTS } from '../constants';
import './ArticlePage.css';

interface ArticleBannerProps {
  article: Article;
  isAuthor: boolean;
  onEdit: () => void;
  onDelete: () => void;
  onFollow: () => void;
  onFavorite: () => void;
}

const ArticleBanner: React.FC<ArticleBannerProps> = ({
  article,
  isAuthor,
  onEdit,
  onDelete,
  onFollow,
  onFavorite,
}) => (
  <div className="banner">
    <div className="container">
      <h1>{article.title}</h1>
      <div className="article-meta">
        <Link to={`/profile/${article.author.username}`} className="author-info">
          <img src={article.author.image || DEFAULT_PROFILE_IMAGE} alt={article.author.username} />
          <div className="info">
            <span className="author">{article.author.username}</span>
            <span className="date">{new Date(article.createdAt).toLocaleDateString()}</span>
          </div>
        </Link>
        {isAuthor ? (
          <div className="actions">
            <Button
              kind="ghost"
              size="sm"
              renderIcon={Edit}
              onClick={onEdit}
            >
              Edit Article
            </Button>
            <Button
              kind="danger--ghost"
              size="sm"
              renderIcon={TrashCan}
              onClick={onDelete}
            >
              Delete Article
            </Button>
          </div>
        ) : (
          <div className="actions">
            <Button
              kind="ghost"
              size="sm"
              onClick={onFollow}
            >
              {article.author.following ? 'Unfollow' : 'Follow'} {article.author.username}
            </Button>
            <Button
              kind="ghost"
              size="sm"
              renderIcon={article.favorited ? FavoriteFilled : Favorite}
              onClick={onFavorite}
            >
              {article.favorited ? 'Unfavorite' : 'Favorite'} Article ({article.favoritesCount})
            </Button>
          </div>
        )}
      </div>
    </div>
  </div>
);

export const ArticlePage: React.FC = () => {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { requireAuth } = useRequireAuth();
  const [article, setArticle] = useState<Article | null>(null);
  const [comments, setComments] = useState<Comment[]>([]);
  const [loading, setLoading] = useState(true);
  const [commentBody, setCommentBody] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadArticle = useCallback(async () => {
    if (!slug) return;
    setLoading(true);
    setError(null);
    try {
      const response = await articlesApi.getArticle(slug);
      setArticle(response.article);
    } catch (error) {
      console.error('Failed to load article:', error);
      if (error instanceof ApiError) {
        setError(error.errors.join(', '));
      } else {
        setError('Failed to load article');
      }
    } finally {
      setLoading(false);
    }
  }, [slug]);

  const loadComments = useCallback(async () => {
    if (!slug) return;
    try {
      const response = await commentsApi.getComments(slug);
      setComments(response.comments);
    } catch (error) {
      console.error('Failed to load comments:', error);
    }
  }, [slug]);

  useEffect(() => {
    loadArticle();
    loadComments();
  }, [loadArticle, loadComments]);

  const handleFavorite = async () => {
    if (!article) return;
    try {
      await requireAuth(async () => {
        const response = article.favorited
          ? await articlesApi.unfavoriteArticle(article.slug)
          : await articlesApi.favoriteArticle(article.slug);
        setArticle(response.article);
        return response;
      });
    } catch (error) {
      console.error('Failed to favorite/unfavorite article:', error);
    }
  };

  const handleFollow = async () => {
    if (!article) return;
    try {
      await requireAuth(async () => {
        const response = article.author.following
          ? await profilesApi.unfollowUser(article.author.username)
          : await profilesApi.followUser(article.author.username);
        setArticle({ ...article, author: response.profile });
        return response;
      });
    } catch (error) {
      console.error('Failed to follow/unfollow user:', error);
    }
  };

  const handleDelete = async () => {
    if (!article || !window.confirm('Are you sure you want to delete this article?')) return;
    try {
      await articlesApi.deleteArticle(article.slug);
      navigate('/');
    } catch (error) {
      console.error('Failed to delete article:', error);
    }
  };

  const handleCommentSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!slug || !commentBody.trim()) return;
    setSubmitting(true);
    try {
      const response = await commentsApi.createComment(slug, commentBody);
      setComments([response.comment, ...comments]);
      setCommentBody('');
    } catch (error) {
      console.error('Failed to post comment:', error);
    } finally {
      setSubmitting(false);
    }
  };

  const handleCommentDelete = async (id: number) => {
    if (!slug) return;
    try {
      await commentsApi.deleteComment(slug, id);
      setComments(comments.filter(c => c.id !== id));
    } catch (error) {
      console.error('Failed to delete comment:', error);
    }
  };

  if (loading) {
    return (
      <PageShell className="article-page loading">
        <Loading description="Loading article..." withOverlay={false} />
      </PageShell>
    );
  }

  if (!article) {
    return (
      <PageShell className="article-page" columnLayout="full">
        <InlineNotification
          kind="error"
          title="Article not found"
          subtitle={error || 'The requested article could not be found'}
        />
      </PageShell>
    );
  }

  const isAuthor = user && article.author.username === user.username;

  return (
    <PageShell
      className="article-page"
      columnLayout="full"
      banner={
        <ArticleBanner
          article={article}
          isAuthor={!!isAuthor}
          onEdit={() => navigate(`/editor/${article.slug}`)}
          onDelete={handleDelete}
          onFollow={handleFollow}
          onFavorite={handleFavorite}
        />
      }
    >
      <div className="article-content">
        <div className="article-body">
          {article.body.split('\n').map((paragraph, index) => (
            <p key={index}>{paragraph}</p>
          ))}
        </div>
        <div className="article-tags">
          {article.tagList.map(tag => (
            <Link key={tag} to={`/?tag=${tag}`} className="tag-pill">
              {tag}
            </Link>
          ))}
        </div>
      </div>

      <hr />

      <div className="row">
        <div className="col-xs-12 col-md-8 offset-md-2">
          {user ? (
            <form className="card comment-form" onSubmit={handleCommentSubmit}>
              <div className="card-block">
                <TextArea
                  id="comment"
                  labelText=""
                  placeholder="Write a comment..."
                  value={commentBody}
                  onChange={(e) => setCommentBody(e.target.value)}
                  rows={3}
                  maxLength={COMMENT_CONSTRAINTS.BODY_MAX_LENGTH}
                />
              </div>
              <div className="card-footer">
                <img
                  src={user.image || DEFAULT_PROFILE_IMAGE}
                  alt={user.username}
                  className="comment-author-img"
                />
                <Button type="submit" size="sm" disabled={submitting || !commentBody.trim()}>
                  Post Comment
                </Button>
              </div>
            </form>
          ) : (
            <div className="row">
              <div className="col-xs-12 col-md-8 offset-md-2">
                <p>
                  <Link to="/login">Sign in</Link> or <Link to="/register">sign up</Link> to add
                  comments on this article.
                </p>
              </div>
            </div>
          )}

          {comments.map(comment => (
            <div key={comment.id} className="card">
              <div className="card-block">
                <p className="card-text">{comment.body}</p>
              </div>
              <div className="card-footer">
                <Link to={`/profile/${comment.author.username}`} className="comment-author">
                  <img
                    src={comment.author.image || DEFAULT_PROFILE_IMAGE}
                    alt={comment.author.username}
                    className="comment-author-img"
                  />
                  <span className="comment-author-name">{comment.author.username}</span>
                </Link>
                <span className="date-posted">
                  {new Date(comment.createdAt).toLocaleDateString()}
                </span>
                {user && user.username === comment.author.username && (
                  <button
                    className="mod-options"
                    onClick={() => handleCommentDelete(comment.id)}
                  >
                    <TrashCan size={16} />
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>
    </PageShell>
  );
};
