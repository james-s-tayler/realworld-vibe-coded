import React, { useState, useEffect, useCallback } from 'react';
import { Grid, Column, Tabs, TabList, Tab, TabPanels, TabPanel, Tile, Pagination } from '@carbon/react';
import { useToast } from '../hooks/useToast';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../hooks/useAuth';
import { useRequireAuth } from '../hooks/useRequireAuth';
import { articlesApi } from '../api/articles';
import { tagsApi } from '../api/tags';
import { ArticleList } from '../components/ArticleList';
import { TagList } from '../components/TagList';
import { PageShell } from '../components/PageShell';
import { ApiError } from '../api/client';
import type { Article } from '../types/article';
import './HomePage.css';

const DEFAULT_PAGE_SIZE = 20;
const PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

const HomeBanner: React.FC = () => {
  const { t } = useTranslation();
  return (
    <div className="banner">
      <Grid>
        <Column lg={16} md={8} sm={4}>
          <h1 className="banner-title">{t('home.bannerTitle')}</h1>
          <p className="banner-subtitle">{t('home.bannerSubtitle')}</p>
        </Column>
      </Grid>
    </div>
  );
};

export const HomePage: React.FC = () => {
  const { t } = useTranslation();
  const { user } = useAuth();
  const { requireAuth } = useRequireAuth();
  const { showToast } = useToast();
  const [articles, setArticles] = useState<Article[]>([]);
  const [articlesCount, setArticlesCount] = useState(0);
  const [tags, setTags] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [tagsLoading, setTagsLoading] = useState(false);
  // Default to "Your Feed" (index 0)
  const [activeTab, setActiveTab] = useState(0);
  const [selectedTag, setSelectedTag] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

  const loadTags = useCallback(async () => {
    setTagsLoading(true);
    try {
      const response = await tagsApi.getTags();
      setTags(response.tags);
    } catch (err) {
      if (err instanceof ApiError) {
        showToast({ kind: 'error', title: t('error.title'), subtitle: err.errors.join(', ') });
      } else {
        showToast({ kind: 'error', title: t('error.title'), subtitle: t('home.failedToLoadTags') });
      }
    } finally {
      setTagsLoading(false);
    }
  }, [t, showToast]);

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
    } catch (err) {
      if (err instanceof ApiError) {
        showToast({ kind: 'error', title: t('error.title'), subtitle: err.errors.join(', ') });
      } else {
        showToast({ kind: 'error', title: t('error.title'), subtitle: t('home.failedToLoadTags') });
      }
    } finally {
      setLoading(false);
    }
  }, [activeTab, selectedTag, user, currentPage, pageSize, t, showToast]);

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
    } catch (err) {
      if (err instanceof ApiError) {
        showToast({ kind: 'error', title: t('error.title'), subtitle: err.errors.join(', ') });
      } else {
        showToast({ kind: 'error', title: t('error.title'), subtitle: t('home.failedToFavorite') });
      }
    }
  };

  const handleUnfavorite = async (slug: string) => {
    try {
      await requireAuth(async () => {
        const response = await articlesApi.unfavoriteArticle(slug);
        setArticles(articles.map(a => a.slug === slug ? response.article : a));
        return response;
      });
    } catch (err) {
      if (err instanceof ApiError) {
        showToast({ kind: 'error', title: t('error.title'), subtitle: err.errors.join(', ') });
      } else {
        showToast({ kind: 'error', title: t('error.title'), subtitle: t('home.failedToUnfavorite') });
      }
    }
  };

  const handleTagClick = (tag: string) => {
    setSelectedTag(tag);
    setActiveTab(2);
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

  const sidebarContent = (
    <Tile className="sidebar">
      <p className="sidebar-title">{t('home.popularTags')}</p>
      <TagList tags={tags} loading={tagsLoading} onTagClick={handleTagClick} />
    </Tile>
  );

  return (
    <PageShell
      className="home-page"
      columnLayout="two-column"
      banner={<HomeBanner />}
      sidebar={sidebarContent}
    >
      <Tabs selectedIndex={activeTab} onChange={handleTabChange}>
        <TabList aria-label={t('home.articleFeeds')}>
          <Tab>{t('home.yourFeed')}</Tab>
          <Tab>{t('home.globalFeed')}</Tab>
          {selectedTag && <Tab>#{selectedTag}</Tab>}
        </TabList>
        <TabPanels>
          {[0, 1, ...(selectedTag ? [2] : [])].map((tabIndex) => (
            <TabPanel key={tabIndex}>
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
          ))}
        </TabPanels>
      </Tabs>
    </PageShell>
  );
};
