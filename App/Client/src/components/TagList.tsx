import React from 'react';
import { Tag, SkeletonText } from '@carbon/react';
import { useTranslation } from 'react-i18next';
import './TagList.scss';

interface TagListProps {
  tags: string[];
  loading?: boolean;
  onTagClick?: (tag: string) => void;
}

export const TagList: React.FC<TagListProps> = ({ tags, loading, onTagClick }) => {
  const { t } = useTranslation();

  if (loading) {
    return (
      <div className="tag-list-loading">
        <SkeletonText />
      </div>
    );
  }

  if (tags.length === 0) {
    return (
      <div className="tag-list-empty">
        <p>{t('tags.empty')}</p>
      </div>
    );
  }

  return (
    <div className="tag-list">
      {tags.map((tag) => (
        <Tag
          key={tag}
          type="outline"
          className="tag-pill"
          onClick={() => onTagClick && onTagClick(tag)}
        >
          {tag}
        </Tag>
      ))}
    </div>
  );
};
