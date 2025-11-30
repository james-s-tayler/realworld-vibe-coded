import React, { useState, useEffect, useCallback } from 'react';
import { Tabs, TabList, Tab, TabPanels, TabPanel, Tile, Pagination } from '@carbon/react';
import { useAuth } from '../hooks/useAuth';
import { useRequireAuth } from '../hooks/useRequireAuth';
import { useToast } from '../hooks/useToast';
import { articlesApi } from '../api/articles';
import { tagsApi } from '../api/tags';
import { ArticleList } from '../components/ArticleList';
import { TagList } from '../components/TagList';
import { PageShell } from '../components/PageShell';
import { normalizeError } from '../utils/errors';
import type { Article } from '../types/article';
import './HomePage.css';

const DEFAULT_PAGE_SIZE = 20;
const PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

export const HomePage: React.FC = () => {
  const { user } = useAuth();
  const { requireAuth } = useRequireAuth();
  const { showError } = useToast();
  const [articles, setArticles] = useState<Article[]>([]);
  const [articlesCount, setArticlesCount] = useState(0);
  const [tags, setTags] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [tagsLoading, setTagsLoading] = useState(false);
  // Default to "Your Feed" (index 0) for authenticated users, "Global Feed" (index 0) for unauthenticated
  // Since unauthenticated users don't have "Your Feed" tab, index 0 is always correct initially
  const [activeTab, setActiveTab] = useState(0);
  const [selectedTag, setSelectedTag] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

  const loadTags = useCallback(async () => {
    setTagsLoading(true);
    try {
      const response = await tagsApi.getTags();
      setTags(response.tags);
    } catch (error) {
      console.error('Failed to load tags:', error);
    } finally {
      setTagsLoading(false);
    }
  }, []);

  const loadArticles = useCallback(async () => {
    setLoading(true);
    try {
      let response;
      const offset = (currentPage - 1) * pageSize;
      if (activeTab === 0 && user) {
        // Your Feed
        response = await articlesApi.getFeed(pageSize, offset);
      } else if (selectedTag) {
        // Articles by Tag
        response = await articlesApi.listArticles({ tag: selectedTag, limit: pageSize, offset });
      } else {
        // Global Feed
        response = await articlesApi.listArticles({ limit: pageSize, offset });
      }
      setArticles(response.articles);
      setArticlesCount(response.articlesCount);
    } catch (error) {
      console.error('Failed to load articles:', error);
      showError(normalizeError(error));
    } finally {
      setLoading(false);
    }
  }, [activeTab, selectedTag, user, currentPage, pageSize, showError]);

  useEffect(() => {
    loadTags();
  }, [loadTags]);

  useEffect(() => {
    loadArticles();
  }, [loadArticles]);

  const handleFavorite = async (slug: string) => {
    try {
      await requireAuth(async () => {
        const response = await articlesApi.favoriteArticle(slug);
        setArticles(articles.map(a => a.slug === slug ? response.article : a));
        return response;
      });
    } catch (error) {
      console.error('Failed to favorite article:', error);
    }
  };

  const handleUnfavorite = async (slug: string) => {
    try {
      await requireAuth(async () => {
        const response = await articlesApi.unfavoriteArticle(slug);
        setArticles(articles.map(a => a.slug === slug ? response.article : a));
        return response;
      });
    } catch (error) {
      console.error('Failed to unfavorite article:', error);
    }
  };

  const handleTagClick = (tag: string) => {
    setSelectedTag(tag);
    setActiveTab(user ? 2 : 1);
    setCurrentPage(1);
  };

  const handleTabChange = (evt: { selectedIndex: number }) => {
    setActiveTab(evt.selectedIndex);
    setSelectedTag(null);
    setCurrentPage(1);
  };

  const handlePageChange = ({ page, pageSize: newPageSize }: { page: number; pageSize: number }) => {
    if (newPageSize !== pageSize) {
      setPageSize(newPageSize);
      setCurrentPage(1);
    } else {
      setCurrentPage(page);
    }
  };

  const banner = (
    <div className="banner">
      <div className="container">
        <h1 className="banner-title">conduit</h1>
        <p className="banner-subtitle">A place to share your <i>Angular</i> knowledge.</p>
      </div>
    </div>
  );

  const sidebar = (
    <Tile className="sidebar">
      <p className="sidebar-title">Popular Tags</p>
      <TagList tags={tags} loading={tagsLoading} onTagClick={handleTagClick} />
    </Tile>
  );

  return (
    <PageShell className="home-page" banner={banner} sidebar={sidebar} columnLayout="two-column">
      {user ? (
        <Tabs selectedIndex={activeTab} onChange={handleTabChange}>
          <TabList aria-label="Article feeds">
            <Tab>Your Feed</Tab>
            <Tab>Global Feed</Tab>
            {selectedTag && <Tab>#{selectedTag}</Tab>}
          </TabList>
          <TabPanels>
            <TabPanel>
              <ArticleList
                articles={articles}
                loading={loading}
                onFavorite={handleFavorite}
                onUnfavorite={handleUnfavorite}
              />
              {articlesCount > 0 && (
                <Pagination
                  page={currentPage}
                  pageSize={pageSize}
                  pageSizes={PAGE_SIZE_OPTIONS}
                  totalItems={articlesCount}
                  onChange={handlePageChange}
                />
              )}
            </TabPanel>
            <TabPanel>
              <ArticleList
                articles={articles}
                loading={loading}
                onFavorite={handleFavorite}
                onUnfavorite={handleUnfavorite}
              />
              {articlesCount > 0 && (
                <Pagination
                  page={currentPage}
                  pageSize={pageSize}
                  pageSizes={PAGE_SIZE_OPTIONS}
                  totalItems={articlesCount}
                  onChange={handlePageChange}
                />
              )}
            </TabPanel>
            {selectedTag && (
              <TabPanel>
                <ArticleList
                  articles={articles}
                  loading={loading}
                  onFavorite={handleFavorite}
                  onUnfavorite={handleUnfavorite}
                />
                {articlesCount > 0 && (
                  <Pagination
                    page={currentPage}
                    pageSize={pageSize}
                    pageSizes={PAGE_SIZE_OPTIONS}
                    totalItems={articlesCount}
                    onChange={handlePageChange}
                  />
                )}
              </TabPanel>
            )}
          </TabPanels>
        </Tabs>
      ) : (
        <Tabs selectedIndex={activeTab} onChange={handleTabChange}>
          <TabList aria-label="Article feeds">
            <Tab>Global Feed</Tab>
            {selectedTag && <Tab>#{selectedTag}</Tab>}
          </TabList>
          <TabPanels>
            <TabPanel>
              <ArticleList
                articles={articles}
                loading={loading}
                onFavorite={handleFavorite}
                onUnfavorite={handleUnfavorite}
              />
              {articlesCount > 0 && (
                <Pagination
                  page={currentPage}
                  pageSize={pageSize}
                  pageSizes={PAGE_SIZE_OPTIONS}
                  totalItems={articlesCount}
                  onChange={handlePageChange}
                />
              )}
            </TabPanel>
            {selectedTag && (
              <TabPanel>
                <ArticleList
                  articles={articles}
                  loading={loading}
                  onFavorite={handleFavorite}
                  onUnfavorite={handleUnfavorite}
                />
                {articlesCount > 0 && (
                  <Pagination
                    page={currentPage}
                    pageSize={pageSize}
                    pageSizes={PAGE_SIZE_OPTIONS}
                    totalItems={articlesCount}
                    onChange={handlePageChange}
                  />
                )}
              </TabPanel>
            )}
          </TabPanels>
        </Tabs>
      )}
    </PageShell>
  );
};
