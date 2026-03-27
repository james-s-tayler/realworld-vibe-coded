import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router';
import {
  Button,
  Loading,
  TextArea,
  Tag,
  Tile,
} from '@carbon/react';
import {
  Favorite,
  FavoriteFilled,
  Edit,
  TrashCan,
  Add,
  Subtract,
} from '@carbon/icons-react';
import { useAuth } from '../hooks/useAuth';
import { articlesApi } from '../api/articles';
import { commentsApi } from '../api/comments';
import { profilesApi } from '../api/profiles';
import { PageShell } from '../components/PageShell';
import { DEFAULT_PROFILE_IMAGE } from '../constants';
import type { ArticleDto, CommentDto } from '../api/generated/models/index.js';
import './ArticlePage.css';

export const ArticlePage: React.FC = () => {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [article, setArticle] = useState<ArticleDto | null>(null);
  const [comments, setComments] = useState<CommentDto[]>([]);
  const [commentBody, setCommentBody] = useState('');
  const [loading, setLoading] = useState(true);
  const [submittingComment, setSubmittingComment] = useState(false);
  const [favoritingArticle, setFavoritingArticle] = useState(false);
  const [togglingFollow, setTogglingFollow] = useState(false);
  const [deletingArticle, setDeletingArticle] = useState(false);

  const loadArticle = useCallback(async () => {
    if (!slug) return;
    setLoading(true);
    try {
      const result = await articlesApi.getArticle(slug);
      setArticle(result.article ?? null);
    } catch {
      setArticle(null);
    } finally {
      setLoading(false);
    }
  }, [slug]);

  const loadComments = useCallback(async () => {
    if (!slug) return;
    try {
      const result = await commentsApi.getComments(slug);
      setComments(result.comments ?? []);
    } catch {
      setComments([]);
    }
  }, [slug]);

  useEffect(() => {
    loadArticle();
    loadComments();
  }, [loadArticle, loadComments]);

  const handleFavorite = async () => {
    if (!article?.slug || favoritingArticle) return;
    setFavoritingArticle(true);
    try {
      const result = article.favorited
        ? await articlesApi.unfavoriteArticle(article.slug)
        : await articlesApi.favoriteArticle(article.slug);
      if (result.article) {
        setArticle(result.article);
      }
    } catch {
      // ignore
    } finally {
      setFavoritingArticle(false);
    }
  };

  const handleFollow = async () => {
    if (!article?.author?.username || togglingFollow) return;
    setTogglingFollow(true);
    try {
      const result = article.author.following
        ? await profilesApi.unfollowUser(article.author.username)
        : await profilesApi.followUser(article.author.username);
      if (result.profile && article) {
        setArticle({
          ...article,
          author: {
            ...article.author,
            following: result.profile.following ?? false,
          },
        });
      }
    } catch {
      // ignore
    } finally {
      setTogglingFollow(false);
    }
  };

  const handleDeleteArticle = async () => {
    if (!article?.slug || deletingArticle) return;
    setDeletingArticle(true);
    try {
      await articlesApi.deleteArticle(article.slug);
      navigate('/');
    } catch {
      setDeletingArticle(false);
    }
  };

  const handlePostComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!slug || !commentBody.trim() || submittingComment) return;
    setSubmittingComment(true);
    try {
      const result = await commentsApi.createComment(slug, commentBody.trim());
      if (result.comment) {
        setComments((prev) => [result.comment!, ...prev]);
        setCommentBody('');
      }
    } catch {
      // ignore
    } finally {
      setSubmittingComment(false);
    }
  };

  const handleDeleteComment = async (commentId: string) => {
    if (!slug || !commentId) return;
    try {
      await commentsApi.deleteComment(slug, commentId);
      setComments((prev) => prev.filter((c) => c.id !== commentId));
    } catch {
      // ignore
    }
  };

  const formatDate = (date: Date | null | undefined) => {
    if (!date) return '';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  if (loading) {
    return (
      <div className="article-page loading">
        <Loading description="Loading article..." withOverlay={false} />
      </div>
    );
  }

  if (!article) {
    return (
      <PageShell className="article-page">
        <p>Article not found.</p>
      </PageShell>
    );
  }

  const isAuthor = user && article.author?.username === user.username;

  const banner = (
    <div className="banner">
      <div className="article-grid-container">
        <h1>{article.title}</h1>
        <div className="article-meta">
          <Link to={`/profile/${article.author?.username}`} className="author-info">
            <img
              src={article.author?.image || DEFAULT_PROFILE_IMAGE}
              alt={article.author?.username ?? ''}
            />
            <div className="info">
              <span className="author">{article.author?.username}</span>
              <span className="date">{formatDate(article.createdAt)}</span>
            </div>
          </Link>
          <div className="actions">
            {!isAuthor && (
              <Button
                kind={article.author?.following ? 'secondary' : 'ghost'}
                size="sm"
                renderIcon={article.author?.following ? Subtract : Add}
                onClick={handleFollow}
                disabled={togglingFollow}
              >
                {article.author?.following
                  ? `Unfollow ${article.author?.username}`
                  : `Follow ${article.author?.username}`}
              </Button>
            )}
            <Button
              kind={article.favorited ? 'primary' : 'ghost'}
              size="sm"
              renderIcon={article.favorited ? FavoriteFilled : Favorite}
              onClick={handleFavorite}
              disabled={favoritingArticle}
            >
              {article.favorited ? 'Unfavorite Article' : 'Favorite Article'}
              {' '}({article.favoritesCount ?? 0})
            </Button>
            {isAuthor && (
              <>
                <Link to={`/editor/${article.slug}`}>
                  <Button kind="secondary" size="sm" renderIcon={Edit}>
                    Edit Article
                  </Button>
                </Link>
                <Button
                  kind="danger"
                  size="sm"
                  renderIcon={TrashCan}
                  onClick={handleDeleteArticle}
                  disabled={deletingArticle}
                >
                  Delete Article
                </Button>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );

  return (
    <PageShell className="article-page" banner={banner} columnLayout="wide">
      <div className="article-content">
        <div className="article-body">
          {article.body?.split('\n').map((paragraph, index) => (
            <p key={index}>{paragraph}</p>
          ))}
        </div>

        {article.tagList && article.tagList.length > 0 && (
          <div className="article-tags">
            {article.tagList.map((tag) => (
              <Tag key={tag} type="cool-gray" size="sm">
                {tag}
              </Tag>
            ))}
          </div>
        )}

        <hr />

        <div className="comments-section">
          <form className="comment-form" onSubmit={handlePostComment}>
            <div className="comment-form-body">
              <TextArea
                id="comment-body"
                labelText=""
                placeholder="Write a comment..."
                value={commentBody}
                onChange={(e) => setCommentBody(e.target.value)}
                rows={3}
              />
            </div>
            <div className="comment-form-footer">
              <img
                src={user?.image || DEFAULT_PROFILE_IMAGE}
                alt={user?.username ?? ''}
                className="comment-author-img"
              />
              <Button
                type="submit"
                size="sm"
                disabled={submittingComment || !commentBody.trim()}
              >
                Post Comment
              </Button>
            </div>
          </form>

          {comments.map((comment) => (
            <Tile key={comment.id} className="comment-tile">
              <div className="comment-body">
                <p>{comment.body}</p>
              </div>
              <div className="comment-footer">
                <Link to={`/profile/${comment.author?.username}`} className="comment-author">
                  <img
                    src={comment.author?.image || DEFAULT_PROFILE_IMAGE}
                    alt={comment.author?.username ?? ''}
                    className="comment-author-img"
                  />
                  <span className="comment-author-name">{comment.author?.username}</span>
                </Link>
                <span className="date-posted">{formatDate(comment.createdAt)}</span>
                {user && comment.author?.username === user.username && (
                  <Button
                    kind="danger--ghost"
                    size="sm"
                    hasIconOnly
                    renderIcon={TrashCan}
                    iconDescription="Delete comment"
                    aria-label="Delete comment"
                    onClick={() => handleDeleteComment(comment.id ?? '')}
                  />
                )}
              </div>
            </Tile>
          ))}
        </div>
      </div>
    </PageShell>
  );
};
