import './AuthorMeta.scss';

import { Stack } from '@carbon/react';
import React from 'react';
import { Link } from 'react-router';

import { DEFAULT_PROFILE_IMAGE } from '../constants';

interface AuthorMetaProps {
  username: string;
  image: string | null;
  date: string;
  variant?: 'default' | 'banner';
  avatarSize?: 'sm' | 'md';
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
};

export const AuthorMeta: React.FC<AuthorMetaProps> = ({
  username,
  image,
  date,
  variant = 'default',
  avatarSize = 'md',
}) => {
  return (
    <Link to={`/profile/${username}`} className={`author-meta ${variant === 'banner' ? 'author-meta--banner' : ''}`}>
      <img
        src={image || DEFAULT_PROFILE_IMAGE}
        alt={username}
        className={`avatar-${avatarSize}`}
      />
      <Stack gap={1} className="author-meta__details">
        <span className="author-meta__name cds--text-truncate-end" title={username}>{username}</span>
        <span className="author-meta__date">{formatDate(date)}</span>
      </Stack>
    </Link>
  );
};
