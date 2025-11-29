import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router';
import {
  Form,
  TextInput,
  TextArea,
  Button,
  Stack,
  Tag,
} from '@carbon/react';
import { articlesApi } from '../api/articles';
import { useApiCall } from '../hooks/useApiCall';
import { PageShell } from '../components/PageShell';
import { RequestBoundary } from '../components/RequestBoundary';
import './EditorPage.css';

export const EditorPage: React.FC = () => {
  const { slug } = useParams<{ slug?: string }>();
  const navigate = useNavigate();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [body, setBody] = useState('');
  const [tagInput, setTagInput] = useState('');
  const [tags, setTags] = useState<string[]>([]);

  // API call for loading an existing article
  const loadArticleApi = useCallback(async () => {
    if (!slug) return null;
    return articlesApi.getArticle(slug);
  }, [slug]);

  const { 
    data: articleData, 
    error: loadError, 
    loading: loadingArticle, 
    execute: loadArticle,
    clearError: clearLoadError 
  } = useApiCall(loadArticleApi);

  // Populate form when article data is loaded
  useEffect(() => {
    if (articleData?.article) {
      setTitle(articleData.article.title);
      setDescription(articleData.article.description);
      setBody(articleData.article.body);
      setTags(articleData.article.tagList);
    }
  }, [articleData]);

  useEffect(() => {
    if (slug) {
      loadArticle();
    }
  }, [slug, loadArticle]);

  // API call for submitting the article
  const submitArticleApi = useCallback(async () => {
    const articleData = {
      title,
      description,
      body,
      tagList: tags,
    };

    return slug
      ? articlesApi.updateArticle(slug, articleData)
      : articlesApi.createArticle(articleData);
  }, [slug, title, description, body, tags]);

  const { 
    error: submitError, 
    loading: submitting, 
    execute: submitArticle,
    clearError: clearSubmitError 
  } = useApiCall(submitArticleApi, {
    onSuccess: (response) => navigate(`/article/${response.article.slug}`),
  });

  // Combine errors for display
  const error = submitError || loadError;
  const clearError = () => {
    clearSubmitError();
    clearLoadError();
  };

  const handleAddTag = () => {
    const tag = tagInput.trim();
    if (tag && !tags.includes(tag)) {
      setTags([...tags, tag]);
      setTagInput('');
    }
  };

  const handleRemoveTag = (tagToRemove: string) => {
    setTags(tags.filter(tag => tag !== tagToRemove));
  };

  const handleTagKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleAddTag();
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await submitArticle();
  };

  return (
    <PageShell
      className="editor-page"
      title={slug ? 'Edit Article' : 'New Article'}
      columnLayout="wide"
    >
      <RequestBoundary
        error={error}
        clearError={clearError}
        loading={loadingArticle}
        loadingMessage="Loading article..."
      >
        <Form onSubmit={handleSubmit}>
          <Stack gap={6}>
            <TextInput
              id="title"
              labelText=""
              placeholder="Article Title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />

            <TextInput
              id="description"
              labelText=""
              placeholder="What's this article about?"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              required
            />

            <TextArea
              id="body"
              labelText=""
              placeholder="Write your article (in markdown)"
              value={body}
              onChange={(e) => setBody(e.target.value)}
              required
              rows={8}
            />

            <div>
              <TextInput
                id="tags"
                labelText=""
                placeholder="Enter tags"
                value={tagInput}
                onChange={(e) => setTagInput(e.target.value)}
                onKeyPress={handleTagKeyPress}
                onBlur={handleAddTag}
              />
              <div className="tag-list">
                {tags.map(tag => (
                  <Tag
                    key={tag}
                    filter
                    onClose={() => handleRemoveTag(tag)}
                  >
                    {tag}
                  </Tag>
                ))}
              </div>
            </div>

            <Button
              type="submit"
              disabled={submitting || !title || !description || !body}
              size="lg"
              className="pull-xs-right"
            >
              {submitting ? 'Publishing...' : 'Publish Article'}
            </Button>
          </Stack>
        </Form>
      </RequestBoundary>
    </PageShell>
  );
};
