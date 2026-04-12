import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router';
import { Grid, Column, Button, TextArea, Loading, Tile, Tag, IconButton, Form, InlineLoading, Breadcrumb, BreadcrumbItem } from '@carbon/react';
import { FavoriteFilled, Favorite, Edit, TrashCan } from '@carbon/icons-react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../hooks/useAuth';
import { useRequireAuth } from '../hooks/useRequireAuth';
import { useToast } from '../hooks/useToast';
import { articlesApi } from '../api/articles';
import { commentsApi } from '../api/comments';
import { profilesApi } from '../api/profiles';
import { PageShell } from '../components/PageShell';
import { ApiError } from '../api/client';
import type { Article } from '../types/article';
import type { Comment } from '../types/comment';
import { DEFAULT_PROFILE_IMAGE, COMMENT_CONSTRAINTS } from '../constants';
import './ArticlePage.scss';

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
}) => {
  const { t } = useTranslation();
  return (
  <div className="banner">
    <Grid>
      <Column lg={16} md={8} sm={4}>
        <h1>{article.title}</h1>
        <div className="article-meta">
        <Link to={`/profile/${article.author.username}`} className="author-info">
          <img src={article.author.image || DEFAULT_PROFILE_IMAGE} alt={article.author.username} />
          <div className="info">
            <span className="author cds--text-truncate-end" title={article.author.username}>{article.author.username}</span>
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
              {t('article.editArticle')}
            </Button>
            <Button
              kind="danger--ghost"
              size="sm"
              renderIcon={TrashCan}
              onClick={onDelete}
            >
              {t('article.deleteArticle')}
            </Button>
          </div>
        ) : (
          <div className="actions">
            <Button
              kind="ghost"
              size="sm"
              onClick={onFollow}
            >
              {article.author.following ? t('article.unfollow', { username: article.author.username }) : t('article.follow', { username: article.author.username })}
            </Button>
            <Button
              kind="ghost"
              size="sm"
              renderIcon={article.favorited ? FavoriteFilled : Favorite}
              onClick={onFavorite}
            >
              {article.favorited ? t('article.unfavorite') : t('article.favorite')} ({article.favoritesCount})
            </Button>
          </div>
        )}
      </div>
      </Column>
    </Grid>
  </div>
  );
};

export const ArticlePage: React.FC = () => {
  const { t } = useTranslation();
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { requireAuth } = useRequireAuth();
  const { showToast } = useToast();
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
    } catch (err) {
      const message = err instanceof ApiError ? err.errors.join(', ') : t('article.notFoundMessage');
      setError(message);
      showToast({ kind: 'error', title: t('article.notFound'), subtitle: message });
    } finally {
      setLoading(false);
    }
  }, [slug, t, showToast]);

  const loadComments = useCallback(async () => {
    if (!slug) return;
    try {
      const response = await commentsApi.getComments(slug);
      setComments(response.comments);
    } catch (err) {
      const message = err instanceof ApiError ? err.errors.join(', ') : t('article.failedToLoadComments');
      showToast({ kind: 'error', title: t('error.title'), subtitle: message });
    }
  }, [slug, t, showToast]);

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
    } catch (err) {
      const message = err instanceof ApiError ? err.errors.join(', ') : t('article.failedToFavorite');
      showToast({ kind: 'error', title: t('error.title'), subtitle: message });
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
    } catch (err) {
      const message = err instanceof ApiError ? err.errors.join(', ') : t('article.failedToFollow');
      showToast({ kind: 'error', title: t('error.title'), subtitle: message });
    }
  };

  const handleDelete = async () => {
    if (!article || !window.confirm('Are you sure you want to delete this article?')) return;
    try {
      await articlesApi.deleteArticle(article.slug);
      navigate('/');
    } catch (err) {
      const message = err instanceof ApiError ? err.errors.join(', ') : t('article.failedToDelete');
      showToast({ kind: 'error', title: t('error.title'), subtitle: message });
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
    } catch (err) {
      const message = err instanceof ApiError ? err.errors.join(', ') : t('article.failedToComment');
      showToast({ kind: 'error', title: t('error.title'), subtitle: message });
    } finally {
      setSubmitting(false);
    }
  };

  const handleCommentDelete = async (id: string) => {
    if (!slug) return;
    try {
      await commentsApi.deleteComment(slug, id);
      setComments(comments.filter(c => c.id !== id));
    } catch (err) {
      const message = err instanceof ApiError ? err.errors.join(', ') : t('article.failedToDeleteComment');
      showToast({ kind: 'error', title: t('error.title'), subtitle: message });
    }
  };

  if (loading) {
    return (
      <PageShell className="article-page loading">
        <Loading description={t('article.loading')} withOverlay={false} />
      </PageShell>
    );
  }

  if (!article) {
    return (
      <PageShell className="article-page" columnLayout="full">
        <p>{error || t('article.notFoundMessage')}</p>
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
      breadcrumbs={
        <Breadcrumb noTrailingSlash>
          <BreadcrumbItem>
            <Link to="/">{t('breadcrumb.home')}</Link>
          </BreadcrumbItem>
          <BreadcrumbItem isCurrentPage title={article.title}>{article.title}</BreadcrumbItem>
        </Breadcrumb>
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
              <Tag key={tag} type="outline" size="sm" as={Link} to={`/?tag=${tag}`}>
                {tag}
              </Tag>
            ))}
          </div>
        </div>

        <hr />

        {user ? (
          <Tile className="comment-form">
              <Form onSubmit={handleCommentSubmit}>
                <div className="comment-form-body">
                  <TextArea
                    id="comment"
                    labelText={t('article.comments.label')}
                    hideLabel
                    placeholder={t('article.comments.placeholder')}
                    value={commentBody}
                    onChange={(e) => setCommentBody(e.target.value)}
                    rows={3}
                    maxLength={COMMENT_CONSTRAINTS.BODY_MAX_LENGTH}
                  />
                </div>
                <div className="comment-form-footer">
                  <img
                    src={user.image || DEFAULT_PROFILE_IMAGE}
                    alt={user.username}
                    className="comment-author-img"
                  />
                  <Button type="submit" size="sm" disabled={submitting || !commentBody.trim()}>
                    {t('article.comments.submit')}
                  </Button>
                  {submitting && <InlineLoading description={t('article.comments.submitting')} />}
                </div>
              </Form>
            </Tile>
          ) : (
            <div className="comment-auth-prompt">
              <p>{t('article.comments.authPrompt')}</p>
            </div>
          )}

          {comments.map(comment => (
            <Tile key={comment.id} className="comment-tile">
              <div className="comment-body">
                <p>{comment.body}</p>
              </div>
              <div className="comment-footer">
                <Link to={`/profile/${comment.author.username}`} className="comment-author">
                  <img
                    src={comment.author.image || DEFAULT_PROFILE_IMAGE}
                    alt={comment.author.username}
                    className="comment-author-img"
                  />
                  <span className="comment-author-name cds--text-truncate-end" title={comment.author.username}>{comment.author.username}</span>
                </Link>
                <span className="date-posted">
                  {new Date(comment.createdAt).toLocaleDateString()}
                </span>
                {user && user.username === comment.author.username && (
                  <IconButton
                    kind="ghost"
                    size="sm"
                    label={t('article.comments.delete')}
                    onClick={() => handleCommentDelete(comment.id)}
                  >
                    <TrashCan size={16} />
                  </IconButton>
                )}
              </div>
            </Tile>
          ))}
    </PageShell>
  );
};
