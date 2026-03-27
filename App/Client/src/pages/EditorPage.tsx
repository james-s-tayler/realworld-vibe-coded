import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router';
import {
  Form,
  TextInput,
  TextArea,
  Button,
  Stack,
  Tag,
  Loading,
} from '@carbon/react';
import { Close } from '@carbon/icons-react';
import { articlesApi } from '../api/articles';
import { ErrorDisplay } from '../components/ErrorDisplay';
import { PageShell } from '../components/PageShell';
import { ARTICLE_CONSTRAINTS } from '../constants';
import type { AppError } from '../utils/errors';
import { normalizeError } from '../utils/errors';
import './EditorPage.css';

export const EditorPage: React.FC = () => {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [body, setBody] = useState('');
  const [tagInput, setTagInput] = useState('');
  const [tagList, setTagList] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<AppError | null>(null);

  const isEditMode = !!slug;

  const loadArticle = useCallback(async () => {
    if (!slug) return;
    setLoading(true);
    try {
      const result = await articlesApi.getArticle(slug);
      if (result.article) {
        setTitle(result.article.title ?? '');
        setDescription(result.article.description ?? '');
        setBody(result.article.body ?? '');
        setTagList(result.article.tagList ?? []);
      }
    } catch (err) {
      setError(normalizeError(err));
    } finally {
      setLoading(false);
    }
  }, [slug]);

  useEffect(() => {
    loadArticle();
  }, [loadArticle]);

  const handleTagKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      const tag = tagInput.trim();
      if (tag && !tagList.includes(tag)) {
        setTagList([...tagList, tag]);
      }
      setTagInput('');
    }
  };

  const removeTag = (tagToRemove: string) => {
    setTagList(tagList.filter((t) => t !== tagToRemove));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      let result;
      if (isEditMode) {
        result = await articlesApi.updateArticle(slug!, {
          title,
          description,
          body,
        });
      } else {
        result = await articlesApi.createArticle({
          title,
          description,
          body,
          tagList,
        });
      }

      if (result.article?.slug) {
        navigate(`/article/${result.article.slug}`);
      }
    } catch (err) {
      setError(normalizeError(err));
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <PageShell className="editor-page">
        <Loading description="Loading article..." withOverlay={false} />
      </PageShell>
    );
  }

  return (
    <PageShell
      className="editor-page"
      columnLayout="narrow"
    >
      <ErrorDisplay
        error={error}
        onClose={() => setError(null)}
      />

      <Form onSubmit={handleSubmit}>
        <Stack gap={6}>
          <TextInput
            id="title"
            labelText=""
            placeholder="Article Title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
            maxLength={ARTICLE_CONSTRAINTS.TITLE_MAX_LENGTH}
          />

          <TextInput
            id="description"
            labelText=""
            placeholder="What's this article about?"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            required
            maxLength={ARTICLE_CONSTRAINTS.DESCRIPTION_MAX_LENGTH}
          />

          <TextArea
            id="body"
            labelText=""
            placeholder="Write your article (in markdown)"
            value={body}
            onChange={(e) => setBody(e.target.value)}
            required
            rows={12}
          />

          <div>
            <TextInput
              id="tags"
              labelText=""
              placeholder="Enter tags"
              value={tagInput}
              onChange={(e) => setTagInput(e.target.value)}
              onKeyDown={handleTagKeyDown}
            />
            {tagList.length > 0 && (
              <div className="tag-list">
                {tagList.map((tag) => (
                  <Tag
                    key={tag}
                    type="cool-gray"
                    size="sm"
                    filter
                    onClose={() => removeTag(tag)}
                    renderIcon={Close}
                  >
                    {tag}
                  </Tag>
                ))}
              </div>
            )}
          </div>

          <Button
            type="submit"
            disabled={submitting}
            size="lg"
            className="pull-xs-right"
          >
            {submitting ? 'Publishing...' : 'Publish Article'}
          </Button>
        </Stack>
      </Form>
    </PageShell>
  );
};
