import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router';
import {
  Form,
  TextInput,
  TextArea,
  Button,
  InlineNotification,
  Stack,
  Tag,
} from '@carbon/react';
import { articlesApi } from '../api/articles';
import { ApiError } from '../api/client';
import './EditorPage.css';

export const EditorPage: React.FC = () => {
  const { slug } = useParams<{ slug?: string }>();
  const navigate = useNavigate();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [body, setBody] = useState('');
  const [tagInput, setTagInput] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [loadingArticle, setLoadingArticle] = useState(false);

  const loadArticle = useCallback(async () => {
    if (!slug) return;
    setLoadingArticle(true);
    try {
      const response = await articlesApi.getArticle(slug);
      const { article } = response;
      setTitle(article.title);
      setDescription(article.description);
      setBody(article.body);
      setTags(article.tagList);
    } catch (error) {
      console.error('Failed to load article:', error);
      setError('Failed to load article for editing');
    } finally {
      setLoadingArticle(false);
    }
  }, [slug]);

  useEffect(() => {
    if (slug) {
      loadArticle();
    }
  }, [slug, loadArticle]);

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
    setError(null);
    setLoading(true);

    try {
      const articleData = {
        title,
        description,
        body,
        tagList: tags,
      };

      const response = slug
        ? await articlesApi.updateArticle(slug, articleData)
        : await articlesApi.createArticle(articleData);

      navigate(`/article/${response.article.slug}`);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors.join(', '));
      } else {
        setError('An unexpected error occurred');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="editor-page">
      <div className="container page">
        <div className="row">
          <div className="col-md-10 offset-md-1 col-xs-12">
            <h1 className="text-xs-center">{slug ? 'Edit Article' : 'New Article'}</h1>

            {error && (
              <InlineNotification
                kind="error"
                title="Error"
                subtitle={error}
                onCloseButtonClick={() => setError(null)}
                style={{ marginBottom: '1rem' }}
              />
            )}

            {loadingArticle ? (
              <p>Loading article...</p>
            ) : (
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
                    disabled={loading || !title || !description || !body}
                    size="lg"
                    className="pull-xs-right"
                  >
                    {loading ? 'Publishing...' : 'Publish Article'}
                  </Button>
                </Stack>
              </Form>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
