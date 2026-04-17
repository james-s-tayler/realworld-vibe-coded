import './CommentSection.scss';

import { TrashCan } from '@carbon/icons-react';
import { Button, Form, IconButton, InlineLoading, TextArea, Tile } from '@carbon/react';
import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';

import { COMMENT_CONSTRAINTS, DEFAULT_PROFILE_IMAGE } from '../constants';
import type { Comment } from '../types/comment';
import type { User } from '../types/user';

interface CommentSectionProps {
  comments: Comment[];
  user: User | null;
  onSubmit: (body: string) => Promise<void>;
  onDelete: (commentId: string) => void;
}

export const CommentSection: React.FC<CommentSectionProps> = ({
  comments,
  user,
  onSubmit,
  onDelete,
}) => {
  const { t } = useTranslation();
  const [commentBody, setCommentBody] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!commentBody.trim()) return;
    setSubmitting(true);
    try {
      await onSubmit(commentBody);
      setCommentBody('');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <>
      {user ? (
        <Tile className="comment-form">
          <Form onSubmit={handleSubmit}>
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
                className="avatar-sm"
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
                className="avatar-sm"
              />
              <span className="comment-author-name cds--text-truncate-end" title={comment.author.username}>{comment.author.username}</span>
            </Link>
            <span className="date-posted">
              {new Date(comment.createdAt).toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
              })}
            </span>
            {user && user.username === comment.author.username && (
              <IconButton
                kind="ghost"
                size="sm"
                label={t('article.comments.delete')}
                onClick={() => onDelete(comment.id)}
              >
                <TrashCan size={16} />
              </IconButton>
            )}
          </div>
        </Tile>
      ))}
    </>
  );
};
